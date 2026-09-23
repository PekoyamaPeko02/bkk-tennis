using BkkTennis.Api.Features.Bookings;

namespace BkkTennis.Api.Jobs;

/// <summary>Releases unpaid holds every 30 seconds. Swap for Hangfire when more jobs arrive (payouts, reminders).</summary>
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
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Hold expiry run failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
