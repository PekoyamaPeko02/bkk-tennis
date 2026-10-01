using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace BkkTennis.IntegrationTests;

/// <summary>C1: partner staff sign in with email + password (BCrypt) and get the same JWT the OTP flow issues.</summary>
[Collection(ApiCollection.Name)]
public class StaffLoginTests(ApiFactory api)
{
    private const string Login = "/api/v1/auth/staff/login";
    // Seeded by db/init/004_seed_staff.sql (dev/test only)
    private const string OwnerEmail = "owner@racquetclub.test", StaffEmail = "staff@racquetclub.test", SeedPassword = "racquet-dev-2026";
    private static readonly Guid Venue = Seed.VenueId;

    private static string Email() => $"staff-{Guid.NewGuid():N}@example.test";

    private async Task<HttpResponseMessage> LoginAsync(string email, string password, HttpClient? http = null) =>
        await (http ?? api.Anonymous()).PostAsJsonAsync(Login, new { email, password });

    private async Task<TestUser> AsAsync(string email, string password)
    {
        var res = await LoginAsync(email, password);
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var http = api.Anonymous();
        http.DefaultRequestHeaders.Authorization = new("Bearer", body.GetProperty("accessToken").GetString());
        return new TestUser(http, body.GetProperty("user").GetProperty("id").GetGuid(), "");
    }

