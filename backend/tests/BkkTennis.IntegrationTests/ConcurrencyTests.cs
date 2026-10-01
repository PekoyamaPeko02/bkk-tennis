using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// The no-double-booking guarantee lives in the database (EXCLUDE constraint on booking_slots), so these tests fire
/// real parallel requests at the API and check the outcome from both sides: HTTP status codes and rows in the DB.
/// </summary>
[Collection(ApiCollection.Name)]
public class ConcurrencyTests(ApiFactory api)
{
    private const int Contenders = 20;

    private async Task<List<TestUser>> LoginManyAsync(int n)
    {
        var users = new List<TestUser>();
        for (var i = 0; i < n; i++) users.Add(await api.LoginAsync());
        return users;
    }

    /// <summary>Releases all requests at (almost) the same instant.</summary>
    private static async Task<T[]> RaceAsync<T>(IEnumerable<Func<Task<T>>> requests)
    {
        var gate = new TaskCompletionSource();
        var tasks = requests.Select(async r =>
        {
            await gate.Task;
            return await r();
        }).ToArray();
        await Task.Delay(50); // let every task park on the gate
        gate.SetResult();
        return await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task Parallel_holds_for_the_same_slot_produce_exactly_one_booking()
    {
        var users = await LoginManyAsync(Contenders);
        var block = SlotPicker.Next(api);

        var responses = await RaceAsync(users.Select(u => (Func<Task<HttpResponseMessage>>)(() => u.Http.HoldAsync(block))));

        var statuses = responses.Select(r => r.StatusCode).ToList();
        var bodies = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));
        Assert.True(statuses.Count(s => s == HttpStatusCode.Created) == 1, $"expected exactly one 201, got: {string.Join(", ", statuses)}\n{string.Join("\n", bodies)}");
        Assert.All(responses.Zip(bodies).Where(x => x.First.StatusCode != HttpStatusCode.Created),
            x =>
            {
                Assert.Equal(HttpStatusCode.Conflict, x.First.StatusCode); // never a 500 / deadlock
                Assert.Equal("slot_taken", JsonDocument.Parse(x.Second).RootElement.ErrorCode());
            });

        // The database agrees: one live booking for this court/hour, and the losers left nothing behind.
        await api.WithDb(async db =>
        {
            var start = block.FirstStartUtc;
            var live = await db.BookingSlots.Where(s => s.CourtId == block.CourtId && s.StartAt == start
                                                        && (s.Status == "held" || s.Status == "confirmed")).CountAsync();
            Assert.Equal(1, live);
            var userIds = users.Select(u => u.Id).ToList();
            Assert.Equal(1, await db.Bookings.CountAsync(b => b.UserId != null && userIds.Contains(b.UserId.Value)));
            Assert.Equal(1, await db.Payments.CountAsync(p => db.Bookings.Any(b => b.Id == p.BookingId && userIds.Contains(b.UserId!.Value))));
        });
    }

    [Fact]
    public async Task Parallel_holds_for_overlapping_slot_sets_let_exactly_one_side_win()
    {
        // A wants 1st+2nd hour, B wants only the 2nd: they overlap on the 2nd hour whichever request lands first.
        // Repeated on fresh slots: two concurrent conflicting inserts under an EXCLUDE constraint can deadlock (40P01) and
        // Postgres aborts one of them. That used to surface as a 500 (roughly 1 race in 300); it must be a plain 409.
        // Each round takes one slot block (≈275 per run available), so keep this modest.
        for (var round = 0; round < 40; round++)
        {
            var a = await api.LoginAsync();
            var b = await api.LoginAsync();
            var block = SlotPicker.Next(api);

            var responses = await RaceAsync<HttpResponseMessage>(
            [
                () => a.Http.HoldAsync(block, slots: 2),
                () => b.Http.HoldAsync(block with { First = block.Second }, slots: 1),
            ]);

            var codes = responses.Select(r => r.StatusCode).OrderBy(c => c).ToArray();
            var bodies = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));
            Assert.True(codes.SequenceEqual(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }),
                $"round {round}: expected [Created, Conflict], got {string.Join(", ", codes)}\n{string.Join("\n", bodies)}\n{string.Join("\n", api.ServerErrors)}");
            await api.WithDb(async db =>
            {
                var second = block.FirstStartUtc.AddHours(1);
                Assert.Equal(1, await db.BookingSlots.CountAsync(s => s.CourtId == block.CourtId && s.StartAt == second
                                                                      && (s.Status == "held" || s.Status == "confirmed")));
            });
        }
    }

    [Fact]
    public async Task Different_slots_do_not_block_each_other()
    {
        var users = await LoginManyAsync(8);
        var blocks = users.Select(_ => SlotPicker.Next(api)).ToList();

        var responses = await RaceAsync(users.Zip(blocks).Select(x => (Func<Task<HttpResponseMessage>>)(() => x.First.Http.HoldAsync(x.Second))));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
    }

    [Fact]
    public async Task Parallel_requests_with_the_same_idempotency_key_create_one_booking()
    {
        var user = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        var key = Guid.NewGuid().ToString();

        var responses = await RaceAsync(Enumerable.Range(0, 6).Select(_ => (Func<Task<HttpResponseMessage>>)(() => user.Http.HoldAsync(block, idempotencyKey: key))));

        // A retry must get the original booking back, never a 409/500 (regression: the loser of the race used to 500 on the unique key).
        var failures = responses.Where(r => r.StatusCode != HttpStatusCode.Created).ToList();
        Assert.True(failures.Count == 0,
            $"{failures.Count} of {responses.Length} replays were not 201: "
            + string.Join(" | ", await Task.WhenAll(failures.Select(async r => $"{(int)r.StatusCode} {await r.Content.ReadAsStringAsync()}"))));
        var ids = new HashSet<Guid>();
        foreach (var r in responses) ids.Add((await r.Content.ReadFromJsonAsync<JsonElement>()).Id());
        Assert.Single(ids);
        await api.WithDb(async db =>
            Assert.Equal(1, await db.Bookings.CountAsync(x => x.UserId == user.Id && x.IdempotencyKey == key)));
    }

    [Fact]
    public async Task Duplicate_payment_webhooks_in_parallel_confirm_once_and_never_fail()
    {
        var user = await api.LoginAsync();
        var held = await user.Http.HoldOkAsync(SlotPicker.Next(api));
        var providerRef = await api.WithScope(async sp =>
            (await sp.GetRequiredService<BkkTennis.Api.Data.AppDbContext>().Payments.AsNoTracking()
                .SingleAsync(p => p.BookingId == held.Id())).ProviderRef);

        var responses = await RaceAsync(Enumerable.Range(0, 5).Select(_ => (Func<Task<HttpResponseMessage>>)(() =>
        {
            var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/webhook/mock")
            {
                Content = JsonContent.Create(new { providerRef, status = "succeeded" }),
            };
            msg.Headers.Add("X-Webhook-Secret", "dev-webhook-secret");
            return api.Anonymous().SendAsync(msg);
        })));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Equal("confirmed", (await user.Http.GetBookingAsync(held.Id())).Status());
        await api.WithDb(async db =>
        {
            Assert.Equal(1, await db.Payments.CountAsync(p => p.BookingId == held.Id() && p.Status == "succeeded"));
            Assert.Equal(0, await db.Refunds.CountAsync(r => db.Payments.Any(p => p.Id == r.PaymentId && p.BookingId == held.Id())));
        });
    }
}
