using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using BkkTennis.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace BkkTennis.IntegrationTests;

/// <summary>
/// The portal refreshes its grid when SignalR says `availabilityChanged` for the venue/day it watches: walk-ins, blocks,
/// unblocking, check-in, no-show and player bookings must all publish it — and only to the right venue and day.
/// </summary>
[Collection(ApiCollection.Name)]
public class RealtimeTests(ApiFactory api)
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(5);

    private DateOnly Future => DateOnly.FromDateTime(api.Clock.GetUtcNow().UtcDateTime).AddDays(6);

    /// <summary>A watcher on /hubs/availability (long polling: the in-memory TestServer has no WebSocket client).</summary>
    private sealed class Watcher : IAsyncDisposable
    {
        private readonly HubConnection _conn;
        private readonly Channel<(Guid VenueId, string Date)> _events = Channel.CreateUnbounded<(Guid, string)>();

        private Watcher(HubConnection conn) => _conn = conn;

        public static async Task<Watcher> StartAsync(ApiFactory api, Guid venueId, params DateOnly[] dates)
        {
            var conn = new HubConnectionBuilder()
                .WithUrl(new Uri(api.Server.BaseAddress, "/hubs/availability"), o =>
                {
                    o.Transports = HttpTransportType.LongPolling;
                    o.HttpMessageHandlerFactory = _ => api.Server.CreateHandler();
                })
                .Build();
            var w = new Watcher(conn);
            conn.On<JsonElement>("availabilityChanged", m =>
                w._events.Writer.TryWrite((m.GetProperty("venueId").GetGuid(), m.GetProperty("date").GetString()!)));
            await conn.StartAsync();
            foreach (var d in dates) await conn.InvokeAsync("Watch", venueId, d.ToString("yyyy-MM-dd"));
            return w;
        }

        /// <summary>The next notification, or a failure if none arrives in time.</summary>
        public async Task<(Guid VenueId, string Date)> NextAsync()
        {
            using var cts = new CancellationTokenSource(Wait);
            try { return await _events.Reader.ReadAsync(cts.Token); }
            catch (OperationCanceledException) { throw new Xunit.Sdk.XunitException("no availabilityChanged arrived within " + Wait); }
        }

        /// <summary>All notifications received during a short quiet period (used to prove nothing else was sent).</summary>
        public async Task<List<(Guid VenueId, string Date)>> DrainAsync(TimeSpan? quiet = null)
        {
            await Task.Delay(quiet ?? TimeSpan.FromMilliseconds(400));
            var all = new List<(Guid, string)>();
            while (_events.Reader.TryRead(out var e)) all.Add(e);
            return all;
        }

        public ValueTask DisposeAsync() => _conn.DisposeAsync();
    }

    private static object WalkIn(ScheduleVenue v, DateOnly date, string start = "10:00") => new
    {
        date = date.ToString("yyyy-MM-dd"),
        slots = new[] { new { courtId = v.CourtA, start } },
        customerName = "Walk Wendy",
    };

    private string BlockBody(ScheduleVenue v, DateTime startUtc, DateTime endUtc) => JsonSerializer.Serialize(new
    {
        courtId = v.CourtB,
        startAt = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
        endAt = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
        reason = "maintenance",
    });

    private static StringContent Json(string raw) => new(raw, System.Text.Encoding.UTF8, "application/json");

    // ------------------------------------------------------------------ what publishes

    [Fact]
    public async Task A_walk_in_notifies_watchers_of_that_venue_and_day()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await using var watcher = await Watcher.StartAsync(api, v.Id, Future);

        var res = await v.Staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{v.Id}/walk-ins", WalkIn(v, Future));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        Assert.Equal((v.Id, Future.ToString("yyyy-MM-dd")), await watcher.NextAsync());
    }

    [Fact]
    public async Task Creating_and_removing_a_block_each_notify()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await using var watcher = await Watcher.StartAsync(api, v.Id, Future);
        var day = Future.ToString("yyyy-MM-dd");

        var created = await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/blocks",
            Json(BlockBody(v, ScheduleVenue.Utc(Future, "10:00"), ScheduleVenue.Utc(Future, "12:00"))));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal((v.Id, day), await watcher.NextAsync());

        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NoContent, (await v.Staff.Http.DeleteAsync($"/api/v1/clubos/venues/{v.Id}/blocks/{id}")).StatusCode);
        Assert.Equal((v.Id, day), await watcher.NextAsync());
    }

    [Fact]
    public async Task A_block_that_spans_midnight_notifies_every_day_it_touches()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var next = Future.AddDays(1);
        await using var watcher = await Watcher.StartAsync(api, v.Id, Future, next);

        var res = await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/blocks",
            Json(BlockBody(v, ScheduleVenue.Utc(Future, "20:00"), ScheduleVenue.Utc(next, "09:00"))));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var days = new[] { (await watcher.NextAsync()).Date, (await watcher.NextAsync()).Date }.Order().ToArray();
        Assert.Equal(new[] { Future.ToString("yyyy-MM-dd"), next.ToString("yyyy-MM-dd") }, days);
    }

    [Fact]
    public async Task Check_in_and_no_show_notify()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var date = DateOnly.FromDateTime(api.Clock.GetUtcNow().UtcDateTime).AddDays(2);
        var checkIn = await v.AddBookingAsync(v.CourtA, date, ["10:00"]);
        var noShow = await v.AddBookingAsync(v.CourtA, date, ["12:00"]);
        await using var watcher = await Watcher.StartAsync(api, v.Id, date);

        // jump the clock into each booking's window (forward only: check-in opens 30 min before, no-show 15 min after the start)
        api.Clock.SetUtcNow(new DateTimeOffset(ScheduleVenue.Utc(date, "10:00").AddMinutes(-10), TimeSpan.Zero));
        Assert.Equal(HttpStatusCode.OK, (await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/bookings/{checkIn}/check-in", null)).StatusCode);
        Assert.Equal((v.Id, date.ToString("yyyy-MM-dd")), await watcher.NextAsync());

        api.Clock.SetUtcNow(new DateTimeOffset(ScheduleVenue.Utc(date, "12:00").AddMinutes(20), TimeSpan.Zero));
        Assert.Equal(HttpStatusCode.OK, (await v.Staff.Http.PostAsync($"/api/v1/clubos/venues/{v.Id}/bookings/{noShow}/no-show", null)).StatusCode);
        Assert.Equal((v.Id, date.ToString("yyyy-MM-dd")), await watcher.NextAsync());
    }

    [Fact]
    public async Task A_player_hold_and_its_cancellation_notify_the_portal()
    {
        var player = await api.LoginAsync();
        var block = SlotPicker.Next(api);
        await using var watcher = await Watcher.StartAsync(api, Seed.VenueId, block.Date);

        var held = await player.Http.HoldOkAsync(block);
        Assert.Equal((Seed.VenueId, block.Date.ToString("yyyy-MM-dd")), await watcher.NextAsync());

        Assert.Equal(HttpStatusCode.OK, (await player.Http.PostAsync($"/api/v1/me/bookings/{held.Id()}/cancel", null)).StatusCode);
        Assert.Equal((Seed.VenueId, block.Date.ToString("yyyy-MM-dd")), await watcher.NextAsync());
    }

    // ------------------------------------------------------------------ what must NOT be delivered

    [Fact]
    public async Task Watchers_of_another_day_or_another_venue_hear_nothing()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var other = await ScheduleVenue.CreateAsync(api);
        await using var otherDay = await Watcher.StartAsync(api, v.Id, Future.AddDays(3));
        await using var otherVenue = await Watcher.StartAsync(api, other.Id, Future);
        await using var right = await Watcher.StartAsync(api, v.Id, Future);

        Assert.Equal(HttpStatusCode.Created, (await v.Staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{v.Id}/walk-ins", WalkIn(v, Future))).StatusCode);

        await right.NextAsync();
        Assert.Empty(await otherDay.DrainAsync());
        Assert.Empty(await otherVenue.DrainAsync());
    }

    [Fact]
    public async Task A_failed_walk_in_publishes_nothing()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        await v.AddBookingAsync(v.CourtA, Future, ["10:00"]);
        await using var watcher = await Watcher.StartAsync(api, v.Id, Future);

        var res = await v.Staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{v.Id}/walk-ins", WalkIn(v, Future)); // slot already taken

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Empty(await watcher.DrainAsync());
    }

    [Fact]
    public async Task Unwatching_stops_the_notifications()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var conn = new HubConnectionBuilder()
            .WithUrl(new Uri(api.Server.BaseAddress, "/hubs/availability"), o =>
            {
                o.Transports = HttpTransportType.LongPolling;
                o.HttpMessageHandlerFactory = _ => api.Server.CreateHandler();
            }).Build();
        var got = 0;
        conn.On<JsonElement>("availabilityChanged", _ => Interlocked.Increment(ref got));
        await conn.StartAsync();
        var day = Future.ToString("yyyy-MM-dd");
        await conn.InvokeAsync("Watch", v.Id, day);
        await conn.InvokeAsync("Unwatch", v.Id, day);

        await v.Staff.Http.PostAsJsonAsync($"/api/v1/clubos/venues/{v.Id}/walk-ins", WalkIn(v, Future));
        await Task.Delay(500);
        await conn.DisposeAsync();

        Assert.Equal(0, got);
    }

    [Fact]
    public async Task Watching_a_malformed_date_is_ignored_not_an_error()
    {
        var v = await ScheduleVenue.CreateAsync(api);
        var conn = new HubConnectionBuilder()
            .WithUrl(new Uri(api.Server.BaseAddress, "/hubs/availability"), o =>
            {
                o.Transports = HttpTransportType.LongPolling;
                o.HttpMessageHandlerFactory = _ => api.Server.CreateHandler();
            }).Build();
        await conn.StartAsync();

        await conn.InvokeAsync("Watch", v.Id, "not-a-date"); // must not throw / drop the connection

        Assert.Equal(HubConnectionState.Connected, conn.State);
        await conn.DisposeAsync();
    }
}
