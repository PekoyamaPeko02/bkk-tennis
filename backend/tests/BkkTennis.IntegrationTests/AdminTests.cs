using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Services;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>A1: platform-admin login + GET /admin/overview. The DB is shared, so numbers are asserted as deltas.</summary>
[Collection(ApiCollection.Name)]
public class AdminTests(ApiFactory api)
{
    private const string AdminLogin = "/api/v1/auth/admin/login", StaffLogin = "/api/v1/auth/staff/login", Overview = "/api/v1/admin/overview";
    private const string AdminEmail = "admin@bkktennis.test", AdminPassword = "admin-dev-2026";
    private const string OwnerEmail = "owner@racquetclub.test", OwnerPassword = "racquet-dev-2026";
    private const string Pw = "correct-horse-1";

    private static string Email() => $"admin-{Guid.NewGuid():N}@example.test";

    private Task<HttpResponseMessage> LoginAsync(string email, string password, HttpClient? http = null, string url = AdminLogin) =>
        (http ?? api.Anonymous()).PostAsJsonAsync(url, new { email, password });

    private async Task<string> CreateUserAsync(string role = "platform_admin", string password = Pw, bool withPassword = true)
    {
        var email = Email();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO users (id, email, role, display_name, password_hash)
            VALUES ({Guid.NewGuid()}, {email}, {role}, 'Test', CASE WHEN {withPassword} THEN crypt({password}, gen_salt('bf', 4)) END)
            """));
        return email;
    }

    private async Task<HttpClient> AdminHttpAsync()
    {
        var res = await LoginAsync(AdminEmail, AdminPassword);
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        return http;
    }

    private static async Task AssertInvalidCredentialsAsync(HttpResponseMessage res)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.BadRequest, $"expected 400 got {(int)res.StatusCode}: {raw}");
        Assert.Equal("invalid_credentials", JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
    }

    // ------------------------------------------------------------------ admin login

    [Fact]
    public async Task Seeded_admin_signs_in_with_a_platform_admin_token()
    {
        var res = await LoginAsync("  Admin@BKKTennis.TEST ", AdminPassword);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
        var body = JsonDocument.Parse(raw).RootElement;
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.True(body.GetProperty("expiresAt").GetDateTime() > api.Clock.GetUtcNow().UtcDateTime);
        Assert.Equal("platform_admin", body.GetProperty("user").GetProperty("role").GetString());
        Assert.Equal(AdminEmail, body.GetProperty("user").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Admin_login_refuses_partner_and_player_accounts_with_the_generic_error()
    {
        foreach (var role in new[] { "partner_owner", "partner_staff", "player" })
            await AssertInvalidCredentialsAsync(await LoginAsync(await CreateUserAsync(role), Pw));

        await AssertInvalidCredentialsAsync(await LoginAsync(OwnerEmail, OwnerPassword)); // the seeded ClubOS owner
    }

    [Fact]
    public async Task Staff_login_refuses_the_seeded_admin()
    {
        await AssertInvalidCredentialsAsync(await LoginAsync(AdminEmail, AdminPassword, url: StaffLogin));
    }

    [Fact]
    public async Task Wrong_password_unknown_email_and_missing_password_hash_are_indistinguishable()
    {
        var wrong = await LoginAsync(await CreateUserAsync(), "nope-nope-nope");
        var unknown = await LoginAsync("nobody@example.test", "nope-nope-nope");
        var noHash = await LoginAsync(await CreateUserAsync(withPassword: false), "anything");
        var partner = await LoginAsync(await CreateUserAsync("partner_owner"), Pw);

        foreach (var r in new[] { wrong, unknown, noHash, partner }) await AssertInvalidCredentialsAsync(r);
        var expected = await wrong.Content.ReadAsStringAsync();
        foreach (var r in new[] { unknown, noHash, partner }) Assert.Equal(expected, await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_password_is_case_sensitive()
    {
        var email = await CreateUserAsync();
        await AssertInvalidCredentialsAsync(await LoginAsync(email, Pw.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, Pw)).StatusCode);
    }

    [Fact]
    public async Task Five_wrong_admin_passwords_lock_the_account_for_fifteen_minutes()
    {
        var email = await CreateUserAsync();
        for (var i = 0; i < 5; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong-" + i));

        await AssertInvalidCredentialsAsync(await LoginAsync(email, Pw));

        api.Clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, Pw)).StatusCode);
    }

    [Fact]
    public async Task Four_misses_then_success_do_not_lock()
    {
        var email = await CreateUserAsync();
        for (var round = 0; round < 2; round++)
        {
            for (var i = 0; i < 4; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong"));
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, Pw)).StatusCode);
        }
    }

    [Theory]
    [InlineData("""{ }""", "email")]
    [InlineData("""{ "email": "a@b.test" }""", "password")]
    [InlineData("""{ "email": "", "password": "x" }""", "email")]
    public async Task Admin_login_validates_its_body(string raw, string field)
    {
        var res = await api.Anonymous().PostAsync(AdminLogin, new StringContent(raw, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty(field, out _), body.ToString());
    }

    [Fact]
    public async Task Admin_login_rejects_passwords_over_72_bytes()
    {
        var email = await CreateUserAsync(password: new string('a', 72));

        var res = await LoginAsync(email, new string('a', 73));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("validation_failed", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Admin_login_is_rate_limited_per_ip()
    {
        using var limited = api.WithWebHostBuilder(b =>
        {
            b.UseSetting("RateLimit:Otp:PermitLimit", "3");
            b.UseSetting("RateLimit:Otp:WindowSeconds", "30");
        });
        var http = limited.CreateClient();
        var email = await CreateUserAsync();

        for (var i = 0; i < 3; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong", http));
        var blocked = await LoginAsync(email, Pw, http);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("rate_limited", (await blocked.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    // ------------------------------------------------------------------ overview authorization

    [Fact]
    public async Task Overview_requires_a_token()
    {
        var res = await api.Anonymous().GetAsync(Overview);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("unauthorized", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Overview_is_forbidden_for_players_and_partners()
    {
        var player = await api.LoginAsync();
        var owner = await api.LoginPartnerAsync(Seed.VenueId, "owner", "partner_owner");
        var staff = await api.LoginPartnerAsync(Seed.VenueId, "staff", "partner_staff");

        foreach (var u in new[] { player, owner, staff })
        {
            var res = await u.Http.GetAsync(Overview);
            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
            Assert.Equal("forbidden", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task Overview_is_readable_by_a_token_from_the_admin_login_and_is_read_only()
    {
        var admin = await AdminHttpAsync();

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(Overview)).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await admin.PostAsync(Overview, null)).StatusCode);
    }

    // ------------------------------------------------------------------ overview numbers

    private sealed record Day(string Date, int Bookings, decimal Gbv, decimal Take);
    private sealed record Snap(decimal Gbv, decimal Take, int Bookings, int WalkIns, int Partners, int Venues, int Courts, List<Day> Days);

    private async Task<Snap> GetAsync(HttpClient admin)
    {
        var res = await admin.GetAsync(Overview);
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var j = await res.Content.ReadFromJsonAsync<JsonElement>();
        return new Snap(j.GetProperty("gbv").GetDecimal(), j.GetProperty("takeRevenue").GetDecimal(), j.GetProperty("bookings").GetInt32(),
            j.GetProperty("walkInBookings").GetInt32(), j.GetProperty("activePartners").GetInt32(), j.GetProperty("activeVenues").GetInt32(),
            j.GetProperty("activeCourts").GetInt32(),
            j.GetProperty("last30Days").EnumerateArray().Select(d => new Day(d.GetProperty("date").GetString()!, d.GetProperty("bookings").GetInt32(),
                d.GetProperty("gbv").GetDecimal(), d.GetProperty("take").GetDecimal())).ToList());
    }

    private Task InsertBookingAsync(string status, string source, decimal total, decimal commission, DateTime createdUtc) =>
        api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bookings (venue_id, status, source, subtotal, total, commission, created_at)
            VALUES ({Seed.VenueId}, {status}, {source}, {total}, {total}, {commission}, {createdUtc})
            """));

    private DateOnly Today() => VenueClock.Today(Seed.Timezone, api.Clock.GetUtcNow().UtcDateTime);
    private static DateTime StartOfBangkokDayUtc(DateOnly d) => VenueClock.ToUtc(d, TimeOnly.MinValue, Seed.Timezone);

    [Fact]
    public async Task Series_has_30_zero_filled_days_ending_today_oldest_first()
    {
        var admin = await AdminHttpAsync();

        var s = await GetAsync(admin);

        Assert.Equal(30, s.Days.Count);
        var today = Today();
        Assert.Equal(Enumerable.Range(0, 30).Select(i => today.AddDays(i - 29).ToString("yyyy-MM-dd")), s.Days.Select(d => d.Date));
    }

    [Fact]
    public async Task Only_sold_app_bookings_count_toward_gbv_take_and_bookings()
    {
        var admin = await AdminHttpAsync();
        var before = await GetAsync(admin);
        var now = api.Clock.GetUtcNow().UtcDateTime;

        // counted: 4 sold statuses, app source
        await InsertBookingAsync("confirmed", "app", 1000, 100, now);
        await InsertBookingAsync("checked_in", "app", 2000, 200, now);
        await InsertBookingAsync("completed", "app", 3000, 300, now);
        await InsertBookingAsync("no_show", "app", 4000, 400, now);
        // not counted: money not held (or refunded)
        await InsertBookingAsync("held", "app", 10_000, 1000, now);
        await InsertBookingAsync("expired", "app", 20_000, 2000, now);
        await InsertBookingAsync("cancelled", "app", 40_000, 4000, now);
        // walk-ins: counted apart, never in GBV/take
        await InsertBookingAsync("confirmed", "walk_in", 50_000, 0, now);
        await InsertBookingAsync("completed", "walk_in", 60_000, 0, now);
        await InsertBookingAsync("cancelled", "walk_in", 70_000, 0, now);

        var after = await GetAsync(admin);

        Assert.Equal(10_000m, after.Gbv - before.Gbv);
        Assert.Equal(1000m, after.Take - before.Take);
        Assert.Equal(4, after.Bookings - before.Bookings);
        Assert.Equal(2, after.WalkIns - before.WalkIns);
        var t = after.Days[^1]; var t0 = before.Days[^1];
        Assert.Equal(4, t.Bookings - t0.Bookings);
        Assert.Equal(10_000m, t.Gbv - t0.Gbv);
        Assert.Equal(1000m, t.Take - t0.Take);
    }

    [Fact]
    public async Task A_booking_lands_on_its_bangkok_day_boundary_is_17_00_utc()
    {
        var admin = await AdminHttpAsync();
        var d = Today().AddDays(-6);
        var before = await GetAsync(admin);
        var startOfD = StartOfBangkokDayUtc(d);
        Assert.Equal(17, startOfD.Hour); // D 00:00 Bangkok == (D-1) 17:00 UTC

        await InsertBookingAsync("confirmed", "app", 100, 10, startOfD);                           // first instant of D
        await InsertBookingAsync("confirmed", "app", 200, 20, startOfD.AddSeconds(-1));            // last second of D-1
        await InsertBookingAsync("confirmed", "app", 400, 40, startOfD.AddDays(1).AddSeconds(-1)); // last second of D
        await InsertBookingAsync("confirmed", "app", 800, 80, startOfD.AddDays(1));                // first instant of D+1

        var after = await GetAsync(admin);

        (int Bookings, decimal Gbv, decimal Take) Delta(int offset)
        {
            var key = d.AddDays(offset).ToString("yyyy-MM-dd");
            var a = after.Days.Single(x => x.Date == key); var b = before.Days.Single(x => x.Date == key);
            return (a.Bookings - b.Bookings, a.Gbv - b.Gbv, a.Take - b.Take);
        }
        Assert.Equal((1, 200m, 20m), Delta(-1));
        Assert.Equal((2, 500m, 50m), Delta(0));
        Assert.Equal((1, 800m, 80m), Delta(1));
    }

    [Fact]
    public async Task Window_includes_today_minus_29_and_excludes_older_days_but_totals_keep_them()
    {
        var admin = await AdminHttpAsync();
        var first = Today().AddDays(-29);
        var before = await GetAsync(admin);

        await InsertBookingAsync("confirmed", "app", 300, 30, StartOfBangkokDayUtc(first));                 // in the window
        await InsertBookingAsync("confirmed", "app", 700, 70, StartOfBangkokDayUtc(first).AddSeconds(-1));  // 30th day back: out
        var after = await GetAsync(admin);

        Assert.Equal(1, after.Days[0].Bookings - before.Days[0].Bookings);
        Assert.Equal(300m, after.Days[0].Gbv - before.Days[0].Gbv);
        Assert.Equal(before.Days.Sum(x => x.Bookings) + 1, after.Days.Sum(x => x.Bookings));
        Assert.Equal(2, after.Bookings - before.Bookings); // all-time totals include the old one
        Assert.Equal(1000m, after.Gbv - before.Gbv);
    }

    [Fact]
    public async Task Series_ignores_walk_ins_and_unsold_bookings()
    {
        var admin = await AdminHttpAsync();
        var before = await GetAsync(admin);
        var now = api.Clock.GetUtcNow().UtcDateTime;

        await InsertBookingAsync("confirmed", "walk_in", 900, 0, now);
        await InsertBookingAsync("held", "app", 900, 90, now);
        await InsertBookingAsync("cancelled", "app", 900, 90, now);

        var after = await GetAsync(admin);
        Assert.Equal(before.Days, after.Days);
    }

    [Fact]
    public async Task A_real_booking_paid_through_the_api_shows_up_with_server_side_price_and_commission()
    {
        var admin = await AdminHttpAsync();
        var before = await GetAsync(admin);
        var player = await api.LoginAsync();
        var block = SlotPicker.Next(api);

        var held = await player.Http.HoldOkAsync(block, slots: 2);
        var afterHold = await GetAsync(admin);
        Assert.Equal(before.Gbv, afterHold.Gbv); // a hold is not revenue
        Assert.Equal(before.Bookings, afterHold.Bookings);

        Assert.True((await player.Http.SimulatePaymentAsync(held.Id())).IsSuccessStatusCode);
        var after = await GetAsync(admin);

        Assert.Equal(2 * Seed.StandardHourly, after.Gbv - before.Gbv);
        Assert.Equal(0.10m * 2 * Seed.StandardHourly, after.Take - before.Take);
        Assert.Equal(1, after.Bookings - before.Bookings);
        Assert.Equal(1, after.Days[^1].Bookings - before.Days[^1].Bookings);
    }

    // ------------------------------------------------------------------ active partners / venues / courts

    // The DB is shared and other tests (venue search) assert exact venue lists: everything created here is removed again.
    private readonly List<Guid> _partners = [], _venues = [];

    private Task CleanUpAsync() => api.WithDb(async db =>
    {
        foreach (var v in _venues) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE id = {v}");
        foreach (var p in _partners) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
    });

    private async Task<Guid> AddPartnerAsync(string status)
    {
        var id = Guid.NewGuid();
        _partners.Add(id);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO partners (id, legal_name, status) VALUES ({id}, 'Test Partner', {status})"));
        return id;
    }

    private async Task AddVenueAsync(Guid partner, string status, int activeCourts, int inactiveCourts)
    {
        var id = Guid.NewGuid();
        _venues.Add(id);
        var slug = "t-" + id.ToString("N");
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, status)
                VALUES ({id}, {partner}, {slug}, 'ทดสอบ', 'Test', 'Test', 'Test', 13.7, 100.5, {status})
                """);
            for (var i = 0; i < activeCourts + inactiveCourts; i++)
            {
                var active = i < activeCourts;
                var name = "C" + i;
                await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO courts (venue_id, name, surface, is_active) VALUES ({id}, {name}, 'hard', {active})");
            }
        });
    }

    [Fact]
    public async Task Active_counts_only_include_active_venues_of_active_partners_and_their_active_courts()
    {
        var admin = await AdminHttpAsync();
        var b0 = await GetAsync(admin);
        try
        {
            var live = await AddPartnerAsync("active");
            await AddVenueAsync(live, "active", activeCourts: 3, inactiveCourts: 2);
            await AddVenueAsync(live, "draft", 4, 0);
            await AddVenueAsync(live, "hidden", 4, 0);
            var b1 = await GetAsync(admin);
            Assert.Equal((1, 1, 3), (b1.Partners - b0.Partners, b1.Venues - b0.Venues, b1.Courts - b0.Courts));

            foreach (var status in new[] { "pending", "suspended" })
            {
                var p = await AddPartnerAsync(status);
                await AddVenueAsync(p, "active", 5, 0);
            }
            var b2 = await GetAsync(admin);
            Assert.Equal((b1.Partners, b1.Venues, b1.Courts), (b2.Partners, b2.Venues, b2.Courts));

            // suspending the partner takes its venue and courts out again; partner counter follows
            await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE partners SET status = 'suspended' WHERE id = {live}"));
            var b3 = await GetAsync(admin);
            Assert.Equal((b0.Partners, b0.Venues, b0.Courts), (b3.Partners, b3.Venues, b3.Courts));
        }
        finally { await CleanUpAsync(); }
    }
}