    /// <summary>Creates a user with a bcrypt password hash made by pgcrypto (same generator the seed uses).</summary>
    private async Task<string> CreateUserAsync(string role = "partner_staff", string password = "correct-horse-1", string? email = null,
        bool withPassword = true, Guid? memberOf = null)
    {
        email ??= Email();
        var id = Guid.NewGuid();
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO users (id, email, role, display_name, password_hash)
                VALUES ({id}, {email}, {role}, 'Test Staff', CASE WHEN {withPassword} THEN crypt({password}, gen_salt('bf', 4)) END)
                """);
            if (memberOf is { } v)
                await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO venue_members (venue_id, user_id, role) VALUES ({v}, {id}, 'staff')");
        });
        return email;
    }

    private static async Task AssertInvalidCredentialsAsync(HttpResponseMessage res)
    {
        var raw = await res.Content.ReadAsStringAsync();
        // 400, not 401: the client treats any 401 as "session expired" and would bounce the login form itself.
        Assert.True(res.StatusCode == HttpStatusCode.BadRequest, $"expected 400 got {(int)res.StatusCode}: {raw}");
        Assert.Equal("invalid_credentials", JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString());
    }

    // ------------------------------------------------------------------ happy paths (also proves BCrypt.Net verifies pgcrypto hashes)

    [Fact]
    public async Task Seeded_owner_can_sign_in_and_use_the_portal()
    {
        var res = await LoginAsync(OwnerEmail, SeedPassword);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.True(body.GetProperty("expiresAt").GetDateTime() > api.Clock.GetUtcNow().UtcDateTime);
        var user = body.GetProperty("user");
        Assert.Equal("partner_owner", user.GetProperty("role").GetString());
        Assert.Equal(OwnerEmail, user.GetProperty("email").GetString());

        var owner = await AsAsync(OwnerEmail, SeedPassword);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync($"/api/v1/clubos/venues/{Venue}/prices")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.Http.GetAsync("/api/v1/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Seeded_front_desk_staff_can_read_but_not_change_prices()
    {
        var staff = await AsAsync(StaffEmail, SeedPassword);

        Assert.Equal(HttpStatusCode.OK, (await staff.Http.GetAsync($"/api/v1/clubos/venues/{Venue}/prices")).StatusCode);
        var write = await staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{Venue}/prices",
            new { dayType = "holiday", start = "03:00", end = "04:00", pricePerHour = 100, priority = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Theory]
    [InlineData("Owner@RacquetClub.TEST")]
    [InlineData("  owner@racquetclub.test  ")]
    public async Task Email_is_trimmed_and_case_insensitive(string email)
    {
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, SeedPassword)).StatusCode);
    }

    [Fact]
    public async Task Passwords_are_case_sensitive_and_not_trimmed()
    {
        var email = await CreateUserAsync(password: SeedPassword); // own account: failures must not count against the shared seeded owner
        await AssertInvalidCredentialsAsync(await LoginAsync(email, SeedPassword.ToUpperInvariant()));
        await AssertInvalidCredentialsAsync(await LoginAsync(email, " " + SeedPassword));
    }

    [Fact]
    public async Task Staff_can_still_sign_in_with_otp()
    {
        var res = await api.LoginAsync(phone: "0800000004"); // the seeded front-desk user's phone

        Assert.Equal("partner_staff", (await res.Http.GetFromJsonAsync<JsonElement>("/api/v1/auth/me")).GetProperty("role").GetString());
    }

    // ------------------------------------------------------------------ one error for every failure

    [Fact]
    public async Task Wrong_password_and_unknown_email_are_indistinguishable()
    {
        var wrong = await LoginAsync(await CreateUserAsync(), "nope-nope-nope");
        var unknown = await LoginAsync("nobody@example.test", "nope-nope-nope");

        await AssertInvalidCredentialsAsync(wrong);
        await AssertInvalidCredentialsAsync(unknown);
        Assert.Equal(await wrong.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Accounts_without_a_password_cannot_sign_in()
    {
        var email = await CreateUserAsync(withPassword: false);

        await AssertInvalidCredentialsAsync(await LoginAsync(email, "anything"));
        await AssertInvalidCredentialsAsync(await LoginAsync(email, "$2a$10$abcdefghijklmnopqrstuv")); // a hash is not a password
    }

    [Theory]
    [InlineData("player")]
    [InlineData("platform_admin")] // admins get their own login (A1); this endpoint is for partner staff only
    public async Task Only_partner_roles_may_use_the_staff_login(string role)
    {
        var email = await CreateUserAsync(role: role, password: "correct-horse-1");

        await AssertInvalidCredentialsAsync(await LoginAsync(email, "correct-horse-1"));
    }

    [Fact]
    public async Task Password_hash_is_never_returned()
    {
        var res = await LoginAsync(OwnerEmail, SeedPassword);
        var raw = await res.Content.ReadAsStringAsync();

        Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$2", raw);
        var me = await (await AsAsync(OwnerEmail, SeedPassword)).Http.GetStringAsync("/api/v1/auth/me");
        Assert.DoesNotContain("password", me, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------------ validation

    [Theory]
    [InlineData("""{ }""", "email")]
    [InlineData("""{ "email": "a@b.test" }""", "password")]
    [InlineData("""{ "email": "", "password": "x" }""", "email")]
    [InlineData("""{ "email": "a@b.test", "password": "" }""", "password")]
    public async Task Missing_fields_are_validation_errors(string raw, string field)
    {
        var res = await api.Anonymous().PostAsync(Login, new StringContent(raw, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty(field, out _), body.ToString());
    }

    [Fact]
    public async Task Absurdly_long_inputs_are_rejected_before_hashing()
    {
        var res = await LoginAsync(new string('a', 400) + "@example.test", new string('p', 5000));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("validation_failed", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    // ------------------------------------------------------------------ brute-force protection

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_for_fifteen_minutes()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");

        for (var i = 0; i < 5; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong-" + i));

        // even the right password is refused while locked (same generic error: no oracle for "account exists")
        await AssertInvalidCredentialsAsync(await LoginAsync(email, "correct-horse-1"));

        api.Clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, "correct-horse-1")).StatusCode);
    }

    [Fact]
    public async Task A_successful_login_resets_the_failure_counter()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");

        for (var round = 0; round < 3; round++)
        {
            for (var i = 0; i < 4; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong"));
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, "correct-horse-1")).StatusCode); // 4 misses < 5, then success resets
        }
    }

    [Fact]
    public async Task Failures_against_an_unknown_email_do_not_create_state_or_errors()
    {
        for (var i = 0; i < 7; i++) await AssertInvalidCredentialsAsync(await LoginAsync("ghost@example.test", "x" + i));
    }

    [Fact]
    public async Task Staff_login_shares_the_per_ip_limiter_with_otp()
    {
        using var limited = api.WithWebHostBuilder(b =>
        {
            b.UseSetting("RateLimit:Otp:PermitLimit", "3");
            b.UseSetting("RateLimit:Otp:WindowSeconds", "30");
        });
        var http = limited.CreateClient();
        var email = await CreateUserAsync();

        for (var i = 0; i < 3; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong", http));
        var blocked = await LoginAsync(email, "correct-horse-1", http);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("rate_limited", (await blocked.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task A_token_from_staff_login_is_a_normal_session_token()
    {
        var email = await CreateUserAsync(memberOf: Venue);
        var staff = await AsAsync(email, "correct-horse-1");

        Assert.Equal(HttpStatusCode.OK, (await staff.Http.GetAsync($"/api/v1/clubos/venues/{Venue}/bookings?date=2030-01-01")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.Http.GetAsync($"/api/v1/clubos/venues/{Guid.NewGuid()}/bookings?date=2030-01-01")).StatusCode);
    }

    // ------------------------------------------------------------------ probes (lock state, concurrency, JWT, email, bcrypt limits)

    private async Task<(int Failed, DateTime? LockedUntil)> LockStateAsync(string email)
    {
        (int, DateTime?) state = default;
        await api.WithDb(async db =>
        {
            var u = await db.Users.AsNoTracking().SingleAsync(x => x.Email == email);
            state = (u.FailedLogins, u.LockedUntil);
        });
        return state;
    }

    [Fact]
    public async Task Parallel_wrong_guesses_cannot_slip_past_the_counter()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(i => LoginAsync(email, "guess-" + i)));

        foreach (var r in results) await AssertInvalidCredentialsAsync(r);
        // 10 atomic increments = two full rounds of 5: no lost updates, and the account ends locked.
        var (failed, lockedUntil) = await LockStateAsync(email);
        Assert.Equal(0, failed);
        Assert.NotNull(lockedUntil);
        await AssertInvalidCredentialsAsync(await LoginAsync(email, "correct-horse-1"));
    }

    [Fact]
    public async Task Fifth_failure_locks_and_fourth_does_not()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");

        await Task.WhenAll(Enumerable.Range(0, 4).Select(i => LoginAsync(email, "guess-" + i)));
        Assert.Equal((4, (DateTime?)null), await LockStateAsync(email));

        await LoginAsync(email, "guess-x");
        var (failed, lockedUntil) = await LockStateAsync(email);
        Assert.Equal(0, failed);
        Assert.NotNull(lockedUntil);
    }

    [Fact]
    public async Task Attempts_while_locked_do_not_change_the_counter_or_extend_the_lock()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");
        for (var i = 0; i < 5; i++) await LoginAsync(email, "wrong");
        var locked = await LockStateAsync(email);
        Assert.Equal(0, locked.Failed);
        Assert.NotNull(locked.LockedUntil);

        api.Clock.Advance(TimeSpan.FromMinutes(1));
        for (var i = 0; i < 3; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "wrong"));

        Assert.Equal(locked, await LockStateAsync(email));
    }

    [Fact]
    public async Task Success_after_the_lock_expires_clears_lock_state()
    {
        var email = await CreateUserAsync(password: "correct-horse-1");
        for (var i = 0; i < 5; i++) await LoginAsync(email, "wrong");
        api.Clock.Advance(TimeSpan.FromMinutes(16));
        await LoginAsync(email, "wrong");
        Assert.Equal(1, (await LockStateAsync(email)).Failed);

        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, "correct-horse-1")).StatusCode);

        Assert.Equal((0, (DateTime?)null), await LockStateAsync(email));
    }

    [Fact]
    public async Task Staff_role_without_a_password_hash_is_refused_and_not_counted()
    {
        var email = await CreateUserAsync(role: "partner_staff", withPassword: false);

        for (var i = 0; i < 6; i++) await AssertInvalidCredentialsAsync(await LoginAsync(email, "x"));

        Assert.Equal((0, (DateTime?)null), await LockStateAsync(email));
    }

    [Fact]
    public async Task Jwt_from_staff_login_carries_sub_and_a_single_role_claim()
    {
        var body = await (await LoginAsync(OwnerEmail, SeedPassword)).Content.ReadFromJsonAsync<JsonElement>();
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(body.GetProperty("accessToken").GetString());

        Assert.Equal(body.GetProperty("user").GetProperty("id").GetString(), jwt.Subject);
        var roles = jwt.Claims.Where(c => c.Type is "role" or System.Security.Claims.ClaimTypes.Role).ToList();
        Assert.Equal("partner_owner", Assert.Single(roles).Value);
    }

    [Fact]
    public async Task Db_rejects_mixed_case_emails()
    {
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO users (id, email, role) VALUES ({Guid.NewGuid()}, 'Mixed.Case@Example.test', 'player')")));

        Assert.Contains("users_email_lowercase", ex.ToString());
    }

    [Fact]
    public async Task Unicode_email_signs_in_regardless_of_input_case()
    {
        var email = $"ผู้ใช้-{Guid.NewGuid():N}@example.test";
        await CreateUserAsync(email: email, password: "correct-horse-1");

        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email.ToUpperInvariant(), "correct-horse-1")).StatusCode);
    }

    // bcrypt only uses the first 72 bytes, so a longer password would be "accepted by its prefix". The API therefore
    // refuses passwords over 72 UTF-8 bytes outright (whoever sets passwords later must apply the same rule).
    [Theory]
    [InlineData(73)]              // one byte too many
    [InlineData(128)]             // the character cap
    public async Task Passwords_longer_than_72_bytes_are_rejected_instead_of_silently_truncated(int length)
    {
        var email = await CreateUserAsync(password: new string('a', 72));

        var res = await LoginAsync(email, new string('a', length)); // shares the first 72 bytes with the real password

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("password", out _), body.ToString());
    }

    [Fact]
    public async Task The_limit_counts_bytes_not_characters_and_72_bytes_still_works()
    {
        var exact = new string('a', 72);
        var email = await CreateUserAsync(password: exact);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, exact)).StatusCode);

        // 25 × '€' is 25 characters but 75 bytes
        var res = await LoginAsync(email, new string('€', 25));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("validation_failed", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }
}
