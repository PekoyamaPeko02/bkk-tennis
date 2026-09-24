using BkkTennis.Api.Features.Bookings;

namespace BkkTennis.Api.Jobs;

/// <summary>Every 30 seconds: releases unpaid holds and settles bookings (auto no-show / completed). Swap for Hangfire when more jobs arrive (payouts, reminders).</summary>
public class HoldExpiryJob(IServiceScopeFactory scopes, ILogger<HoldExpiryJob> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var bookings = scope.ServiceProvider.GetRequiredService<BookingService>();
                var n = await bookings.ExpireHoldsAsync(stoppingToken);
                if (n > 0) log.LogInformation("Expired {Count} unpaid holds", n);
                var (noShows, completed) = await bookings.SettleBookingsAsync(stoppingToken);
                if (noShows + completed > 0) log.LogInformation("Settled bookings: {NoShows} no-show, {Completed} completed", noShows, completed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Hold expiry run failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
