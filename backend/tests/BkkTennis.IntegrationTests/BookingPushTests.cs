using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Auth;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.Api.Domain;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// P2: /hubs/bookings pushes `bookingChanged` { bookingId, status } to the booking's owner (group user:{id}) — and to nobody else —
/// whenever the booking changes state. Every test uses REAL SignalR connections (long polling over the in-memory TestServer).
/// </summary>
[Collection(ApiCollection.Name)]
public class BookingPushTests(ApiFactory api)
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(5);
    private const string HubUrl = "/hubs/bookings";
    private readonly int _errorsAtStart = api.ServerErrors.Count; // the queue is shared by the whole collection: only count new ones

    private sealed record Push(Guid BookingId, string Status, string[] Properties);

    private sealed class Listener : IAsyncDisposable
    {
        private readonly Channel<Push> _events = Channel.CreateUnbounded<Push>();
        public HubConnection Conn { get; }

        private Listener(HubConnection conn) => Conn = conn;

        public static HubConnection Build(ApiFactory api, string? token, bool viaHeader = false) =>
            new HubConnectionBuilder()
                .WithUrl(new Uri(api.Server.BaseAddress, HubUrl), o =>
                {
                    o.Transports = HttpTransportType.LongPolling;
                    o.HttpMessageHandlerFactory = _ => api.Server.CreateHandler();
                    if (token is not null)
                    {
                        if (viaHeader) o.Headers["Authorization"] = "Bearer " + token;
                        else o.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    }
                })
                .Build();

        public static async Task<Listener> StartAsync(ApiFactory api, string token)
        {
            var l = new Listener(Build(api, token));
            l.Conn.On<JsonElement>("bookingChanged", m => l._events.Writer.TryWrite(new Push(
                m.GetProperty("bookingId").GetGuid(),
                m.GetProperty("status").GetString()!,
                m.EnumerateObject().Select(p => p.Name).Order().ToArray())));
            await l.Conn.StartAsync();
            return l;
        }

        public async Task<Push> NextAsync()
        {
            using var cts = new CancellationTokenSource(Wait);
            try { return await _events.Reader.ReadAsync(cts.Token); }
            catch (OperationCanceledException) { throw new Xunit.Sdk.XunitException("no bookingChanged arrived within " + Wait); }
        }

        public async Task<List<Push>> DrainAsync(TimeSpan? quiet = null)
        {
            await Task.Delay(quiet ?? TimeSpan.FromMilliseconds(400));
            var all = new List<Push>();
            while (_events.Reader.TryRead(out var e)) all.Add(e);
            return all;
        }

        public ValueTask DisposeAsync() => Conn.DisposeAsync();
    }

    private static string Token(TestUser u) => u.Http.DefaultRequestHeaders.Authorization!.Parameter!;

    private Task<Listener> ListenAsync(TestUser u) => Listener.StartAsync(api, Token(u));

    private async Task<string> AdminTokenAsync()
    {
        var res = await api.Anonymous().PostAsJsonAsync("/api/v1/auth/admin/login", new { email = "admin@bkktennis.test", password = "admin-dev-2026" });
        Assert.True(res.StatusCode == HttpStatusCode.OK, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }

    private Task<int> ExpireAsync() =>
        api.WithScope(sp => sp.GetRequiredService<BookingService>().ExpireHoldsAsync(CancellationToken.None));

    private Task<(int NoShows, int Completed)> SettleAsync() =>
        api.WithScope(sp => sp.GetRequiredService<BookingService>().SettleBookingsAsync(CancellationToken.None));

    private Task<string> DbStatusAsync(Guid id) =>
        api.WithScope(async sp => (await sp.GetRequiredService<AppDbContext>().Bookings.AsNoTracking().SingleAsync(b => b.Id == id)).Status);

    private async Task AssertPush(Listener l, Guid bookingId, string status)
    {
        var p = await l.NextAsync();
        Assert.Equal(bookingId, p.BookingId);
        Assert.Equal(status, p.Status);
    }

    private Task<HttpResponseMessage> Webhook(string providerRef)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/webhook/mock")
        {
            Content = JsonContent.Create(new { providerRef, status = "succeeded" }),
        };
        msg.Headers.Add("X-Webhook-Secret", "dev-webhook-secret");
        return api.Anonymous().SendAsync(msg);
    }

    private Task<string> ProviderRefAsync(Guid bookingId) =>
        api.WithScope(async sp =>
            (await sp.GetRequiredService<AppDbContext>().Payments.AsNoTracking().SingleAsync(p => p.BookingId == bookingId)).ProviderRef);

    private DateOnly InTwoDays => DateOnly.FromDateTime(api.Clock.GetUtcNow().UtcDateTime).AddDays(2);

    /// <summary>A confirmed app booking at the private venue that belongs to <paramref name="player"/>.</summary>
    private async Task<Guid> OwnedBookingAsync(ScheduleVenue v, TestUser player, DateOnly date, string start, Guid? court = null)
    {
        var id = await v.AddBookingAsync(court ?? v.CourtA, date, [start]);
        await api.WithDb(db => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE bookings SET user_id = {player.Id} WHERE id = {id}"));
        return id;
    }

    private void GoTo(DateOnly date, string time, int addMinutes = 0) =>
        api.Clock.SetUtcNow(new DateTimeOffset(ScheduleVenue.Utc(date, time).AddMinutes(addMinutes), TimeSpan.Zero));

    private void AssertNoServerErrors() => Assert.True(api.ServerErrors.Count == _errorsAtStart, string.Join("\n", api.ServerErrors.Skip(_errorsAtStart)));

    // ------------------------------------------------------------------ connecting

    [Fact]
    public async Task Anonymous_connection_is_rejected()
    {
        await using var conn = Listener.Build(api, token: null);
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => conn.StartAsync());
        Assert.Contains("401", ex.Message);
        Assert.NotEqual(HubConnectionState.Connected, conn.State);
    }

    [Fact]
    public async Task Garbage_token_is_rejected()
    {
        await using var conn = Listener.Build(api, "not.a.jwt");
        await Assert.ThrowsAnyAsync<Exception>(() => conn.StartAsync());
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var user = await api.LoginAsync();
        var jwt = api.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var forged = new JwtOptions { Issuer = jwt.Issuer, Audience = jwt.Audience, SigningKey = "another-signing-key-0123456789-abcdef" };
        var token = AuthEndpoints.Issue(new User { Id = user.Id, Role = "player", Phone = user.Phone }, forged, api.Clock.GetUtcNow().UtcDateTime).AccessToken;

        await using var conn = Listener.Build(api, token);
        await Assert.ThrowsAnyAsync<Exception>(() => conn.StartAsync());
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        var user = await api.LoginAsync();
        var jwt = api.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var old = AuthEndpoints.Issue(new User { Id = user.Id, Role = "player", Phone = user.Phone }, jwt,
            api.Clock.GetUtcNow().UtcDateTime.AddDays(-(jwt.AccessTokenDays + 2))).AccessToken;

        await using var conn = Listener.Build(api, old);
        await Assert.ThrowsAnyAsync<Exception>(() => conn.StartAsync());
    }

    [Fact]
    public async Task Real_login_token_connects_via_access_token_query_and_via_header()
    {
        var user = await api.LoginAsync();

        await using var viaQuery = Listener.Build(api, Token(user));
        await viaQuery.StartAsync();
        Assert.Equal(HubConnectionState.Connected, viaQuery.State);

        await using var viaHeader = Listener.Build(api, Token(user), viaHeader: true);
        await viaHeader.StartAsync();
        Assert.Equal(HubConnectionState.Connected, viaHeader.State);
    }

    // ------------------------------------------------------------------ what pushes

    [Fact]
    public async Task Hold_sends_nothing_then_payment_pushes_confirmed_exactly_once_even_when_the_webhook_is_replayed()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);

        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        Assert.Empty(await l.DrainAsync()); // creating a hold is silent: the client already has the response

        var providerRef = await ProviderRefAsync(held.Id());
        Assert.Equal(HttpStatusCode.OK, (await Webhook(providerRef)).StatusCode);
        var push = await l.NextAsync();
        Assert.Equal(held.Id(), push.BookingId);
        Assert.Equal("confirmed", push.Status);
        Assert.Equal(push.Status, await DbStatusAsync(held.Id()));

        Assert.Equal(HttpStatusCode.OK, (await Webhook(providerRef)).StatusCode); // gateways retry
        Assert.Equal(HttpStatusCode.OK, (await Webhook(providerRef)).StatusCode);
        Assert.Empty(await l.DrainAsync());
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Simulate_payment_pushes_confirmed()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        Assert.Equal(HttpStatusCode.OK, (await user.Http.SimulatePaymentAsync(held.Id())).StatusCode);

        await AssertPush(l, held.Id(), "confirmed");
        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task A_failed_payment_webhook_pushes_nothing()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/webhook/mock")
        {
            Content = JsonContent.Create(new { providerRef = await ProviderRefAsync(held.Id()), status = "failed" }),
        };
        msg.Headers.Add("X-Webhook-Secret", "dev-webhook-secret");

        Assert.Equal(HttpStatusCode.OK, (await api.Anonymous().SendAsync(msg)).StatusCode);

        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task Hold_expiry_pushes_expired()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        api.Clock.Advance(TimeSpan.FromMinutes(11));
        await ExpireAsync();

        var push = await l.NextAsync();
        Assert.Equal((held.Id(), "expired"), (push.BookingId, push.Status));
        Assert.Equal("expired", await DbStatusAsync(held.Id()));
        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task Late_payment_that_rescues_an_expired_hold_pushes_expired_then_confirmed()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        api.Clock.Advance(TimeSpan.FromMinutes(11));
        await ExpireAsync();
        await AssertPush(l, held.Id(), "expired");

        await user.Http.SimulatePaymentAsync(held.Id());

        await AssertPush(l, held.Id(), "confirmed");
        Assert.Equal("confirmed", await DbStatusAsync(held.Id()));
        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task Late_payment_refund_path_pushes_expired_to_the_original_owner_only()
    {
        var alice = await api.LoginAsync();
        var bob = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        await using var aliceL = await ListenAsync(alice);
        await using var bobL = await ListenAsync(bob);
        var aliceHold = await alice.Http.HoldOkAsync(block);
        api.Clock.Advance(TimeSpan.FromMinutes(11));
        await ExpireAsync();
        await AssertPush(aliceL, aliceHold.Id(), "expired");
        var bobHold = await bob.Http.HoldOkAsync(block);
        await bob.Http.SimulatePaymentAsync(bobHold.Id());
        await AssertPush(bobL, bobHold.Id(), "confirmed");

        Assert.Equal(HttpStatusCode.OK, (await alice.Http.SimulatePaymentAsync(aliceHold.Id())).StatusCode);

        await AssertPush(aliceL, aliceHold.Id(), "expired"); // refund path: the booking stays expired, the client re-reads payment state
        Assert.Equal("expired", await DbStatusAsync(aliceHold.Id()));
        Assert.Empty(await aliceL.DrainAsync());
        Assert.Empty(await bobL.DrainAsync());
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Player_cancel_pushes_cancelled()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        await user.Http.SimulatePaymentAsync(held.Id());
        await AssertPush(l, held.Id(), "confirmed");

        Assert.Equal(HttpStatusCode.OK, (await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);

        await AssertPush(l, held.Id(), "cancelled");
        Assert.Equal("cancelled", await DbStatusAsync(held.Id()));
        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task Clubos_check_in_pushes_checked_in_to_the_player_and_not_to_the_staff_member()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var date = InTwoDays;
        var id = await OwnedBookingAsync(v, player, date, "10:00");
        await using var playerL = await ListenAsync(player);
        await using var staffL = await ListenAsync(v.Staff);
        GoTo(date, "10:00", -10);

        Assert.Equal(HttpStatusCode.OK, (await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/bookings/{id}/check-in", null)).StatusCode);

        await AssertPush(playerL, id, "checked_in");
        Assert.Equal("checked_in", await DbStatusAsync(id));
        Assert.Empty(await staffL.DrainAsync());
        Assert.Empty(await playerL.DrainAsync());
    }

    [Fact]
    public async Task Clubos_no_show_pushes_no_show()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var date = InTwoDays;
        var id = await OwnedBookingAsync(v, player, date, "10:00");
        await using var l = await ListenAsync(player);
        GoTo(date, "10:00", 20);

        Assert.Equal(HttpStatusCode.OK, (await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/bookings/{id}/no-show", null)).StatusCode);

        await AssertPush(l, id, "no_show");
        Assert.Equal("no_show", await DbStatusAsync(id));
        Assert.Empty(await l.DrainAsync());
    }

    [Fact]
    public async Task Auto_settle_pushes_no_show_for_a_missed_booking_and_completed_after_check_in()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var date = InTwoDays;
        var missed = await OwnedBookingAsync(v, player, date, "08:00");
        var played = await OwnedBookingAsync(v, player, date, "12:00", v.CourtB);
        await using var l = await ListenAsync(player);

        GoTo(date, "11:50");
        await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/bookings/{played}/check-in", null);
        await AssertPush(l, played, "checked_in");

        GoTo(date, "12:00", 20); // 08:00 booking is 4 h past its start, 12:00 is still running
        await SettleAsync();
        await AssertPush(l, missed, "no_show");
        Assert.Empty(await l.DrainAsync()); // the checked-in one has not finished yet

        GoTo(date, "13:00", 1);
        await SettleAsync();
        await AssertPush(l, played, "completed");
        Assert.Equal("completed", await DbStatusAsync(played));
        Assert.Equal("no_show", await DbStatusAsync(missed));
        Assert.Empty(await l.DrainAsync());
    }

    // ------------------------------------------------------------------ who must NOT hear it

    [Fact]
    public async Task Another_player_and_admin_receive_nothing_for_someone_elses_booking()
    {
        var owner = await api.LoginAsync();
        var other = await api.LoginAsync();
        await using var ownerL = await ListenAsync(owner);
        await using var otherL = await ListenAsync(other);
        await using var adminL = await Listener.StartAsync(api, await AdminTokenAsync());
        var held = await owner.Http.HoldOkAsync(SlotPicker.Next(api));

        await owner.Http.SimulatePaymentAsync(held.Id());
        await AssertPush(ownerL, held.Id(), "confirmed");
        await owner.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);
        await AssertPush(ownerL, held.Id(), "cancelled");

        Assert.Empty(await otherL.DrainAsync());
        Assert.Empty(await adminL.DrainAsync());
    }

    [Fact]
    public async Task Two_connections_of_the_same_user_both_receive()
    {
        var user = await api.LoginAsync();
        await using var phone = await ListenAsync(user);
        await using var laptop = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        await user.Http.SimulatePaymentAsync(held.Id());

        await AssertPush(phone, held.Id(), "confirmed");
        await AssertPush(laptop, held.Id(), "confirmed");
        Assert.Empty(await phone.DrainAsync());
        Assert.Empty(await laptop.DrainAsync());
    }

    [Fact]
    public async Task A_disconnected_client_stops_receiving_and_the_server_does_not_break()
    {
        var user = await api.LoginAsync();
        var stays = await ListenAsync(user);
        var goes = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        await goes.Conn.StopAsync();
        await goes.DisposeAsync();

        Assert.Equal(HttpStatusCode.OK, (await user.Http.SimulatePaymentAsync(held.Id())).StatusCode);

        await AssertPush(stays, held.Id(), "confirmed");
        Assert.Equal(HubConnectionState.Disconnected, goes.Conn.State);
        Assert.Empty(await goes.DrainAsync());
        await stays.DisposeAsync();

        // nobody connected any more: a state change must still succeed
        Assert.Equal(HttpStatusCode.OK, (await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);
        AssertNoServerErrors();
    }

    [Fact]
    public async Task Walk_in_creation_and_settlement_push_nothing_to_anyone()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var player = await api.LoginAsync();
        var date = InTwoDays;
        await using var playerL = await ListenAsync(player);
        await using var staffL = await ListenAsync(v.Staff);

        var res = await v.Staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{v.Id}/walk-ins", new
        {
            date = date.ToString("yyyy-MM-dd"),
            slots = new[] { new { courtId = v.CourtA, start = "10:00" } },
            customerName = "Walk Wendy",
        });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var walkIn = (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        GoTo(date, "11:00", 1);
        await SettleAsync(); // walk-in completes with no user to tell: must not throw
        Assert.Equal("completed", await DbStatusAsync(walkIn));

        Assert.Empty(await playerL.DrainAsync());
        Assert.Empty(await staffL.DrainAsync());
        AssertNoServerErrors();
    }

    // ------------------------------------------------------------------ payload

    [Fact]
    public async Task Payload_is_exactly_bookingId_and_status_with_no_payment_details()
    {
        var user = await api.LoginAsync();
        await using var l = await ListenAsync(user);
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        await user.Http.SimulatePaymentAsync(held.Id());

        var push = await l.NextAsync();
        Assert.Equal(new[] { "bookingId", "status" }, push.Properties);
        Assert.DoesNotContain(push.Properties, p => p.Contains("qr", StringComparison.OrdinalIgnoreCase)
            || p.Contains("payment", StringComparison.OrdinalIgnoreCase) || p.Contains("amount", StringComparison.OrdinalIgnoreCase));
    }
}
