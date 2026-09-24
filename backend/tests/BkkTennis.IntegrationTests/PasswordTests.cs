using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// F3: partner password change (signed in) and admin-issued reset links. Every partner/owner created here is removed in
/// <see cref="DisposeAsync"/>; the seeded owner/staff/admin passwords are never touched (other tests sign in with them).
/// The fake clock never ticks by itself: tests advance it between password changes because the session stamp is the change time in ms.
/// </summary>
[Collection(ApiCollection.Name)]
public class PasswordTests(ApiFactory api) : IAsyncLifetime
{
    private const string Partners = "/api/v1/admin/partners", StaffLogin = "/api/v1/auth/staff/login", AdminLogin = "/api/v1/auth/admin/login";
    private const string Change = "/api/v1/auth/password/change", Reset = "/api/v1/auth/password/reset";
    private const string Pw = "owner-pass-2026", NewPw = "brand-new-pass-1";
    private readonly List<Guid> _partners = [];
    private readonly int _errorsBefore = api.ServerErrors.Count;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        Assert.True(!api.ServerErrors.Skip(_errorsBefore).Any(), string.Join("\n", api.ServerErrors.Skip(_errorsBefore)));
        await api.WithDb(async db =>
        {
            foreach (var p in _partners)
            {
                var owners = await db.VenueMembers.Where(m => m.Role == "owner" && db.Venues.Any(v => v.Id == m.VenueId && v.PartnerId == p))
                    .Select(m => m.UserId).ToListAsync();
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venues WHERE partner_id = {p}");
                foreach (var u in owners) await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {u} AND role = 'partner_owner'");
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {p}");
            }
        });
    }

    // ------------------------------------------------------------------ helpers

    private sealed record Owner(Guid PartnerId, Guid UserId, string Email, HttpClient Http, string Token);

    private static HttpClient Bearer(HttpClient http, string? token)
    {
        if (token is not null) http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return http;
    }

    private async Task<HttpClient> AdminAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync(AdminLogin, new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        return Bearer(api.Anonymous(), (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
    }

    private async Task<(string? Token, JsonElement Body, HttpStatusCode Status)> StaffLoginAsync(string email, string password, HttpClient? http = null)
    {
        var res = await (http ?? api.Anonymous()).PostAsJsonAsync(StaffLogin, new { email, password });
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return (res.IsSuccessStatusCode ? body.GetProperty("accessToken").GetString() : null, body, res.StatusCode);
    }

    private async Task<Owner> NewOwnerAsync()
    {
        var t = Guid.NewGuid().ToString("N")[..10];
        var email = $"pw-{t}@example.test";
        var body = new JsonObject
        {
            ["legalName"] = "PW Partner " + t, ["taxId"] = "0105551234567", ["bankName"] = "KBank", ["bankAccount"] = "123-4-56789-0",
            ["owner"] = new JsonObject { ["email"] = email, ["displayName"] = "Owner " + t, ["password"] = Pw },
            ["venue"] = new JsonObject
            {
                ["slug"] = "pw-" + t, ["nameTh"] = "สนามทดสอบ " + t, ["nameEn"] = "PW Venue " + t, ["district"] = "Test", ["address"] = "1 Test Rd",
                ["lat"] = 14.6, ["lng"] = 101.3,
            },
        };
        var admin = await AdminAsync();
        var res = await admin.PostAsync(Partners, new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"));
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.Created, raw);
        var partnerId = JsonDocument.Parse(raw).RootElement.GetProperty("id").GetGuid();
        _partners.Add(partnerId);
        var (token, login, status) = await StaffLoginAsync(email, Pw);
        Assert.True(status == HttpStatusCode.OK, login.ToString());
        return new Owner(partnerId, login.GetProperty("user").GetProperty("id").GetGuid(), email, Bearer(api.Anonymous(), token), token!);
    }

    private static Task<HttpResponseMessage> ChangeAsync(HttpClient http, string current, string next) =>
        http.PostAsJsonAsync(Change, new { currentPassword = current, newPassword = next });

    private async Task<string> ErrorCodeAsync(HttpResponseMessage res, HttpStatusCode status)
    {
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == status, $"expected {(int)status} got {(int)res.StatusCode}: {raw}");
        return JsonDocument.Parse(raw).RootElement.GetProperty("code").GetString()!;
    }

    private async Task<HttpClient> WithTokenAsync(HttpResponseMessage okAuthResponse) =>
        Bearer(api.Anonymous(), (await okAuthResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());

    private static async Task<HttpStatusCode> MeAsync(HttpClient http) => (await http.GetAsync("/api/v1/auth/me")).StatusCode;

    private async Task<JsonElement> IssueLinkAsync(Owner o, HttpClient? admin = null)
    {
        var res = await (admin ?? await AdminAsync()).PostAsync($"{Partners}/{o.PartnerId}/owner/reset-link", null);
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.StatusCode == HttpStatusCode.OK, raw);
        return JsonDocument.Parse(raw).RootElement.Clone();
    }

    private static Task<HttpResponseMessage> ResetAsync(HttpClient http, string token, string password) =>
        http.PostAsJsonAsync(Reset, new { token, newPassword = password });

    private void Tick() => api.Clock.Advance(TimeSpan.FromMilliseconds(5));

    // ------------------------------------------------------------------ change: happy path & sessions

    [Fact]
    public async Task Change_succeeds_new_password_works_and_old_one_is_refused()
    {
        var o = await NewOwnerAsync();
        Tick();

        var res = await ChangeAsync(o.Http, Pw, NewPw);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.Equal(o.Email, body.GetProperty("user").GetProperty("email").GetString());
        Assert.DoesNotContain("password", body.ToString().Replace("passwordChanged", ""), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, NewPw)).Status);
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, Pw)).Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Old_token_dies_after_change_and_the_returned_token_keeps_the_session_alive()
    {
        var o = await NewOwnerAsync();
        Assert.Equal(HttpStatusCode.OK, await MeAsync(o.Http));
        Tick();

        var res = await ChangeAsync(o.Http, Pw, NewPw);
        var fresh = await WithTokenAsync(res);

        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(o.Http));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(fresh));
        // and the fresh token is a real portal session (venue scoped endpoints, not just /me)
        Assert.Equal(HttpStatusCode.OK, (await fresh.GetAsync("/api/v1/clubos/me/venues")).StatusCode);
    }

    [Fact]
    public async Task Other_sessions_are_logged_out_by_a_change()
    {
        var o = await NewOwnerAsync();
        var other = Bearer(api.Anonymous(), (await StaffLoginAsync(o.Email, Pw)).Token); // second device
        Assert.Equal(HttpStatusCode.OK, await MeAsync(other));
        Tick();

        Assert.Equal(HttpStatusCode.OK, (await ChangeAsync(o.Http, Pw, NewPw)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(other));
    }

    [Fact]
    public async Task Repeated_changes_each_return_a_working_token_whatever_the_sub_millisecond_clock_fraction()
    {
        // Probes the stamp: the token is minted from the in-memory time (100 ns ticks) but validated against the value read back
        // from Postgres (microseconds). Odd tick offsets would expose rounding differences at a millisecond boundary.
        var o = await NewOwnerAsync();
        var current = Pw;
        var http = o.Http;
        foreach (var ticks in new long[] { 1, 9_999, 5_000, 9_995, 4_999, 10_007, 19_999, 123_457 })
        {
            api.Clock.Advance(TimeSpan.FromMilliseconds(3) + TimeSpan.FromTicks(ticks));
            var next = $"round-{ticks}-pass-xx";
            var res = await ChangeAsync(http, current, next);
            Assert.True(res.StatusCode == HttpStatusCode.OK, $"ticks={ticks}: {await res.Content.ReadAsStringAsync()}");
            var fresh = await WithTokenAsync(res);
            Assert.True(await MeAsync(fresh) == HttpStatusCode.OK, $"fresh token rejected right after change (ticks={ticks})");
            http = fresh;
            current = next;
        }
    }

    // ------------------------------------------------------------------ change: errors

    [Fact]
    public async Task Wrong_current_password_is_400_wrong_password_and_changes_nothing()
    {
        var o = await NewOwnerAsync();

        Assert.Equal("wrong_password", await ErrorCodeAsync(await ChangeAsync(o.Http, "not-my-password", NewPw), HttpStatusCode.BadRequest));

        Assert.Equal(HttpStatusCode.OK, await MeAsync(o.Http)); // session untouched
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, Pw)).Status);
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, NewPw)).Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Five_wrong_current_passwords_lock_the_account_like_the_login_does()
    {
        var o = await NewOwnerAsync();

        for (var i = 0; i < 5; i++)
            Assert.Equal("wrong_password", await ErrorCodeAsync(await ChangeAsync(o.Http, "wrong-" + i, NewPw), HttpStatusCode.BadRequest));

        // locked: neither the login nor the change accepts the right password
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, Pw)).Body.GetProperty("code").GetString());
        Assert.Equal("wrong_password", await ErrorCodeAsync(await ChangeAsync(o.Http, Pw, NewPw), HttpStatusCode.BadRequest));

        api.Clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, Pw)).Status);
    }

    [Fact]
    public async Task Four_wrong_guesses_then_the_right_password_still_changes()
    {
        var o = await NewOwnerAsync();
        for (var i = 0; i < 4; i++) await ChangeAsync(o.Http, "wrong-" + i, NewPw);
        Tick();

        Assert.Equal(HttpStatusCode.OK, (await ChangeAsync(o.Http, Pw, NewPw)).StatusCode);
    }

    [Fact]
    public async Task Same_password_is_400_same_password()
    {
        var o = await NewOwnerAsync();

        Assert.Equal("same_password", await ErrorCodeAsync(await ChangeAsync(o.Http, Pw, Pw), HttpStatusCode.BadRequest));

        Assert.Equal(HttpStatusCode.OK, await MeAsync(o.Http)); // nothing was invalidated
    }

    [Theory]
    [InlineData("123456789", "newPassword")]                                // 9 chars
    [InlineData("", "newPassword")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "newPassword")] // 74 bytes
    public async Task Invalid_new_passwords_are_validation_errors(string next, string field)
    {
        var o = await NewOwnerAsync();

        var res = await ChangeAsync(o.Http, Pw, next);

        Assert.Equal("validation_failed", await ErrorCodeAsync(res, HttpStatusCode.BadRequest));
        Assert.True((await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors").TryGetProperty(field, out _));
    }

    [Fact]
    public async Task Multibyte_new_password_over_72_bytes_is_rejected_and_exactly_72_bytes_is_accepted()
    {
        var o = await NewOwnerAsync();
        Assert.Equal("validation_failed", await ErrorCodeAsync(await ChangeAsync(o.Http, Pw, new string('€', 25)), HttpStatusCode.BadRequest)); // 75 bytes
        Tick();

        var exact = new string('p', 72);
        var res = await ChangeAsync(o.Http, Pw, exact);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, exact)).Status);
    }

    [Fact]
    public async Task Missing_fields_are_validation_errors()
    {
        var o = await NewOwnerAsync();
        var res = await o.Http.PostAsync(Change, new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal("validation_failed", await ErrorCodeAsync(res, HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Anonymous_change_is_401_json()
    {
        var res = await ChangeAsync(api.Anonymous(), Pw, NewPw);

        Assert.Equal("unauthorized", await ErrorCodeAsync(res, HttpStatusCode.Unauthorized));
    }

    [Fact]
    public async Task A_player_without_a_password_gets_400_no_password()
    {
        var player = await api.LoginAsync();

        Assert.Equal("no_password", await ErrorCodeAsync(await ChangeAsync(player.Http, "whatever-123", NewPw), HttpStatusCode.BadRequest));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(player.Http)); // players' OTP sessions are unaffected
    }

    [Fact]
    public async Task A_partner_without_a_password_hash_gets_400_no_password_and_stays_signed_in()
    {
        var staff = await api.LoginPartnerAsync(Seed.VenueId);

        Assert.Equal("no_password", await ErrorCodeAsync(await ChangeAsync(staff.Http, "whatever-123", NewPw), HttpStatusCode.BadRequest));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(staff.Http));
    }

    [Fact]
    public async Task Garbage_and_stampless_tokens_are_handled()
    {
        var bad = Bearer(api.Anonymous(), "not.a.jwt");
        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(bad));
    }

    // ------------------------------------------------------------------ admin issues a link

    [Fact]
    public async Task Admin_issues_a_link_with_url_token_and_24h_expiry()
    {
        var o = await NewOwnerAsync();
        var body = await IssueLinkAsync(o);

        var token = body.GetProperty("token").GetString()!;
        Assert.True(token.Length >= 40, token);
        Assert.Matches("^[A-Za-z0-9_-]+$", token); // url safe
        Assert.Equal($"http://localhost:9001/reset-password?token={token}", body.GetProperty("url").GetString());
        var expires = body.GetProperty("expiresAt").GetDateTime();
        var expected = api.Clock.GetUtcNow().UtcDateTime.AddHours(24);
        Assert.InRange((expires - expected).TotalSeconds, -2, 2);
    }

    [Fact]
    public async Task Only_admins_may_issue_links()
    {
        var o = await NewOwnerAsync();
        var url = $"{Partners}/{o.PartnerId}/owner/reset-link";

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.Anonymous().PostAsync(url, null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await o.Http.PostAsync(url, null)).StatusCode);            // the owner themselves
        var player = await api.LoginAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await player.Http.PostAsync(url, null)).StatusCode);
        var other = await NewOwnerAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Http.PostAsync(url, null)).StatusCode);        // another partner's owner
        Assert.Equal(0, await CountTokensAsync(o.UserId));
    }

    [Fact]
    public async Task Unknown_partner_is_404_and_partner_without_an_owner_account_is_409_no_owner()
    {
        var admin = await AdminAsync();
        var missing = await admin.PostAsync($"{Partners}/{Guid.NewGuid()}/owner/reset-link", null);
        Assert.Equal("not_found", await ErrorCodeAsync(missing, HttpStatusCode.NotFound));

        var bare = Guid.NewGuid();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO partners (id, legal_name) VALUES ({bare}, 'Bare partner')"));
        try
        {
            Assert.Equal("no_owner", await ErrorCodeAsync(await admin.PostAsync($"{Partners}/{bare}/owner/reset-link", null), HttpStatusCode.Conflict));
        }
        finally
        {
            await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM partners WHERE id = {bare}"));
        }
    }

    [Fact]
    public async Task Owner_without_a_password_hash_counts_as_no_owner()
    {
        var o = await NewOwnerAsync();
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE users SET password_hash = NULL WHERE id = {o.UserId}"));

        var res = await (await AdminAsync()).PostAsync($"{Partners}/{o.PartnerId}/owner/reset-link", null);

        Assert.Equal("no_owner", await ErrorCodeAsync(res, HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Token_is_never_stored_in_plaintext()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;

        var rows = new List<(string Hash, string Row)>();
        await api.WithDb(async db =>
        {
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT token_hash, t::text FROM password_reset_tokens t WHERE user_id = '{o.UserId}'";
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) rows.Add((r.GetString(0), r.GetString(1)));
            await conn.CloseAsync();
        });

        var row = Assert.Single(rows);
        Assert.NotEqual(token, row.Hash);
        Assert.DoesNotContain(token, row.Row);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))), row.Hash, ignoreCase: true);
    }

    private async Task<int> CountTokensAsync(Guid userId)
    {
        var n = 0;
        await api.WithDb(async db => n = await db.PasswordResetTokens.CountAsync(t => t.UserId == userId));
        return n;
    }

    // ------------------------------------------------------------------ reset flow

    [Fact]
    public async Task Reset_end_to_end_new_password_works_old_one_and_old_sessions_do_not()
    {
        var o = await NewOwnerAsync();
        Assert.Equal(HttpStatusCode.OK, await MeAsync(o.Http));
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();

        var res = await ResetAsync(api.Anonymous(), token, NewPw);

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, NewPw)).Status);
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, Pw)).Body.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(o.Http)); // session from before the reset
    }

    [Fact]
    public async Task Reset_token_is_single_use()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();
        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), token, NewPw)).StatusCode);

        var again = await ResetAsync(api.Anonymous(), token, "attacker-chosen-pass");

        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(again, HttpStatusCode.BadRequest));
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, NewPw)).Status);
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, "attacker-chosen-pass")).Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Concurrent_double_use_lets_exactly_one_request_win()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(i => ResetAsync(api.Anonymous(), token, $"racer-{i}-password")));

        Assert.Equal(1, results.Count(r => r.StatusCode == HttpStatusCode.NoContent));
        foreach (var r in results.Where(r => r.StatusCode != HttpStatusCode.NoContent))
            Assert.Equal("reset_token_invalid", await ErrorCodeAsync(r, HttpStatusCode.BadRequest));
        // exactly one of the racing passwords is now the password
        // (checked against the stored hash: probing through the login would trip the 5-failure lockout)
        var winners = 0;
        await api.WithDb(async db =>
        {
            var hash = (await db.Users.AsNoTracking().SingleAsync(x => x.Id == o.UserId)).PasswordHash!;
            for (var i = 0; i < 8; i++)
                if (BCrypt.Net.BCrypt.Verify($"racer-{i}-password", hash)) winners++;
        });
        Assert.Equal(1, winners);
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        var past = api.Clock.GetUtcNow().UtcDateTime.AddSeconds(-1);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE password_reset_tokens SET expires_at = {past} WHERE user_id = {o.UserId}"));

        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, NewPw), HttpStatusCode.BadRequest));
        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, Pw)).Status); // unchanged
    }

    [Fact]
    public async Task Token_expires_after_24_hours_of_clock_time()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;

        api.Clock.Advance(TimeSpan.FromHours(24) + TimeSpan.FromSeconds(1));

        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, NewPw), HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task A_new_link_voids_the_previous_one()
    {
        var o = await NewOwnerAsync();
        var admin = await AdminAsync();
        var first = (await IssueLinkAsync(o, admin)).GetProperty("token").GetString()!;
        var second = (await IssueLinkAsync(o, admin)).GetProperty("token").GetString()!;
        Assert.NotEqual(first, second);
        Tick();

        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), first, NewPw), HttpStatusCode.BadRequest));
        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), second, NewPw)).StatusCode);
    }

    [Fact]
    public async Task Using_a_link_voids_other_outstanding_links_and_a_normal_change_does_too()
    {
        var o = await NewOwnerAsync();
        // two outstanding tokens for the same user: insert a second one by hand (the API always voids the previous one)
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        var extra = "extra-" + Guid.NewGuid().ToString("N");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(extra)));
        var exp = api.Clock.GetUtcNow().UtcDateTime.AddHours(5);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO password_reset_tokens (user_id, token_hash, expires_at) VALUES ({o.UserId}, {hash}, {exp})"));
        Tick();

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), token, NewPw)).StatusCode);

        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), extra, "another-new-pass"), HttpStatusCode.BadRequest));

        // a password change (not a reset) also kills a link the admin issued earlier
        var pending = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        var login = await StaffLoginAsync(o.Email, NewPw);
        Tick();
        Assert.Equal(HttpStatusCode.OK, (await ChangeAsync(Bearer(api.Anonymous(), login.Token), NewPw, "changed-after-link-1")).StatusCode);
        Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), pending, "yet-another-pass1"), HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Reset_clears_the_lockout()
    {
        var o = await NewOwnerAsync();
        for (var i = 0; i < 5; i++) await StaffLoginAsync(o.Email, "wrong-" + i);
        Assert.Equal("invalid_credentials", (await StaffLoginAsync(o.Email, Pw)).Body.GetProperty("code").GetString()); // locked
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), token, NewPw)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await StaffLoginAsync(o.Email, NewPw)).Status); // no 15 minute wait
        await api.WithDb(async db =>
        {
            var u = await db.Users.AsNoTracking().SingleAsync(x => x.Id == o.UserId);
            Assert.Equal(0, u.FailedLogins);
            Assert.Null(u.LockedUntil);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData("nonsense")]
    [InlineData("mock-reset.b3duZXI.99999999999999")]
    public async Task Unknown_tokens_are_400_reset_token_invalid(string token)
    {
        var res = await ResetAsync(api.Anonymous(), token, NewPw);

        // an empty token fails DataAnnotations first; everything else is the generic invalid error
        Assert.Equal(token == "" ? "validation_failed" : "reset_token_invalid", await ErrorCodeAsync(res, HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task Token_is_trimmed_and_a_failed_reset_does_not_burn_it()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();

        // too-short password fails validation before the token is looked at, so the link stays usable
        Assert.Equal("validation_failed", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, "short"), HttpStatusCode.BadRequest));

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), $"  {token}  ", NewPw)).StatusCode);
    }

    [Fact]
    public async Task Reset_password_validation_matches_change_rules()
    {
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;

        Assert.Equal("validation_failed", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, new string('€', 25)), HttpStatusCode.BadRequest));
        Assert.Equal("validation_failed", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, new string('a', 129)), HttpStatusCode.BadRequest));
        Assert.Equal("validation_failed", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), new string('t', 101), NewPw), HttpStatusCode.BadRequest));
    }

    [Theory]
    [InlineData("player")]
    [InlineData("platform_admin")]
    public async Task Reset_links_only_work_for_partner_accounts(string role)
    {
        // a token row for a non-partner (cannot be issued through the API; defence in depth): must not take the account over
        var id = Guid.NewGuid();
        var email = $"np-{id:N}@example.test";
        var token = "np-" + id.ToString("N");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var exp = api.Clock.GetUtcNow().UtcDateTime.AddHours(1);
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO users (id, email, role, display_name, password_hash) VALUES ({id}, {email}, {role}, 'x', crypt('original-pass-1', gen_salt('bf', 4)))");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO password_reset_tokens (user_id, token_hash, expires_at) VALUES ({id}, {hash}, {exp})");
        });
        try
        {
            Assert.Equal("reset_token_invalid", await ErrorCodeAsync(await ResetAsync(api.Anonymous(), token, NewPw), HttpStatusCode.BadRequest));
            await api.WithDb(async db =>
            {
                var u = await db.Users.AsNoTracking().SingleAsync(x => x.Id == id);
                Assert.True(BCrypt.Net.BCrypt.Verify("original-pass-1", u.PasswordHash));
                Assert.Null(u.PasswordChangedAt);
            });
        }
        finally
        {
            await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {id}"));
        }
    }

    // ------------------------------------------------------------------ session stamp: regressions on other token kinds

    [Fact]
    public async Task Admin_token_for_a_reset_owner_is_unaffected_and_other_partners_are_not_logged_out()
    {
        var a = await NewOwnerAsync();
        var b = await NewOwnerAsync();
        var admin = await AdminAsync();
        var token = (await IssueLinkAsync(a, admin)).GetProperty("token").GetString()!;
        Tick();

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(api.Anonymous(), token, NewPw)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, await MeAsync(admin));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(b.Http));
    }

    [Fact]
    public async Task Otp_player_tokens_survive_partner_password_resets()
    {
        var player = await api.LoginAsync();
        var o = await NewOwnerAsync();
        var token = (await IssueLinkAsync(o)).GetProperty("token").GetString()!;
        Tick();
        await ResetAsync(api.Anonymous(), token, NewPw);

        Assert.Equal(HttpStatusCode.OK, await MeAsync(player.Http));
    }

    [Fact]
    public async Task A_deleted_user_s_token_is_rejected()
    {
        var o = await NewOwnerAsync();
        var extra = await NewOwnerAsync();
        await api.WithDb(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM venue_members WHERE user_id = {extra.UserId}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM users WHERE id = {extra.UserId}");
        });

        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(extra.Http));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(o.Http));
    }

    [Fact]
    public async Task A_token_without_the_stamp_claim_is_rejected_for_users_with_a_password()
    {
        // forge a validly signed token that lacks "pwd" (e.g. issued before this feature) for a partner with a password
        var o = await NewOwnerAsync();
        // never-changed password: a missing claim reads as "0" and matches (documented legacy behaviour)
        Assert.Equal(HttpStatusCode.OK, await MeAsync(Bearer(api.Anonymous(), ForgeToken(o.UserId, "partner_owner", stamp: null))));
        Tick();
        Assert.Equal(HttpStatusCode.OK, (await ChangeAsync(o.Http, Pw, NewPw)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(Bearer(api.Anonymous(), ForgeToken(o.UserId, "partner_owner", stamp: null))));
    }

    [Fact]
    public async Task A_token_with_a_wrong_stamp_is_rejected_and_the_right_one_accepted()
    {
        var o = await NewOwnerAsync(); // password never changed => stamp "0"
        Assert.Equal(HttpStatusCode.OK, await MeAsync(Bearer(api.Anonymous(), ForgeToken(o.UserId, "partner_owner", "0"))));
        Assert.Equal(HttpStatusCode.Unauthorized, await MeAsync(Bearer(api.Anonymous(), ForgeToken(o.UserId, "partner_owner", "12345"))));
    }

    [Fact]
    public async Task Seeded_accounts_keep_working_with_stamped_tokens()
    {
        var owner = (await StaffLoginAsync("owner@racquetclub.test", "racquet-dev-2026")).Token;
        Assert.Equal(HttpStatusCode.OK, await MeAsync(Bearer(api.Anonymous(), owner)));
        Assert.Equal(HttpStatusCode.OK, await MeAsync(await AdminAsync()));
    }

    private string ForgeToken(Guid userId, string role, string? stamp)
    {
        var opts = api.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<BkkTennis.Api.Features.Auth.JwtOptions>>().Value;
        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Role, role),
        };
        if (stamp is not null) claims.Add(new("pwd", stamp));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey)),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var now = api.Clock.GetUtcNow().UtcDateTime;
        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(
            new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(opts.Issuer, opts.Audience, claims, now, now.AddHours(1), creds));
    }
}
