using System.Net;
using System.Text;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// Who may call /clubos/venues/{venueId}/…: any venue member (read + walk-ins + blocks), only the venue owner for price
/// writes, never players or members of other venues. These are the guard rails for the authorization policy.
/// </summary>
[Collection(ApiCollection.Name)]
public class ClubOsAuthorizationTests(ApiFactory api)
{
    private static readonly Guid Racquet = Seed.VenueId;
    private static readonly Guid Benchasiri = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid Rule = Guid.NewGuid(); // never exists: authorization must answer before "not found"

    private static string Base(Guid venue) => $"/api/v1/clubos/venues/{venue}";
    private static StringContent Json(string raw) => new(raw, Encoding.UTF8, "application/json");
    private const string Day = "2030-01-01";

    private static async Task AssertErrorAsync(HttpResponseMessage res, HttpStatusCode status, string code)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} but got {(int)res.StatusCode}: {raw}");
        Assert.Equal(code, JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
    }

    // Passes DTO validation but fails the domain rule (bad_day_type) — so an authorized caller reaches the handler
    // WITHOUT persisting a price rule on the shared seed venue (which would change other tests' prices).
    private static string PriceBody => """{ "dayType": "holiday", "start": "03:00", "end": "04:00", "pricePerHour": 100, "priority": 5 }""";

    /// <summary>Every operation a venue's partner portal has, tagged with whether it is owner-only.</summary>
    public static IEnumerable<object[]> Operations() =>
    [
        ["GET", "/availability?date=" + Day, false],
        ["GET", "/schedule?date=" + Day, false],
        ["GET", "/bookings?date=" + Day, false],
        ["GET", "/blocks?date=" + Day, false],
        ["GET", "/prices", false],
        ["POST", "/walk-ins", false],
        ["POST", "/blocks", false],
        ["POST", "/prices", true],
        ["PUT", "/prices/" + "{rule}", true],
        ["DELETE", "/prices/" + "{rule}", true],
    ];

    private static Task<HttpResponseMessage> CallAsync(HttpClient http, Guid venue, string method, string path)
    {
        var req = new HttpRequestMessage(new HttpMethod(method), Base(venue) + path.Replace("{rule}", Rule.ToString()));
        // An invalid/empty body on purpose: authorization must be decided before any body validation.
        if (method is "POST" or "PUT") req.Content = Json(path.StartsWith("/prices") ? PriceBody : "{}");
        return http.SendAsync(req);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Anonymous_callers_get_401(string method, string path, bool ownerOnly)
    {
        _ = ownerOnly;
        await AssertErrorAsync(await CallAsync(api.Anonymous(), Racquet, method, path), HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Players_get_403(string method, string path, bool ownerOnly)
    {
        _ = ownerOnly;
        var player = await api.LoginAsync();
        await AssertErrorAsync(await CallAsync(player.Http, Racquet, method, path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Staff_of_another_venue_get_403(string method, string path, bool ownerOnly)
    {
        _ = ownerOnly;
        var other = await api.LoginPartnerAsync(Benchasiri);
        await AssertErrorAsync(await CallAsync(other.Http, Racquet, method, path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Partner_users_without_any_membership_get_403(string method, string path, bool ownerOnly)
    {
        _ = ownerOnly;
        var nobody = await api.LoginPartnerAsync(venueId: null);
        await AssertErrorAsync(await CallAsync(nobody.Http, Racquet, method, path), HttpStatusCode.Forbidden, "forbidden");
    }

    [Fact]
    public async Task A_venue_that_does_not_exist_is_403_not_404_so_ids_are_not_probeable()
    {
        var staff = await api.LoginPartnerAsync(Racquet);
        await AssertErrorAsync(await CallAsync(staff.Http, Guid.NewGuid(), "GET", "/bookings?date=" + Day), HttpStatusCode.Forbidden, "forbidden");
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Staff_members_may_do_everything_except_the_owner_only_operations(string method, string path, bool ownerOnly)
    {
        var staff = await api.LoginPartnerAsync(Racquet, memberRole: "staff");

        var res = await CallAsync(staff.Http, Racquet, method, path);

        if (ownerOnly) await AssertErrorAsync(res, HttpStatusCode.Forbidden, "forbidden");
        else Assert.NotEqual(HttpStatusCode.Forbidden, res.StatusCode); // reached the handler (it may still answer 400/404 for the dummy body)
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task The_owner_reaches_every_handler(string method, string path, bool ownerOnly)
    {
        _ = ownerOnly;
        var owner = await api.LoginPartnerAsync(Racquet, memberRole: "owner", role: "partner_owner");

        var res = await CallAsync(owner.Http, Racquet, method, path);

        Assert.NotEqual(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task A_member_reads_their_own_venue_and_cannot_read_another()
    {
        var staff = await api.LoginPartnerAsync(Benchasiri);

        Assert.Equal(HttpStatusCode.OK, (await staff.Http.GetAsync($"{Base(Benchasiri)}/bookings?date={Day}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.Http.GetAsync($"{Base(Racquet)}/bookings?date={Day}")).StatusCode);
    }

    [Fact]
    public async Task Membership_is_what_counts_not_only_the_role_claim()
    {
        // A partner_owner token without a venue_members row for this venue must not get in.
        var ownerElsewhere = await api.LoginPartnerAsync(Benchasiri, memberRole: "owner", role: "partner_owner");

        Assert.Equal(HttpStatusCode.Forbidden, (await ownerElsewhere.Http.GetAsync($"{Base(Racquet)}/prices")).StatusCode);
    }

    [Fact]
    public async Task Owner_membership_but_a_player_role_claim_is_still_forbidden()
    {
        // The role claim gates the portal even when a membership row exists.
        var user = await api.LoginPartnerAsync(Racquet, memberRole: "owner", role: "player");

        Assert.Equal(HttpStatusCode.Forbidden, (await user.Http.GetAsync($"{Base(Racquet)}/prices")).StatusCode);
    }
}
