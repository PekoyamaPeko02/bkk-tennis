using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.Api.Data;
using BkkTennis.Api.Features.Bookings;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

[Collection(ApiCollection.Name)]
public class BookingFlowTests(ApiFactory api)
{
    // ------------------------------------------------------------------ hold

    [Fact]
    public async Task Hold_returns_201_with_server_computed_price_qr_and_ten_minute_expiry()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);

        var booking = await user.Http.HoldOkAsync(block, slots: 2);

        Assert.Equal("held", booking.Status());
        Assert.Equal(2 * Seed.StandardHourly, booking.GetProperty("total").GetDecimal());
        Assert.False(string.IsNullOrWhiteSpace(booking.GetProperty("code").GetString()));
        Assert.Equal(2, booking.GetProperty("slots").GetArrayLength());

        var payment = booking.GetProperty("payment");
        Assert.Equal("pending", payment.GetProperty("status").GetString());
        Assert.Equal(2 * Seed.StandardHourly, payment.GetProperty("amount").GetDecimal());
        Assert.StartsWith("000201", payment.GetProperty("qrPayload").GetString()); // EMVCo PromptPay payload

        var expires = booking.GetProperty("holdExpiresAt").GetDateTime();
        Assert.InRange((expires - api.Clock.GetUtcNow().UtcDateTime).TotalMinutes, 9.9, 10.1);
    }

    [Fact]
    public async Task Hold_adds_addons_priced_per_booking_and_per_hour()
    {
        var user = await api.LoginAsync();
        var venue = await user.Http.GetFromJsonAsync<JsonElement>($"/api/v1/venues/{Seed.VenueSlug}");
        var addons = venue.GetProperty("addons").EnumerateArray().ToList();
        var racket = addons.Single(a => a.GetProperty("unit").GetString() == "per_booking" && a.GetProperty("price").GetDecimal() == 100m);
        var knocker = addons.Single(a => a.GetProperty("unit").GetString() == "per_hour");

        var res = await user.Http.HoldAsync(SlotPicker.Next(api), slots: 2, addons:
            [(racket.GetProperty("id").GetGuid(), 1), (knocker.GetProperty("id").GetGuid(), 1)]);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var booking = await res.Content.ReadFromJsonAsync<JsonElement>();
        var expectedAddons = 100m + knocker.GetProperty("price").GetDecimal() * 2; // per_hour × 2 hours
        Assert.Equal(expectedAddons, booking.GetProperty("addonTotal").GetDecimal());
        Assert.Equal(2 * Seed.StandardHourly + expectedAddons, booking.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Hold_requires_authentication()
    {
        var res = await api.Anonymous().HoldAsync(SlotPicker.Next(api));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Overlapping_hold_by_another_user_is_409_slot_taken()
    {
        var alice = await api.LoginAsync();
        var bob = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        await alice.Http.HoldOkAsync(block);

        var res = await bob.Http.HoldAsync(block);

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("slot_taken", (await res.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());
    }

    [Fact]
    public async Task Second_open_hold_by_the_same_user_is_409_open_hold_exists()
    {
        var user = await api.LoginAsync();
        await user.Http.HoldOkAsync(SlotPicker.Next(api));

        var res = await user.Http.HoldAsync(SlotPicker.Next(api));

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("open_hold_exists", (await res.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());
    }

    [Fact]
    public async Task Hold_validates_slots_and_hours()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);

        var tooEarly = await user.Http.HoldAsync(block with { First = "05:00" });
        Assert.Equal(HttpStatusCode.BadRequest, tooEarly.StatusCode);
        Assert.Equal("outside_hours", (await tooEarly.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());

        var offGrid = await user.Http.HoldAsync(block with { First = "10:30" });
        Assert.Equal("outside_hours", (await offGrid.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());

        var past = await user.Http.HoldAsync(block with { Date = block.Date.AddDays(-30) });
        Assert.Equal("date_out_of_range", (await past.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());

        var far = await user.Http.HoldAsync(block with { Date = block.Date.AddDays(30) });
        Assert.Equal("date_out_of_range", (await far.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());
    }

    [Fact]
    public async Task Same_idempotency_key_returns_the_same_booking_without_a_second_hold()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var key = Guid.NewGuid().ToString();

        var first = await user.Http.HoldAsync(block, idempotencyKey: key);
        var second = await user.Http.HoldAsync(block, idempotencyKey: key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var a = await first.Content.ReadFromJsonAsync<JsonElement>();
        var b = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(a.Id(), b.Id());
        Assert.Equal(a.GetProperty("code").GetString(), b.GetProperty("code").GetString());

        await api.WithDb(async db =>
            Assert.Equal(1, await db.Bookings.CountAsync(x => x.UserId == user.Id && x.IdempotencyKey == key)));
    }

    // ------------------------------------------------------------------ expiry

    [Fact]
    public async Task Expired_hold_frees_the_slot_for_someone_else()
    {
        var alice = await api.LoginAsync();
        var bob = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var held = await alice.Http.HoldOkAsync(block);

        // Still inside the hold window: the expiry job must leave it alone.
        await ExpireAsync();
        Assert.Equal("held", (await alice.Http.GetBookingAsync(held.Id())).Status());
        Assert.Equal(HttpStatusCode.Conflict, (await bob.Http.HoldAsync(block)).StatusCode);

        api.Clock.Advance(TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(1));
        Assert.True(await ExpireAsync() >= 1);

        Assert.Equal("expired", (await alice.Http.GetBookingAsync(held.Id())).Status());
        Assert.Equal(HttpStatusCode.Created, (await bob.Http.HoldAsync(block)).StatusCode);
    }

    [Fact]
    public async Task Expired_hold_lets_the_same_user_hold_again()
    {
        var user = await api.LoginAsync();
        await user.Http.HoldOkAsync(SlotPicker.Next(api));

        api.Clock.Advance(TimeSpan.FromMinutes(11)); // hold lapsed: no longer an "open hold" even before the job runs

        Assert.Equal(HttpStatusCode.Created, (await user.Http.HoldAsync(SlotPicker.Next(api))).StatusCode);
    }

    // ------------------------------------------------------------------ payment

    [Fact]
    public async Task Simulated_payment_confirms_the_booking_and_hides_the_qr()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        var paid = await user.Http.SimulatePaymentAsync(held.Id());
        Assert.Equal(HttpStatusCode.OK, paid.StatusCode);

        var booking = await user.Http.GetBookingAsync(held.Id());
        Assert.Equal("confirmed", booking.Status());
        Assert.Equal(JsonValueKind.Null, booking.GetProperty("holdExpiresAt").ValueKind);
        var payment = booking.GetProperty("payment");
        Assert.Equal("succeeded", payment.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, payment.GetProperty("qrPayload").ValueKind);

        var list = await user.Http.GetFromJsonAsync<JsonElement>("/api/v1/me/bookings/");
        Assert.Contains(list.EnumerateArray(), b => b.Id() == held.Id() && b.Status() == "confirmed");

        await api.WithDb(async db =>
        {
            var b = await db.Bookings.Include(x => x.Slots).SingleAsync(x => x.Id == held.Id());
            Assert.All(b.Slots, s => Assert.Equal("confirmed", s.Status));
            Assert.Equal(Math.Round(b.Total * 0.10m, 2), b.Commission);
        });
    }

    [Fact]
    public async Task Webhook_with_a_bad_secret_is_rejected_and_the_booking_stays_held()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        var providerRef = await ProviderRefAsync(held.Id());

        var res = await Webhook(providerRef, secret: "wrong");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("held", (await user.Http.GetBookingAsync(held.Id())).Status());
    }

    [Fact]
    public async Task Webhook_with_the_right_secret_confirms_and_is_idempotent()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        var providerRef = await ProviderRefAsync(held.Id());

        Assert.Equal(HttpStatusCode.OK, (await Webhook(providerRef)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Webhook(providerRef)).StatusCode); // gateways retry

        Assert.Equal("confirmed", (await user.Http.GetBookingAsync(held.Id())).Status());
    }

    [Fact]
    public async Task Failed_payment_webhook_does_not_confirm()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));

        await Webhook(await ProviderRefAsync(held.Id()), status: "failed");

        var booking = await user.Http.GetBookingAsync(held.Id());
        Assert.Equal("held", booking.Status());
        Assert.Equal("failed", booking.GetProperty("payment").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Late_payment_after_expiry_reconfirms_when_the_slot_is_still_free()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        api.Clock.Advance(TimeSpan.FromMinutes(11));
        await ExpireAsync();
        Assert.Equal("expired", (await user.Http.GetBookingAsync(held.Id())).Status());

        Assert.Equal(HttpStatusCode.OK, (await user.Http.SimulatePaymentAsync(held.Id())).StatusCode);

        Assert.Equal("confirmed", (await user.Http.GetBookingAsync(held.Id())).Status());
        await api.WithDb(async db =>
        {
            var b = await db.Bookings.Include(x => x.Slots).Include(x => x.Payments).SingleAsync(x => x.Id == held.Id());
            Assert.All(b.Slots, s => Assert.Equal("confirmed", s.Status));
            Assert.Equal("succeeded", Assert.Single(b.Payments).Status);
            Assert.False(await db.Refunds.AnyAsync(r => r.PaymentId == b.Payments[0].Id));
        });
    }

    [Fact]
    public async Task Late_payment_after_expiry_is_refunded_when_someone_else_took_the_slot()
    {
        var alice = await api.LoginAsync();
        var bob = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var aliceHold = await alice.Http.HoldOkAsync(block);
        api.Clock.Advance(TimeSpan.FromMinutes(11));
        await ExpireAsync();
        var bobHold = await bob.Http.HoldOkAsync(block);
        await bob.Http.SimulatePaymentAsync(bobHold.Id());

        Assert.Equal(HttpStatusCode.OK, (await alice.Http.SimulatePaymentAsync(aliceHold.Id())).StatusCode);

        Assert.Equal("expired", (await alice.Http.GetBookingAsync(aliceHold.Id())).Status());
        Assert.Equal("confirmed", (await bob.Http.GetBookingAsync(bobHold.Id())).Status());
        await api.WithDb(async db =>
        {
            var payment = await db.Payments.SingleAsync(p => p.BookingId == aliceHold.Id());
            Assert.Equal("refunded", payment.Status);
            var refund = await db.Refunds.SingleAsync(r => r.PaymentId == payment.Id);
            Assert.Equal(payment.Amount, refund.Amount);
            Assert.Equal("slot_taken_after_hold_expired", refund.Reason);
        });
    }

    // ------------------------------------------------------------------ cancel

    [Fact]
    public async Task Cancel_at_least_four_hours_before_start_refunds_everything()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api); // ≥ 3 days ahead
        var held = await user.Http.HoldOkAsync(block, slots: 2);
        await user.Http.SimulatePaymentAsync(held.Id());

        var res = await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2 * Seed.StandardHourly, body.GetProperty("refund").GetDecimal());
        Assert.Equal("cancelled", body.GetProperty("booking").Status());
        await api.WithDb(async db =>
        {
            var refund = await db.Refunds.SingleAsync(r => db.Payments.Any(p => p.Id == r.PaymentId && p.BookingId == held.Id()));
            Assert.Equal(2 * Seed.StandardHourly, refund.Amount);
            Assert.Equal("user_cancel", refund.Reason);
        });
    }

    [Fact]
    public async Task Cancel_less_than_four_hours_before_start_refunds_nothing()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var held = await user.Http.HoldOkAsync(block);
        await user.Http.SimulatePaymentAsync(held.Id());

        api.Clock.SetUtcNow(new DateTimeOffset(block.FirstStartUtc.AddHours(-3), TimeSpan.Zero)); // 3 h before start (< 4 h free window)
        var res = await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0m, body.GetProperty("refund").GetDecimal());
        Assert.Equal("cancelled", body.GetProperty("booking").Status());
        await api.WithDb(async db =>
        {
            Assert.False(await db.Refunds.AnyAsync(r => db.Payments.Any(p => p.Id == r.PaymentId && p.BookingId == held.Id())));
            Assert.Equal("succeeded", (await db.Payments.SingleAsync(p => p.BookingId == held.Id())).Status);
        });
    }

    [Fact]
    public async Task Cancel_exactly_at_the_four_hour_boundary_is_still_free()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var held = await user.Http.HoldOkAsync(block);
        await user.Http.SimulatePaymentAsync(held.Id());

        api.Clock.SetUtcNow(new DateTimeOffset(block.FirstStartUtc.AddHours(-4), TimeSpan.Zero));
        var body = await (await user.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(Seed.StandardHourly, body.GetProperty("refund").GetDecimal());
    }

    [Fact]
    public async Task Cancelling_an_unpaid_hold_releases_the_slot_without_a_refund()
    {
        var alice = await api.LoginAsync();
        var bob = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var held = await alice.Http.HoldOkAsync(block);

        var res = await alice.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);

        Assert.Equal(0m, (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("refund").GetDecimal());
        Assert.Equal(HttpStatusCode.Created, (await bob.Http.HoldAsync(block)).StatusCode);
    }

    [Fact]
    public async Task Cancelling_twice_is_409_and_someone_elses_booking_is_404()
    {
        var alice = await api.LoginAsync();
        var mallory = await api.LoginAsync();
        var held = await alice.Http.HoldOkAsync(SlotPicker.Next(api));

        Assert.Equal(HttpStatusCode.NotFound, (await mallory.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await mallory.Http.GetAsync($"/api/v1/me/bookings/{held.Id()}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await alice.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);
        var again = await alice.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("not_cancellable", (await again.Content.ReadFromJsonAsync<JsonElement>()).ErrorCode());
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Runs the hold-expiry pass (the background job is disabled in tests). Returns how many holds expired.</summary>
    private async Task<int> ExpireAsync() =>
        await api.WithScope(sp => sp.GetRequiredService<BookingService>().ExpireHoldsAsync(CancellationToken.None));

    private Task<string> ProviderRefAsync(Guid bookingId) =>
        api.WithScope(async sp =>
            (await sp.GetRequiredService<AppDbContext>().Payments.AsNoTracking().SingleAsync(p => p.BookingId == bookingId)).ProviderRef);

    private Task<HttpResponseMessage> Webhook(string providerRef, string status = "succeeded", string secret = "dev-webhook-secret")
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/webhook/mock")
        {
            Content = JsonContent.Create(new { providerRef, status }),
        };
        msg.Headers.Add("X-Webhook-Secret", secret);
        return api.Anonymous().SendAsync(msg);
    }
}
