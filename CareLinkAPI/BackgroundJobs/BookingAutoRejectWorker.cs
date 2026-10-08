using CareLinkAPI.Common.Options;
using CareLinkAPI.Services;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.BackgroundJobs;

/// <summary>
/// Book-07: periodically auto-rejects PendingAcceptance bookings that the nurse did not accept in time.
/// The worker holds no business logic; it only triggers <see cref="IBookingService"/> in a fresh scope.
/// </summary>
public class BookingAutoRejectWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<BookingOptions> options,
    ILogger<BookingAutoRejectWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, options.Value.AutoRejectScanIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        try
        {
            do
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.AutoRejectExpiredAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Booking auto-reject scan failed; will retry on the next tick.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Application is shutting down.
        }
    }
}
