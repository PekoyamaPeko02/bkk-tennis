using BkkTennis.Api.Features.Admin;

namespace BkkTennis.Api.Jobs;

/// <summary>
/// Every 10 minutes: on Fridays (Bangkok) settles the past week's completed bookings into payouts. Idempotent, so running all
/// day is what makes it self-healing after downtime; on other days it does nothing (missed weeks are caught up by the next run
/// because a booking is only ever settled once). Admins can also trigger a run by hand.
/// </summary>
public class PayoutJob(IServiceScopeFactory scopes, ILogger<PayoutJob> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var payouts = scope.ServiceProvider.GetRequiredService<PayoutService>();
                var today = payouts.Today;
                if (PayoutService.IsPayoutDay(today)) await payouts.RunAsync(today, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Payout job failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
