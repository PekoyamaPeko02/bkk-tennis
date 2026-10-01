using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

/// <summary>P1: /favorites. Users, private partner/venue and favourite rows created here are removed in DisposeAsync.</summary>
[Collection(ApiCollection.Name)]
public class FavoritesTests(ApiFactory api) : IAsyncLifetime
{
    private const string Fav = "/api/v1/favorites";
    private static readonly Guid Racquet = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid Benchasiri = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid Asoke = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid Ari = Guid.Parse("20000000-0000-0000-0000-000000000004");

    private readonly List<Guid> _users = [], _partners = [];
    private readonly int _errorsBefore = api.ServerErrors.Count;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        try
        {
            Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));
        }
        finally
        {
            await api.WithDb(async db =>
            {
                foreach (var u in _users) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u}");
                foreach (var p in _partners)
                {
                    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
                    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
                }
            });
        }
    }

    private async Task<TestUser> NewUserAsync()
    {
        var u = await api.LoginAsync();
        _users.Add(u.Id);
        return u;
    }

    private static async Task<JsonElement[]> ListAsync(HttpClient http)
    {
        var res = await http.GetAsync(Fav);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToArray();
    }

    private static void Same(Guid[] e, Guid[] a) => Assert.Equal<IEnumerable<Guid>>(e, a);

    private static Guid[] Ids(IEnumerable<JsonElement> l) => l.Select(i => i.GetProperty("id").GetGuid()).ToArray();

    private static async Task PutOk(HttpClient http, Guid v) => Assert.Equal(HttpStatusCode.NoContent, (await http.PutAsync($"{Fav}/{v}", null)).StatusCode);

    private Task<int> RowCount(Guid user, Guid? venue = null) => api.WithScope(async sp =>
        await sp.GetRequiredService<AppDbContext>().Favorites.CountAsync(f => f.UserId == user && (venue == null || f.VenueId == venue)));

    private Task<List<DateTime>> Stamps(Guid user, Guid? venue = null) => api.WithScope(async sp =>
        await sp.GetRequiredService<AppDbContext>().Favorites.Where(f => f.UserId == user && (venue == null || f.VenueId == venue))
            .Select(f => f.CreatedAt).ToListAsync());

    private async Task<(Guid Partner, Guid Venue)> PrivateVenueAsync(string partnerStatus = "active", string venueStatus = "active")
    {
        var p = Guid.NewGuid();
        var v = Guid.NewGuid();
        var slug = "fav-" + v.ToString("N")[..10];
        _partners.Add(p);
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO partners (id, legal_name, status) VALUES ({p}, 'Fav Partner', {partnerStatus})");
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({v}, {p}, {slug}, 'ก', 'Fav Venue', 'T', 'T', 14.6, 101.3, {venueStatus})
                """);
        });
        return (p, v);
    }

    private Task SetPartnerStatus(Guid p, string s) => api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE partners SET status = {s} WHERE id = {p}"));
    private Task SetVenueStatus(Guid v, string s) => api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE venues SET status = {s} WHERE id = {v}"));

    // ------------------------------------------------------------------ tests

    [Fact]
    public async Task Add_list_remove_round_trip()
    {
        var u = await NewUserAsync();
        Assert.Empty(await ListAsync(u.Http));

        await PutOk(u.Http, Racquet);
        Same([Racquet], Ids(await ListAsync(u.Http)));

        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{Racquet}")).StatusCode);
        Assert.Empty(await ListAsync(u.Http));
        Assert.Equal(0, await RowCount(u.Id));
    }

    [Fact]
    public async Task List_is_newest_favourite_first_and_rapid_puts_never_tie()
    {
        var u = await NewUserAsync();
        Guid[] order = [Ari, Racquet, Asoke, Benchasiri];
        foreach (var v in order) await PutOk(u.Http, v); // back to back, no delay

        Assert.Equal(order.Reverse().ToArray(), Ids(await ListAsync(u.Http)));
        Assert.Equal(4, (await Stamps(u.Id)).Distinct().Count());
    }

    [Fact]
    public async Task List_orders_by_created_at_even_when_inserted_out_of_order()
    {
        var u = await NewUserAsync();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO favorites (user_id, venue_id, created_at) VALUES
             ({u.Id}, {Asoke}, now() - interval '1 hour'), ({u.Id}, {Racquet}, now() - interval '3 hours'), ({u.Id}, {Ari}, now() - interval '2 hours')
            """));
        Same([Asoke, Ari, Racquet], Ids(await ListAsync(u.Http)));
    }

    [Fact]
    public async Task Put_is_idempotent_and_keeps_original_created_at()
    {
        var u = await NewUserAsync();
        await PutOk(u.Http, Racquet);
        await PutOk(u.Http, Benchasiri);
        var before = await Stamps(u.Id, Racquet);

        await PutOk(u.Http, Racquet); // again

        Assert.Equal(1, await RowCount(u.Id, Racquet));
        Assert.Equal(before, await Stamps(u.Id, Racquet));
        Same([Benchasiri, Racquet], Ids(await ListAsync(u.Http))); // did not jump to the top
    }

    [Fact]
    public async Task Delete_is_idempotent_and_204_for_unknown_venues()
    {
        var u = await NewUserAsync();
        await PutOk(u.Http, Racquet);
        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{Racquet}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{Racquet}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{Ari}")).StatusCode); // never favourited
        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{Guid.NewGuid()}")).StatusCode); // does not exist
    }

    [Fact]
    public async Task Users_are_isolated_and_delete_only_touches_own_row()
    {
        var a = await NewUserAsync();
        var b = await NewUserAsync();
        await PutOk(a.Http, Racquet);
        await PutOk(a.Http, Ari);
        await PutOk(b.Http, Racquet);

        Same([Racquet], Ids(await ListAsync(b.Http)));
        Assert.Equal(2, (await ListAsync(a.Http)).Length);

        Assert.Equal(HttpStatusCode.NoContent, (await b.Http.DeleteAsync($"{Fav}/{Ari}")).StatusCode); // b never had Ari
        Assert.Equal(2, await RowCount(a.Id));

        Assert.Equal(HttpStatusCode.NoContent, (await b.Http.DeleteAsync($"{Fav}/{Racquet}")).StatusCode);
        Assert.Empty(await ListAsync(b.Http));
        Same([Ari, Racquet], Ids(await ListAsync(a.Http)));
    }

    [Fact]
    public async Task Anonymous_gets_401_on_all_three()
    {
        var http = api.Anonymous();
        var r1 = await http.GetAsync(Fav);
        var r2 = await http.PutAsync($"{Fav}/{Racquet}", null);
        var r3 = await http.DeleteAsync($"{Fav}/{Racquet}");
        Assert.All(new[] { r1, r2, r3 }, r => Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode));
        Assert.Equal(JsonValueKind.String, (await r1.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").ValueKind);
    }

    [Fact]
    public async Task Put_404_for_unknown_venue_and_non_guid_and_writes_nothing()
    {
        var u = await NewUserAsync();
        var unknown = await u.Http.PutAsync($"{Fav}/{Guid.NewGuid()}", null);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(JsonValueKind.String, (await unknown.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").ValueKind);
        Assert.Equal(HttpStatusCode.NotFound, (await u.Http.PutAsync($"{Fav}/not-a-guid", null)).StatusCode);
        Assert.Equal(0, await RowCount(u.Id));
    }

    [Theory]
    [InlineData("active", "draft")]
    [InlineData("active", "hidden")]
    [InlineData("pending", "active")]
    [InlineData("suspended", "active")]
    public async Task Non_public_venue_cannot_be_favourited(string partnerStatus, string venueStatus)
    {
        var u = await NewUserAsync();
        var (_, v) = await PrivateVenueAsync(partnerStatus, venueStatus);

        Assert.Equal(HttpStatusCode.NotFound, (await u.Http.PutAsync($"{Fav}/{v}", null)).StatusCode);
        Assert.Equal(0, await RowCount(u.Id));
    }

    [Fact]
    public async Task Suspending_partner_hides_favourite_keeps_row_and_reapproval_brings_it_back()
    {
        var u = await NewUserAsync();
        var (p, v) = await PrivateVenueAsync();
        await PutOk(u.Http, Racquet);
        await PutOk(u.Http, v);
        Same([v, Racquet], Ids(await ListAsync(u.Http)));

        await SetPartnerStatus(p, "suspended");
        Same([Racquet], Ids(await ListAsync(u.Http)));
        Assert.Equal(1, await RowCount(u.Id, v));
        Assert.Equal(HttpStatusCode.NotFound, (await u.Http.PutAsync($"{Fav}/{v}", null)).StatusCode);

        await SetPartnerStatus(p, "active");
        Same([v, Racquet], Ids(await ListAsync(u.Http)));
    }

    [Fact]
    public async Task Draft_venue_hides_favourite_and_can_still_be_unfavourited()
    {
        var u = await NewUserAsync();
        var (_, v) = await PrivateVenueAsync();
        await PutOk(u.Http, v);

        await SetVenueStatus(v, "draft");
        Assert.Empty(await ListAsync(u.Http));
        Assert.Equal(HttpStatusCode.NotFound, (await u.Http.PutAsync($"{Fav}/{v}", null)).StatusCode);
        Assert.Equal(1, await RowCount(u.Id, v));
        Assert.Equal(HttpStatusCode.NoContent, (await u.Http.DeleteAsync($"{Fav}/{v}")).StatusCode);
        Assert.Equal(0, await RowCount(u.Id, v));
    }

    [Fact]
    public async Task Hidden_venue_row_reappears_when_active_again()
    {
        var u = await NewUserAsync();
        var (_, v) = await PrivateVenueAsync();
        await PutOk(u.Http, v);
        await SetVenueStatus(v, "hidden");
        Assert.Empty(await ListAsync(u.Http));
        await SetVenueStatus(v, "active");
        Same([v], Ids(await ListAsync(u.Http)));
    }

    [Fact]
    public async Task Deleting_the_user_or_the_venue_cascades()
    {
        var u = await NewUserAsync();
        var (_, v) = await PrivateVenueAsync();
        var other = await NewUserAsync();
        await PutOk(u.Http, v);
        await PutOk(u.Http, Racquet);
        await PutOk(other.Http, v);

        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE id = {v}"));
        Assert.Equal(0, await api.WithScope(async sp => await sp.GetRequiredService<AppDbContext>().Favorites.CountAsync(f => f.VenueId == v)));
        Same([Racquet], Ids(await ListAsync(u.Http)));

        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u.Id}"));
        Assert.Equal(0, await RowCount(u.Id));
        Assert.Equal(0, await RowCount(other.Id));
    }

    [Fact]
    public async Task Parallel_puts_of_the_same_venue_create_one_row_and_no_500()
    {
        var u = await NewUserAsync();
        var res = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => u.Http.PutAsync($"{Fav}/{Racquet}", null))));

        Assert.All(res, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        Assert.Equal(1, await RowCount(u.Id, Racquet));
    }

    [Fact]
    public async Task Payload_matches_VenueSummary_field_names()
    {
        var u = await NewUserAsync();
        await PutOk(u.Http, Racquet);
        var item = (await ListAsync(u.Http)).Single();

        string[] expected =
        [
            "id", "slug", "nameTh", "nameEn", "district", "address", "lat", "lng", "distanceKm", "transitNote", "rating", "ratingCount",
            "amenities", "surfaces", "hasIndoor", "hasAirCon", "courtCount", "priceFrom", "priceTo", "nextFreeTimes", "freeSlotCount", "coverImageUrl",
        ];
        Assert.Equal(expected.Order(), item.EnumerateObject().Select(p => p.Name).Order());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("distanceKm").ValueKind);
        Assert.Equal(0, item.GetProperty("freeSlotCount").GetInt32());
        Assert.Empty(item.GetProperty("nextFreeTimes").EnumerateArray());
        Assert.True(item.GetProperty("courtCount").GetInt32() > 0);
        Assert.Equal("racquet-club-sukhumvit-49", item.GetProperty("slug").GetString());
    }
}
