using System.Net;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>F2: GET /clubos/me/venues lists the caller's venues (any status) with their membership role. All data is private to each test.</summary>
[Collection(ApiCollection.Name)]
public class MyVenuesTests(ApiFactory api)
{
    private const string Url = "/api/v1/clubos/me/venues";

    private async Task<Guid> NewVenueAsync(string nameEn, string status = "hidden", string nameTh = "ทดสอบ")
    {
        var p = Guid.NewGuid();
        var v = Guid.NewGuid();
        var slug = "mine-" + v.ToString("N")[..12];
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO partners (id, legal_name, status) VALUES ({p}, 'MyVenues Partner', 'active')");
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({v}, {p}, {slug}, {nameTh}, {nameEn}, 'T', 'T', 14.6, 101.3, {status})
                """);
        });
        return v;
    }

    private Task JoinAsync(TestUser u, Guid venue, string role) => api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
        $"INSERT INTO venue_members (venue_id, user_id, role) VALUES ({venue}, {u.Id}, {role})"));

    private async Task<JsonElement> ListAsync(HttpClient http)
    {
        var res = await http.GetAsync(Url);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK,
            $"expected 200 but got {(int)res.StatusCode}: {raw}\n{string.Join("\n", api.ServerErrors.TakeLast(2))}");
        return JsonDocument.Parse(raw).RootElement;
    }

    [Fact]
    public async Task Anonymous_gets_401_json()
    {
        var res = await api.Anonymous().GetAsync(Url);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);
        Assert.True(JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement.TryGetProperty("code", out _));
    }

    [Fact]
    public async Task Player_gets_403()
    {
        var player = await api.LoginAsync();
        var res = await player.Http.GetAsync(Url);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Partner_without_membership_gets_empty_list()
    {
        var nobody = await api.LoginPartnerAsync(venueId: null);
        var list = await ListAsync(nobody.Http);
        Assert.Equal(JsonValueKind.Array, list.ValueKind);
        Assert.Equal(0, list.GetArrayLength());
    }

    [Fact]
    public async Task Returns_only_own_venues_with_full_shape_and_role()
    {
        var mine = await NewVenueAsync("Mine Alpha", nameTh: "ของฉัน");
        var others = await NewVenueAsync("Somebody Else");
        var owner = await api.LoginPartnerAsync(mine, "owner", "partner_owner");
        var stranger = await api.LoginPartnerAsync(others, "staff");

        var list = await ListAsync(owner.Http);
        Assert.Equal(1, list.GetArrayLength());
        var v = list[0];
        Assert.Equal(mine, v.GetProperty("id").GetGuid());
        Assert.StartsWith("mine-", v.GetProperty("slug").GetString());
        Assert.Equal("ของฉัน", v.GetProperty("nameTh").GetString());
        Assert.Equal("Mine Alpha", v.GetProperty("nameEn").GetString());
        Assert.Equal("hidden", v.GetProperty("status").GetString());
        Assert.Equal("owner", v.GetProperty("role").GetString());

        var theirs = await ListAsync(stranger.Http);
        Assert.Equal(1, theirs.GetArrayLength());
        Assert.Equal(others, theirs[0].GetProperty("id").GetGuid());
        Assert.Equal("staff", theirs[0].GetProperty("role").GetString());
    }

    [Fact]
    public async Task Multi_venue_member_gets_every_venue_with_the_role_of_each_membership_ordered_by_english_name()
    {
        var tag = Guid.NewGuid().ToString("N")[..6];
        var b = await NewVenueAsync($"Bravo {tag}");
        var a = await NewVenueAsync($"Alpha {tag}");
        var c = await NewVenueAsync($"Charlie {tag}");
        var notMine = await NewVenueAsync($"Aardvark {tag}");
        var u = await api.LoginPartnerAsync(venueId: null, role: "partner_owner");
        await JoinAsync(u, b, "staff");
        await JoinAsync(u, c, "owner");
        await JoinAsync(u, a, "owner");

        var list = await ListAsync(u.Http);
        Assert.Equal(new List<Guid> { a, b, c }, list.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToList());
        Assert.Equal(new List<string?> { "owner", "staff", "owner" }, list.EnumerateArray().Select(x => x.GetProperty("role").GetString()).ToList());
        Assert.DoesNotContain(list.EnumerateArray(), x => x.GetProperty("id").GetGuid() == notMine);
    }

    [Theory]
    [InlineData("draft")]
    [InlineData("hidden")]
    [InlineData("active")]
    public async Task Venues_of_any_status_are_included(string status)
    {
        var v = await NewVenueAsync("Status " + status, status);
        var u = await api.LoginPartnerAsync(v, "owner", "partner_owner");
        try
        {
            var list = await ListAsync(u.Http);
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal(status, list[0].GetProperty("status").GetString());
        }
        finally
        {
            // keep the shared DB free of public venues so venue-search tests are unaffected
            await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE venues SET status = 'hidden' WHERE id = {v}"));
        }
    }

    [Fact]
    public async Task Membership_changes_show_up_on_the_next_call_without_re_login()
    {
        var v = await NewVenueAsync("Late Join");
        var u = await api.LoginPartnerAsync(venueId: null);
        Assert.Equal(0, (await ListAsync(u.Http)).GetArrayLength());
        await JoinAsync(u, v, "staff");
        Assert.Equal(1, (await ListAsync(u.Http)).GetArrayLength());
    }
}
