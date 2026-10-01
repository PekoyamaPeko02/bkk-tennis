using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// A2: partner onboarding (/admin/partners). The DB is shared and other tests assert exact venue lists, so every partner,
/// owner user, venue and booking created here is removed again in <see cref="DisposeAsync"/> (runs even when a test fails).
/// </summary>
[Collection(ApiCollection.Name)]
public class AdminPartnersTests(ApiFactory api) : IAsyncLifetime
{
    private const string Partners = "/api/v1/admin/partners", StaffLogin = "/api/v1/auth/staff/login", AdminLogin = "/api/v1/auth/admin/login";
    private const string OwnerPw = "owner-pass-2026";
    // A spot no seeded venue is near: radius searches around it only ever return the venue under test.
    private const double Lat = 14.6, Lng = 101.3;

    private readonly List<Guid> _partners = [];

    private readonly int _errorsBefore = api.ServerErrors.Count;

    private void AssertNoServerErrors() =>
        Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => api.WithDb(async db =>
    {
        foreach (var p in _partners)
        {
            // owners created by the onboarding (never seeded users): remembered before the venue rows disappear
            var owners = await db.VenueMembers.Where(m => m.Role == "owner" && db.Venues.Any(v => v.Id == m.VenueId && v.PartnerId == p))
                .Select(m => m.UserId).ToListAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DELETE FROM refunds WHERE payment_id IN (SELECT pay.id FROM payments pay JOIN bookings b ON b.id = pay.booking_id
                                                         JOIN venues v ON v.id = b.venue_id WHERE v.partner_id = {p})
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DELETE FROM payments WHERE booking_id IN (SELECT b.id FROM bookings b JOIN venues v ON v.id = b.venue_id WHERE v.partner_id = {p})
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM bookings WHERE venue_id IN (SELECT id FROM venues WHERE partner_id = {p})");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
            foreach (var u in owners) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u} AND role = 'partner_owner'");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM payouts WHERE partner_id = {p}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
        }
    });

    // ------------------------------------------------------------------ helpers

    private static string Tag() => Guid.NewGuid().ToString("N")[..10];

    private async Task<HttpClient> AdminAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync(AdminLogin, new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private async Task<HttpClient> OwnerHttpAsync(string email, string password = OwnerPw)
    {
        var res = await api.Anonymous().PostAsJsonAsync(StaffLogin, new { email, password });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private sealed record Created(string Legal, string Email, string Slug, JsonObject Body);

    private Created NewRequest(string? email = null, string? slug = null, string? legal = null, Action<JsonObject>? tweak = null)
    {
        var t = Tag();
        legal ??= "Partner " + t;
        email ??= $"owner-{t}@example.test";
        slug ??= "p-" + t;
        var body = new JsonObject
        {
            ["legalName"] = legal, ["taxId"] = "0105551234567", ["bankName"] = "KBank", ["bankAccount"] = "123-4-56789-0",
            ["owner"] = new JsonObject { ["email"] = email, ["displayName"] = "Owner " + t, ["password"] = OwnerPw },
            ["venue"] = new JsonObject
            {
                ["slug"] = slug, ["nameTh"] = "สนามทดสอบ " + t, ["nameEn"] = "Test Venue " + t, ["district"] = "Test", ["address"] = "1 Test Rd",
                ["lat"] = Lat, ["lng"] = Lng,
            },
        };
        tweak?.Invoke(body);
        return new Created(legal, email, slug, body);
    }

    /// <summary>Sets a dotted path ("owner.password") to a raw JSON value; the literal REMOVE deletes the property.</summary>
    private static void Set(JsonObject root, string path, string json)
    {
        var parts = path.Split('.');
        var node = root;
        for (var i = 0; i < parts.Length - 1; i++) node = node[parts[i]]!.AsObject();
        if (json == "REMOVE") node.Remove(parts[^1]);
        else node[parts[^1]] = JsonNode.Parse(json);
    }

    private static Task<HttpResponseMessage> PostJsonAsync(HttpClient http, string url, JsonNode body) =>
        http.PostAsync(url, new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"));

    private async Task<JsonElement> CreateOkAsync(HttpClient admin, Created c)
    {
        var res = await PostJsonAsync(admin, Partners, c.Body);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.Created, raw + "\n" + string.Join("\n", api.ServerErrors));
        var dto = JsonDocument.Parse(raw).RootElement.Clone();
        _partners.Add(dto.GetProperty("id").GetGuid());
        return dto;
    }

    private async Task<JsonElement> ErrorAsync(HttpResponseMessage res, HttpStatusCode status, string code)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} got {(int)res.StatusCode}: {raw}");
        var body = JsonDocument.Parse(raw).RootElement.Clone();
        Assert.Equal(code, body.GetProperty("code").GetString());
        AssertNoServerErrors();
        return body;
    }

    private static Guid VenueId(JsonElement partner) => partner.GetProperty("venues")[0].GetProperty("id").GetGuid();

    private async Task<JsonElement> ActAsync(HttpClient admin, JsonElement partner, string action, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var res = await admin.PostAsync($"{Partners}/{partner.GetProperty("id").GetGuid()}/{action}", null);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == expected, $"{action}: expected {(int)expected} got {(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement.Clone();
    }

    private async Task<int> CountPartnersAsync(string legal)
    {
        var n = 0;
        await api.WithDb(async db => n = await db.Partners.CountAsync(p => p.LegalName == legal));
        return n;
    }

    private async Task<bool> UserExistsAsync(string email)
    {
        var e = false;
        await api.WithDb(async db => e = await db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant()));
        return e;
    }

    private async Task<bool> VenueSlugExistsAsync(string slug)
    {
        var e = false;
        await api.WithDb(async db => e = await db.Venues.AnyAsync(v => v.Slug == slug));
        return e;
    }

    private async Task<JsonElement[]> PartnerListAsync(HttpClient admin, string? status = null)
    {
        var res = await admin.GetAsync(Partners + (status is null ? "" : "?status=" + status));
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToArray();
    }

    private static string Path(Guid venue, string tail) => $"/api/v1/clubos/venues/{venue}/{tail}";

    /// <summary>Everything a public visitor can observe about a venue: search (plain + radius), detail by id/slug, availability.</summary>
    private sealed record Visibility(bool Search, bool Radius, HttpStatusCode ById, HttpStatusCode BySlug, HttpStatusCode Availability)
    {
        public bool AllVisible => Search && Radius && ById == HttpStatusCode.OK && BySlug == HttpStatusCode.OK && Availability == HttpStatusCode.OK;
        public bool NoneVisible => !Search && !Radius && ById == HttpStatusCode.NotFound && BySlug == HttpStatusCode.NotFound && Availability == HttpStatusCode.NotFound;
    }

    private async Task<Visibility> VisibilityAsync(Guid venueId, string slug, DateOnly date)
    {
        var http = api.Anonymous();
        async Task<bool> InSearch(string q)
        {
            var res = await http.GetAsync("/api/v1/venues" + q);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            return (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("items").EnumerateArray()
                .Any(i => i.GetProperty("slug").GetString() == slug);
        }
        return new Visibility(
            await InSearch(""),
            await InSearch($"?lat={Lat}&lng={Lng}&radiusKm=1"),
            (await http.GetAsync($"/api/v1/venues/{venueId}")).StatusCode,
            (await http.GetAsync($"/api/v1/venues/{slug}")).StatusCode,
            (await http.GetAsync($"/api/v1/venues/{venueId}/availability?date={date:yyyy-MM-dd}")).StatusCode);
    }

    private DateOnly Day() => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime).AddDays(3);

    /// <summary>The owner sets the venue up through the real ClubOS settings endpoints: 1 court, open 06-22 every day, 300/h.</summary>
    private async Task<Guid> SetUpVenueAsync(HttpClient owner, Guid venueId)
    {
        var court = await owner.PostAsJsonAsync(Path(venueId, "courts"), new { name = "Court 1", surface = "hard", indoor = false, airCon = false, floodlit = true, sortOrder = 1 });
        Assert.True(court.StatusCode == HttpStatusCode.Created, await court.Content.ReadAsStringAsync());
        var courtId = (await court.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var hours = await owner.PutAsJsonAsync(Path(venueId, "hours"),
            new { days = Enumerable.Range(0, 7).Select(d => new { dayOfWeek = d, open = "06:00", close = "22:00", slotMinutes = 60 }).ToArray() });
        Assert.True(hours.StatusCode == HttpStatusCode.OK, await hours.Content.ReadAsStringAsync());

        var price = await owner.PostAsJsonAsync(Path(venueId, "prices"),
            new { courtId = (Guid?)null, dayType = "any", start = "06:00", end = "22:00", pricePerHour = 300, label = "Std", priority = 0 });
        Assert.True(price.StatusCode == HttpStatusCode.Created, await price.Content.ReadAsStringAsync());
        return courtId;
    }

    private static Task<HttpResponseMessage> HoldAsync(HttpClient player, Guid venueId, Guid courtId, DateOnly date, string start) =>
        player.PostAsJsonAsync("/api/v1/bookings", new { venueId, date = date.ToString("yyyy-MM-dd"), slots = new[] { new { courtId, start } } });

    // ------------------------------------------------------------------ lifecycle

    [Fact]
    public async Task Partner_lifecycle_create_hidden_approve_bookable_suspend_hidden_reapprove()
    {
        var admin = await AdminAsync();
        var c = NewRequest(email: $"Mixed.Case-{Tag()}@Example.TEST");

        // ---- create: pending partner + draft venue + owner login
        var partner = await CreateOkAsync(admin, c);
        Assert.Equal("pending", partner.GetProperty("status").GetString());
        Assert.Equal(0.10m, partner.GetProperty("commissionRate").GetDecimal());
        Assert.Equal(c.Legal, partner.GetProperty("legalName").GetString());
        Assert.Equal(c.Email.ToLowerInvariant(), partner.GetProperty("ownerEmail").GetString());
        Assert.Equal(1, partner.GetProperty("venues").GetArrayLength());
        var venue = partner.GetProperty("venues")[0];
        Assert.Equal("draft", venue.GetProperty("status").GetString());
        Assert.Equal(c.Slug, venue.GetProperty("slug").GetString());
        var venueId = VenueId(partner);
        var raw = partner.ToString();
        Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(OwnerPw, raw);

        var owner = await OwnerHttpAsync(c.Email); // exact casing typed by the admin, and lowercase, both sign in
        await OwnerHttpAsync(c.Email.ToLowerInvariant());
        var courtId = await SetUpVenueAsync(owner, venueId);
        var day = Day();
        var player = await api.LoginAsync();

        // ---- pending: nothing public
        Assert.True((await VisibilityAsync(venueId, c.Slug, day)).NoneVisible);
        var pendingHold = await HoldAsync(player.Http, venueId, courtId, day, "06:00");
        await ErrorAsync(pendingHold, HttpStatusCode.NotFound, "venue_not_found");
        // the owner can already work in ClubOS
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(Path(venueId, "schedule"))).StatusCode);

        // ---- approve: partner active, draft venue published
        var approved = await ActAsync(admin, partner, "approve");
        Assert.Equal("active", approved.GetProperty("status").GetString());
        Assert.Equal("active", approved.GetProperty("venues")[0].GetProperty("status").GetString());
        var vis = await VisibilityAsync(venueId, c.Slug, day);
        Assert.True(vis.AllVisible, vis.ToString());

        var avail = await api.Anonymous().GetFromJsonAsync<JsonElement>($"/api/v1/venues/{venueId}/availability?date={day:yyyy-MM-dd}");
        Assert.Contains("06:00", avail.ToString());

        // ---- bookable, priced by the rule the owner created (server-side pricing), confirmed only by the webhook
        var held = await HoldAsync(player.Http, venueId, courtId, day, "06:00");
        Assert.Equal(HttpStatusCode.Created, held.StatusCode);
        var confirmedBooking = await held.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("held", confirmedBooking.Status());
        Assert.Equal(300m, confirmedBooking.GetProperty("total").GetDecimal());
        Assert.True((await player.Http.SimulatePaymentAsync(confirmedBooking.Id())).IsSuccessStatusCode);
        Assert.Equal("confirmed", (await player.Http.GetBookingAsync(confirmedBooking.Id())).Status());

        var held2 = await player.Http.PostAsJsonAsync("/api/v1/bookings",
            new { venueId, date = day.ToString("yyyy-MM-dd"), slots = new[] { new { courtId, start = "08:00" } } });
        Assert.Equal(HttpStatusCode.Created, held2.StatusCode);
        var stillHeld = await held2.Content.ReadFromJsonAsync<JsonElement>();

        var otherPlayer = await api.LoginAsync();
        var taken = await HoldAsync(otherPlayer.Http, venueId, courtId, day, "06:00");
        await ErrorAsync(taken, HttpStatusCode.Conflict, "slot_taken");

        // ---- suspend: hidden again, no new holds
        var suspended = await ActAsync(admin, partner, "suspend");
        Assert.Equal("suspended", suspended.GetProperty("status").GetString());
        Assert.Equal("active", suspended.GetProperty("venues")[0].GetProperty("status").GetString()); // the venue row itself is untouched
        Assert.True((await VisibilityAsync(venueId, c.Slug, day)).NoneVisible);
        await ErrorAsync(await HoldAsync(otherPlayer.Http, venueId, courtId, day, "10:00"), HttpStatusCode.NotFound, "venue_not_found");

        // ---- existing bookings survive suspension: still readable, a held one can still be paid, a confirmed one cancelled with a refund
        Assert.Equal("confirmed", (await player.Http.GetBookingAsync(confirmedBooking.Id())).Status());
        Assert.True((await player.Http.SimulatePaymentAsync(stillHeld.Id())).IsSuccessStatusCode);
        Assert.Equal("confirmed", (await player.Http.GetBookingAsync(stillHeld.Id())).Status());
        var cancel = await player.Http.PostAsync($"/api/v1/me/bookings/{confirmedBooking.Id()}/cancel", null);
        Assert.True(cancel.StatusCode == HttpStatusCode.OK, await cancel.Content.ReadAsStringAsync());
        Assert.Equal("cancelled", (await player.Http.GetBookingAsync(confirmedBooking.Id())).Status());
        // ...and the owner keeps ClubOS access to see them
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(Path(venueId, "schedule"))).StatusCode);

        // ---- re-approve: back in business, the venue was not re-drafted
        var again = await ActAsync(admin, partner, "approve");
        Assert.Equal("active", again.GetProperty("status").GetString());
        Assert.True((await VisibilityAsync(venueId, c.Slug, day)).AllVisible);
        Assert.Equal(HttpStatusCode.Created, (await HoldAsync(otherPlayer.Http, venueId, courtId, day, "10:00")).StatusCode);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Created_rows_match_the_contract_partner_owner_user_with_bcrypt_hash_no_phone_owner_member_and_draft_venue()
    {
        var admin = await AdminAsync();
        var c = NewRequest(email: $"  Rows.{Tag()}@Example.Test ", tweak: b => { b["commissionRate"] = 0.15; b["taxId"] = "   "; });
        var partner = await CreateOkAsync(admin, c);
        var partnerId = partner.GetProperty("id").GetGuid();
        var venueId = VenueId(partner);

        await api.WithDb(async db =>
        {
            var p = await db.Partners.AsNoTracking().SingleAsync(x => x.Id == partnerId);
            Assert.Equal("pending", p.Status);
            Assert.Equal(0.15m, p.CommissionRate);
            Assert.Null(p.TaxId);                      // blank strings are stored as NULL
            Assert.Equal("KBank", p.BankName);

            var u = await db.Users.AsNoTracking().SingleAsync(x => x.Email == c.Email.Trim().ToLowerInvariant());
            Assert.Equal("partner_owner", u.Role);
            Assert.Null(u.Phone);
            Assert.StartsWith("$2", u.PasswordHash);
            Assert.NotEqual(OwnerPw, u.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(OwnerPw, u.PasswordHash));

            var m = await db.VenueMembers.AsNoTracking().SingleAsync(x => x.VenueId == venueId);
            Assert.Equal(u.Id, m.UserId);
            Assert.Equal("owner", m.Role);

            var v = await db.Venues.AsNoTracking().SingleAsync(x => x.Id == venueId);
            Assert.Equal("draft", v.Status);
            Assert.Equal(partnerId, v.PartnerId);
            Assert.Equal(Lat, v.Lat);
        });
    }

    [Fact]
    public async Task Owner_can_sign_in_to_ClubOS_immediately_but_only_reaches_their_own_venue_and_only_writes_as_owner()
    {
        var admin = await AdminAsync();
        var c = NewRequest();
        var partner = await CreateOkAsync(admin, c);
        var venueId = VenueId(partner);
        await ActAsync(admin, partner, "approve");

        var owner = await OwnerHttpAsync(c.Email);
        var me = await owner.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");
        Assert.Equal("partner_owner", me.GetProperty("role").GetString());

        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(Path(venueId, "schedule"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(Path(venueId, "prices"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync(Path(venueId, "settings/cancel-policy"), new { cancelFreeHours = 6 })).StatusCode);

        // another venue: forbidden on reads and on owner-only writes
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.GetAsync(Path(Seed.VenueId, "schedule"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.GetAsync(Path(Seed.VenueId, "prices"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.PutAsJsonAsync(Path(Seed.VenueId, "settings/cancel-policy"), new { cancelFreeHours = 1 })).StatusCode);

        // and the owner is not a platform admin
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.GetAsync(Partners)).StatusCode);
        // the new owner cannot use the admin login (staff accounts only)
        var adminLogin = await api.Anonymous().PostAsJsonAsync(AdminLogin, new { email = c.Email, password = OwnerPw });
        Assert.Equal(HttpStatusCode.BadRequest, adminLogin.StatusCode);
    }

    [Fact]
    public async Task Suspending_hides_every_venue_of_the_partner_and_approving_only_publishes_draft_venues()
    {
        var admin = await AdminAsync();
        var c = NewRequest();
        var partner = await CreateOkAsync(admin, c);
        var pid = partner.GetProperty("id").GetGuid();
        var draftId = VenueId(partner);
        var (activeId, hiddenId) = (Guid.NewGuid(), Guid.NewGuid());
        var (activeSlug, hiddenSlug) = ("x-a-" + Tag(), "x-h-" + Tag());
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status) VALUES
                  ({activeId}, {pid}, {activeSlug}, 'ก', 'A', 'T', 'T', {Lat}, {Lng}, 'active'),
                  ({hiddenId}, {pid}, {hiddenSlug}, 'ข', 'H', 'T', 'T', {Lat}, {Lng}, 'hidden')
                """);
        });
        var day = Day();

        // pending partner: even its already-"active" venue is not public
        Assert.True((await VisibilityAsync(activeId, activeSlug, day)).NoneVisible);

        var approved = await ActAsync(admin, partner, "approve");
        var statuses = approved.GetProperty("venues").EnumerateArray().ToDictionary(v => v.GetProperty("slug").GetString()!, v => v.GetProperty("status").GetString());
        Assert.Equal("active", statuses[c.Slug]);        // draft -> active
        Assert.Equal("active", statuses[activeSlug]);
        Assert.Equal("hidden", statuses[hiddenSlug]);   // hidden stays hidden
        Assert.True((await VisibilityAsync(draftId, c.Slug, day)).AllVisible);
        Assert.True((await VisibilityAsync(activeId, activeSlug, day)).AllVisible);
        Assert.True((await VisibilityAsync(hiddenId, hiddenSlug, day)).NoneVisible);

        await ActAsync(admin, partner, "suspend");
        Assert.True((await VisibilityAsync(draftId, c.Slug, day)).NoneVisible);
        Assert.True((await VisibilityAsync(activeId, activeSlug, day)).NoneVisible);

        // re-approving a suspended partner does not resurrect a hidden venue nor touch statuses
        var re = await ActAsync(admin, partner, "approve");
        Assert.Equal("hidden", re.GetProperty("venues").EnumerateArray().Single(v => v.GetProperty("slug").GetString() == hiddenSlug).GetProperty("status").GetString());
        Assert.True((await VisibilityAsync(hiddenId, hiddenSlug, day)).NoneVisible);
    }

    [Fact]
    public async Task A_suspended_partners_draft_venue_is_published_when_it_is_approved_again()
    {
        // pending -> active -> suspended; a venue added meanwhile as draft goes live on re-approval (approve publishes drafts)
        var admin = await AdminAsync();
        var partner = await CreateOkAsync(admin, NewRequest());
        var pid = partner.GetProperty("id").GetGuid();
        await ActAsync(admin, partner, "approve");
        await ActAsync(admin, partner, "suspend");
        var extra = Guid.NewGuid();
        var extraSlug = "x-d-" + Tag();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
            VALUES ({extra}, {pid}, {extraSlug}, 'ก', 'A', 'T', 'T', {Lat}, {Lng}, 'draft')
            """));

        var re = await ActAsync(admin, partner, "approve");

        Assert.All(re.GetProperty("venues").EnumerateArray(), v => Assert.Equal("active", v.GetProperty("status").GetString()));
    }

    // ------------------------------------------------------------------ state machine

    [Fact]
    public async Task Transitions_only_pending_or_suspended_to_active_and_only_active_to_suspended()
    {
        var admin = await AdminAsync();
        var partner = await CreateOkAsync(admin, NewRequest());

        // pending: cannot suspend
        await ErrorAsync(await admin.PostAsync($"{Partners}/{partner.GetProperty("id").GetGuid()}/suspend", null), HttpStatusCode.Conflict, "bad_transition");
        await ActAsync(admin, partner, "approve");
        // active: cannot approve again
        await ErrorAsync(await admin.PostAsync($"{Partners}/{partner.GetProperty("id").GetGuid()}/approve", null), HttpStatusCode.Conflict, "bad_transition");
        await ActAsync(admin, partner, "suspend");
        // suspended: cannot suspend again, can approve
        await ErrorAsync(await admin.PostAsync($"{Partners}/{partner.GetProperty("id").GetGuid()}/suspend", null), HttpStatusCode.Conflict, "bad_transition");
        Assert.Equal("active", (await ActAsync(admin, partner, "approve")).GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("approve")]
    [InlineData("suspend")]
    public async Task Unknown_partner_is_404_and_a_non_guid_id_never_reaches_the_handler(string action)
    {
        var admin = await AdminAsync();

        var res = await admin.PostAsync($"{Partners}/{Guid.NewGuid()}/{action}", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("not_found", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        Assert.Equal(HttpStatusCode.NotFound, (await admin.PostAsync($"{Partners}/not-a-guid/{action}", null)).StatusCode);
    }

    // ------------------------------------------------------------------ authorization

    [Fact]
    public async Task Every_partner_route_is_401_for_anonymous_callers()
    {
        var anon = api.Anonymous();
        var id = Guid.NewGuid();

        var responses = new[]
        {
            await anon.GetAsync(Partners),
            await PostJsonAsync(anon, Partners, NewRequest().Body),
            await anon.PostAsync($"{Partners}/{id}/approve", null),
            await anon.PostAsync($"{Partners}/{id}/suspend", null),
        };

        foreach (var r in responses)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            Assert.Equal("unauthorized", (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Every_partner_route_is_403_for_players_and_partner_staff_and_owners_and_changes_nothing()
    {
        var admin = await AdminAsync();
        var target = await CreateOkAsync(admin, NewRequest());
        var id = target.GetProperty("id").GetGuid();
        var players = new[]
        {
            await api.LoginAsync(),
            await api.LoginPartnerAsync(Seed.VenueId, "owner", "partner_owner"),
            await api.LoginPartnerAsync(Seed.VenueId, "staff", "partner_staff"),
        };
        var blocked = NewRequest();

        foreach (var u in players)
        {
            var responses = new[]
            {
                await u.Http.GetAsync(Partners),
                await PostJsonAsync(u.Http, Partners, blocked.Body),
                await u.Http.PostAsync($"{Partners}/{id}/approve", null),
                await u.Http.PostAsync($"{Partners}/{id}/suspend", null),
            };
            foreach (var r in responses)
            {
                Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
                Assert.Equal("forbidden", (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
            }
        }

        Assert.Equal(0, await CountPartnersAsync(blocked.Legal));
        Assert.False(await UserExistsAsync(blocked.Email));
        Assert.Equal("pending", (await PartnerListAsync(admin, "pending")).Single(p => p.GetProperty("id").GetGuid() == id).GetProperty("status").GetString());
    }

    [Fact]
    public async Task Forbidden_wins_over_validation_for_a_non_admin_posting_an_invalid_body()
    {
        var player = await api.LoginAsync();

        var res = await PostJsonAsync(player.Http, Partners, new JsonObject());

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    // ------------------------------------------------------------------ list

    [Fact]
    public async Task List_is_newest_first_filters_by_status_and_reports_owner_and_venues()
    {
        var admin = await AdminAsync();
        var a = await CreateOkAsync(admin, NewRequest());
        api.Clock.Advance(TimeSpan.FromMinutes(1));
        var b = await CreateOkAsync(admin, NewRequest());
        api.Clock.Advance(TimeSpan.FromMinutes(1));
        var c = await CreateOkAsync(admin, NewRequest());
        await ActAsync(admin, b, "approve");
        await ActAsync(admin, c, "approve");
        await ActAsync(admin, c, "suspend");
        Guid Id(JsonElement e) => e.GetProperty("id").GetGuid();
        var mine = new[] { Id(a), Id(b), Id(c) };

        var all = (await PartnerListAsync(admin)).Where(p => mine.Contains(Id(p))).ToArray();
        Assert.Equal(new[] { Id(c), Id(b), Id(a) }, all.Select(Id));  // newest first

        Guid[] Filtered(JsonElement[] l) => l.Where(p => mine.Contains(Id(p))).Select(Id).ToArray();
        Assert.Equal(new[] { Id(a) }, Filtered(await PartnerListAsync(admin, "pending")));
        Assert.Equal(new[] { Id(b) }, Filtered(await PartnerListAsync(admin, "active")));
        Assert.Equal(new[] { Id(c) }, Filtered(await PartnerListAsync(admin, "suspended")));
        foreach (var status in new[] { "pending", "active", "suspended" })
            Assert.All(await PartnerListAsync(admin, status), p => Assert.Equal(status, p.GetProperty("status").GetString()));

        var row = all.Single(p => Id(p) == Id(a));
        Assert.False(string.IsNullOrEmpty(row.GetProperty("ownerEmail").GetString()));
        Assert.StartsWith("Owner ", row.GetProperty("ownerName").GetString());
        Assert.Equal(1, row.GetProperty("venues").GetArrayLength());
        Assert.Equal("draft", row.GetProperty("venues")[0].GetProperty("status").GetString());
    }

    [Fact]
    public async Task List_includes_the_seeded_partner_and_rejects_an_unknown_status_filter()
    {
        var admin = await AdminAsync();

        var active = await PartnerListAsync(admin, "active");
        Assert.Contains(active, p => p.GetProperty("venues").EnumerateArray().Any(v => v.GetProperty("slug").GetString() == Seed.VenueSlug));

        foreach (var bad in new[] { "bogus", "ACTIVE", "" })
            await ErrorAsync(await admin.GetAsync($"{Partners}?status={bad}"), HttpStatusCode.BadRequest, "bad_status");
    }

    // ------------------------------------------------------------------ create: validation

    [Theory]
    [InlineData("commissionRate", "0.5001", "bad_commission")]
    [InlineData("commissionRate", "-0.01", "bad_commission")]
    [InlineData("commissionRate", "1", "bad_commission")]
    [InlineData("venue.slug", "\"My-Club\"", "bad_slug")]
    [InlineData("venue.slug", "\"my_club\"", "bad_slug")]
    [InlineData("venue.slug", "\"-club\"", "bad_slug")]
    [InlineData("venue.slug", "\"club-\"", "bad_slug")]
    [InlineData("venue.slug", "\"my--club\"", "bad_slug")]
    [InlineData("venue.slug", "\"my club\"", "bad_slug")]
    [InlineData("venue.slug", "\"สนาม\"", "bad_slug")]
    [InlineData("venue.lat", "90.1", "bad_location")]
    [InlineData("venue.lat", "-90.1", "bad_location")]
    [InlineData("venue.lng", "180.1", "bad_location")]
    [InlineData("venue.lng", "-180.1", "bad_location")]
    public async Task Create_rejects_business_rule_violations_with_a_specific_code_and_creates_nothing(string path, string json, string code)
    {
        var admin = await AdminAsync();
        var c = NewRequest(tweak: b => Set(b, path, json));

        await ErrorAsync(await PostJsonAsync(admin, Partners, c.Body), HttpStatusCode.BadRequest, code);

        Assert.Equal(0, await CountPartnersAsync(c.Legal));
        Assert.False(await UserExistsAsync(c.Email));
        Assert.False(await VenueSlugExistsAsync(c.Slug));
    }

    [Theory]
    [InlineData("owner.password", "\"123456789\"")]                     // 9 chars
    [InlineData("owner.password", "\"\"")]
    [InlineData("owner.password", "\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"")] // 73 bytes
    [InlineData("owner.password", "\"กขคงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรลวศษสหฬอฮ\"")]                               // 44 chars but 132 bytes
    [InlineData("owner.email", "\"not-an-email\"")]
    [InlineData("owner.email", "\"\"")]
    [InlineData("owner.displayName", "\"\"")]
    [InlineData("legalName", "\"\"")]
    [InlineData("legalName", "\"   \"")]
    [InlineData("venue.nameTh", "\"\"")]
    [InlineData("venue.nameEn", "null")]
    [InlineData("venue.district", "\"\"")]
    [InlineData("venue.address", "\"\"")]
    [InlineData("venue.slug", "\"\"")]
    [InlineData("owner", "REMOVE")]
    [InlineData("venue", "REMOVE")]
    [InlineData("legalName", "REMOVE")]
    public async Task Create_rejects_invalid_bodies_with_validation_failed_and_an_errors_object(string path, string json)
    {
        var admin = await AdminAsync();
        var c = NewRequest(tweak: b => Set(b, path, json));

        var body = await ErrorAsync(await PostJsonAsync(admin, Partners, c.Body), HttpStatusCode.BadRequest, "validation_failed");

        Assert.Equal(JsonValueKind.Object, body.GetProperty("errors").ValueKind);
        Assert.NotEmpty(body.GetProperty("errors").EnumerateObject());
        Assert.Equal(0, await CountPartnersAsync(c.Legal));
        Assert.False(await UserExistsAsync(c.Email));
        Assert.False(await VenueSlugExistsAsync(c.Slug));
    }

    [Fact]
    public async Task Create_rejects_an_overlong_field_and_an_empty_body()
    {
        var admin = await AdminAsync();
        var long1 = NewRequest(tweak: b => Set(b, "legalName", "\"" + new string('x', 161) + "\""));
        var long2 = NewRequest(tweak: b => Set(b, "venue.slug", "\"" + new string('a', 61) + "\""));

        await ErrorAsync(await PostJsonAsync(admin, Partners, long1.Body), HttpStatusCode.BadRequest, "validation_failed");
        await ErrorAsync(await PostJsonAsync(admin, Partners, long2.Body), HttpStatusCode.BadRequest, "validation_failed");
        var empty = await admin.PostAsync(Partners, new StringContent("", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode); // (the framework logs the missing body at Error, but answers 400)
    }

    [Fact]
    public async Task Create_accepts_the_boundaries_commission_0_and_0_5_password_of_10_chars_and_72_bytes_and_extreme_coordinates()
    {
        var admin = await AdminAsync();
        var zero = await CreateOkAsync(admin, NewRequest(tweak: b => b["commissionRate"] = 0));
        Assert.Equal(0m, zero.GetProperty("commissionRate").GetDecimal());
        var half = await CreateOkAsync(admin, NewRequest(tweak: b => b["commissionRate"] = 0.5));
        Assert.Equal(0.5m, half.GetProperty("commissionRate").GetDecimal());

        var pw72 = new string('a', 72);
        var c72 = NewRequest(tweak: b => Set(b, "owner.password", $"\"{pw72}\""));
        await CreateOkAsync(admin, c72);
        await OwnerHttpAsync(c72.Email, pw72);

        var c10 = NewRequest(tweak: b => Set(b, "owner.password", "\"0123456789\""));
        await CreateOkAsync(admin, c10);
        await OwnerHttpAsync(c10.Email, "0123456789");

        // 72 UTF-8 bytes made of 3-byte Thai characters (24 chars)
        var thai = string.Concat(Enumerable.Repeat("ก", 24));
        var cThai = NewRequest(tweak: b => Set(b, "owner.password", $"\"{thai}\""));
        await CreateOkAsync(admin, cThai);
        await OwnerHttpAsync(cThai.Email, thai);

        await CreateOkAsync(admin, NewRequest(tweak: b => { Set(b, "venue.lat", "90"); Set(b, "venue.lng", "-180"); }));
    }

    [Fact]
    public async Task Owner_password_is_hashed_with_bcrypt_and_is_case_sensitive_at_login()
    {
        var admin = await AdminAsync();
        var c = NewRequest();
        await CreateOkAsync(admin, c);

        var wrong = await api.Anonymous().PostAsJsonAsync(StaffLogin, new { email = c.Email, password = OwnerPw.ToUpperInvariant() });
        await ErrorAsync(wrong, HttpStatusCode.BadRequest, "invalid_credentials");
        await OwnerHttpAsync(c.Email);
    }

    // ------------------------------------------------------------------ create: uniqueness + atomicity

    [Fact]
    public async Task Email_is_unique_case_insensitively_and_a_rejected_create_leaves_nothing_behind()
    {
        var admin = await AdminAsync();
        var first = NewRequest(email: $"dup-{Tag()}@example.test");
        await CreateOkAsync(admin, first);

        foreach (var variant in new[] { first.Email, first.Email.ToUpperInvariant(), "  " + first.Email + " " })
        {
            var dup = NewRequest(email: variant);
            await ErrorAsync(await PostJsonAsync(admin, Partners, dup.Body), HttpStatusCode.Conflict, "email_taken");
            Assert.Equal(0, await CountPartnersAsync(dup.Legal));
            Assert.False(await VenueSlugExistsAsync(dup.Slug));
        }
    }

    [Fact]
    public async Task Existing_seeded_and_player_emails_cannot_be_reused()
    {
        var admin = await AdminAsync();
        foreach (var email in new[] { "owner@racquetclub.test", "ADMIN@bkktennis.test" })
        {
            var c = NewRequest(email: email);
            await ErrorAsync(await PostJsonAsync(admin, Partners, c.Body), HttpStatusCode.Conflict, "email_taken");
            Assert.Equal(0, await CountPartnersAsync(c.Legal));
        }
        // the seeded logins still work (nothing was overwritten)
        await OwnerHttpAsync("owner@racquetclub.test", "racquet-dev-2026");
    }

    [Fact]
    public async Task Slug_is_unique_across_all_venues_and_a_rejected_create_leaves_no_partner_user_or_venue()
    {
        var admin = await AdminAsync();
        var first = NewRequest();
        await CreateOkAsync(admin, first);

        foreach (var slug in new[] { first.Slug, Seed.VenueSlug })
        {
            var dup = NewRequest(slug: slug);
            await ErrorAsync(await PostJsonAsync(admin, Partners, dup.Body), HttpStatusCode.Conflict, "slug_taken");
            Assert.Equal(0, await CountPartnersAsync(dup.Legal));
            Assert.False(await UserExistsAsync(dup.Email));
        }
    }

    [Fact]
    public async Task Email_wins_over_slug_when_both_are_taken()
    {
        var admin = await AdminAsync();
        var first = NewRequest();
        await CreateOkAsync(admin, first);

        var both = NewRequest(email: first.Email, slug: first.Slug);

        await ErrorAsync(await PostJsonAsync(admin, Partners, both.Body), HttpStatusCode.Conflict, "email_taken");
    }

    [Fact]
    public async Task Racing_creates_with_the_same_email_or_slug_produce_exactly_one_partner_and_a_clean_409_for_the_rest()
    {
        var admin = await AdminAsync();
        var email = $"race-{Tag()}@example.test";
        var slug = "race-" + Tag();
        var reqs = Enumerable.Range(0, 6).Select(_ => NewRequest(email: email, slug: slug)).ToArray();

        var results = await Task.WhenAll(reqs.Select(r => PostJsonAsync(admin, Partners, r.Body)));
        foreach (var r in results.Where(r => r.StatusCode == HttpStatusCode.Created))
            _partners.Add((await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());

        Assert.True(results.Count(r => r.StatusCode == HttpStatusCode.Created) == 1,
            string.Join(" | ", results.Select(r => (int)r.StatusCode)));
        foreach (var loser in results.Where(r => r.StatusCode != HttpStatusCode.Created))
        {
            Assert.Equal(HttpStatusCode.Conflict, loser.StatusCode);
            Assert.Contains((await loser.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString(),
                new[] { "email_taken", "slug_taken", "conflict" });
        }
        // atomic: no orphans from the losers
        var total = 0;
        await api.WithDb(async db => total = await db.Partners.CountAsync(p => reqs.Select(r => r.Legal).Contains(p.LegalName)));
        Assert.Equal(1, total);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Two_different_partners_may_share_names_tax_ids_and_bank_details()
    {
        var admin = await AdminAsync();
        var one = NewRequest(legal: "Same Legal Name " + Tag());
        await CreateOkAsync(admin, one);

        await CreateOkAsync(admin, NewRequest(legal: one.Legal)); // only email + slug must be unique
    }
}
