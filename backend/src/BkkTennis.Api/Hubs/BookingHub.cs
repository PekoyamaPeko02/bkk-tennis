using BkkTennis.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BkkTennis.Api.Hubs;

/// <summary>
/// Signed-in users connect (token as <c>?access_token=</c> or bearer header) and are put in their own group; the server pushes
/// <c>bookingChanged</c> { bookingId, status } whenever one of their bookings changes state (paid, expired, cancelled, checked in,
/// no-show, completed). The message carries no details — the client re-reads the booking through the normal, authorised API.
/// </summary>
[Authorize]
public class BookingHub : Hub
{
    public static string Group(Guid userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(Context.User!.UserId()));
        await base.OnConnectedAsync();
    }
}

public class BookingNotifier(IHubContext<BookingHub> hub)
{
    public Task ChangedAsync(Guid? userId, Guid bookingId, string status) =>
        userId is { } id
            ? hub.Clients.Group(BookingHub.Group(id)).SendAsync("bookingChanged", new { bookingId, status })
            : Task.CompletedTask; // walk-ins have no user
}
