using Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YaContracts.Enums;

namespace Application.Services.HostedServices;

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
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    pendingBookingIds = await bookingRepository.GetListOfPendingBookings(cancellationToken);
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
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (booking == null || booking.Status != BookingStatus.Pending)
                return;

            var @event = await eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);
            if (@event == null)
            {
                booking.Reject();
                await bookingRepository.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Event {EventId} not found, booking {BookingId} rejected", booking.EventId, booking.Id);
                return;
            }

            booking.Confirm();
            await bookingRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Booking {BookingId} confirmed", booking.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            await RejectBookingOnErrorAsync(bookingId, ex, cancellationToken);
        }
    }

    /// <summary>
    /// Rejects a booking when an error occurs during processing
    /// </summary>
    private async Task RejectBookingOnErrorAsync(Guid bookingId, Exception ex, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (booking == null)
                return;

            booking.Reject();

            var @event = await eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);
            if (@event != null)
                @event.ReleaseSeats();

            await bookingRepository.SaveChangesAsync(cancellationToken);

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