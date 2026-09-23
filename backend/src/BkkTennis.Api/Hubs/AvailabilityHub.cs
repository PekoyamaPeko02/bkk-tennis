using Microsoft.AspNetCore.SignalR;

namespace BkkTennis.Api.Hubs;

/// <summary>
/// Clients call <c>Watch(venueId, "2026-10-24")</c> and receive <c>availabilityChanged</c>
/// with { venueId, date } whenever a slot on that day is held, confirmed, released or blocked.
/// </summary>
public class AvailabilityHub : Hub
{
    public static string Group(Guid venueId, DateOnly date) => $"venue:{venueId}:{date:yyyy-MM-dd}";

    public Task Watch(Guid venueId, string date) =>
        DateOnly.TryParse(date, out var d)
            ? Groups.AddToGroupAsync(Context.ConnectionId, Group(venueId, d))
            : Task.CompletedTask;

    public Task Unwatch(Guid venueId, string date) =>
        DateOnly.TryParse(date, out var d)
            ? Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(venueId, d))
            : Task.CompletedTask;
}

public class AvailabilityNotifier(IHubContext<AvailabilityHub> hub)
{
    public Task ChangedAsync(Guid venueId, IEnumerable<DateOnly> dates)
    {
        var tasks = dates.Distinct().Select(d =>
            hub.Clients.Group(AvailabilityHub.Group(venueId, d))
                .SendAsync("availabilityChanged", new { venueId, date = d.ToString("yyyy-MM-dd") }));
        return Task.WhenAll(tasks);
    }
}
