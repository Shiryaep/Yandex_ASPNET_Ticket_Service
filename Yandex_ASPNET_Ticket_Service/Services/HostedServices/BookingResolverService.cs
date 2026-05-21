using Microsoft.EntityFrameworkCore;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;

namespace Yandex_ASPNET_Ticket_Service.Services.HostedServices;

/// <summary>
/// Background service that periodically resolves pending bookings by confirming or rejecting them
/// </summary>
public class BookingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<BookingBackgroundService> _logger = logger;
    private readonly int _pollingInterval = 3000;
    private readonly int _processingDelay = 2000;

    /// <summary>
    /// Executes the background processing loop
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the service</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                List<Guid> pendingBookingIds;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    pendingBookingIds = await context.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .Select(b => b.Id)
                        .ToListAsync(cancellationToken);
                }

                var tasks = pendingBookingIds.Select(id =>
                    ProcessBookingAsync(id, cancellationToken));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing pending bookings");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a single booking asynchronously
    /// </summary>
    /// <param name="bookingId">Booking to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_processingDelay, cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            if (booking == null || booking.Status != BookingStatus.Pending)
                return;

            var @event = await context.Events.FirstOrDefaultAsync(e => e.Id == booking.EventId, cancellationToken);
            if (@event == null)
            {
                booking.Reject();
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Event {EventId} not found, booking {BookingId} rejected", booking.EventId, booking.Id);

                return;
            }

            booking.Confirm();
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Booking {BookingId} confirmed", booking.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
                if (booking != null)
                {
                    booking.Reject();

                    var @event = await context.Events.FirstOrDefaultAsync(e => e.Id == booking.EventId, cancellationToken);
                    if (@event != null)
                        @event.ReleaseSeats();

                    await context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogError(ex,
                    "Booking {BookingId} rejected due to processing error",
                    bookingId);
            }
            catch (Exception releaseEx)
            {
                _logger.LogError(releaseEx,
                    "Failed to reject booking {BookingId} after error",
                    bookingId);
            }
        }
    }
}