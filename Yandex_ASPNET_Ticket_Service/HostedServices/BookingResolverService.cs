using Microsoft.Extensions.Logging;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;
using Yandex_ASPNET_Ticket_Service.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Yandex_ASPNET_Ticket_Service.HostedServices;

/// <summary>
/// Background service that periodically resolves pending bookings by confirming or rejecting them
/// </summary>
public class BookingResolverService(
    IBookingStorage bookingStorage,
    IEventService eventService,
    ILogger<BookingResolverService> logger) : BackgroundService
{
    private readonly IBookingStorage _bookingStorage = bookingStorage;
    private readonly IEventService _eventService = eventService;
    private readonly ILogger<BookingResolverService> _logger = logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
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
            var pendingBookings = await _bookingStorage.GetByStatusAsync(BookingStatus.Pending);
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
            await Task.Delay(_pollingInterval, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a single booking asynchronously
    /// </summary>
    /// <param name="booking">Booking to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
    {
        try
        {
            // Simulate external API call before acquiring semaphore (parallel delay)
            await Task.Delay(_processingDelay, cancellationToken);

            // Acquire semaphore to protect storage updates
            await _processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Check if event still exists
                var eventEntity = _eventService.GetEvent(booking.EventId);
                if (eventEntity == null)
                {
                    booking.Reject();
                    _logger.LogWarning("Event {EventId} not found, booking {BookingId} rejected", booking.EventId, booking.Id);
                    await _bookingStorage.UpdateAsync(booking);
                    return;
                }

                // Confirm booking
                booking.Confirm();
                await _bookingStorage.UpdateAsync(booking);
                _logger.LogInformation("Booking {BookingId} confirmed", booking.Id);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing of booking {BookingId} was cancelled", booking.Id);
            // Re-throw to allow proper cancellation
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing booking {BookingId}", booking.Id);
            // Reject booking and release seats if possible
            await RejectBookingWithSeatRelease(booking, ex);
        }
    }

    /// <summary>
    /// Rejects a booking and releases seats if event exists
    /// </summary>
    /// <param name="booking">Booking to reject</param>
    /// <param name="exception">Exception that caused rejection</param>
    private async Task RejectBookingWithSeatRelease(Booking booking, Exception? exception = null)
    {
        // Note: This method is called from catch block where semaphore may already be released.
        // We need to acquire semaphore again to protect concurrent updates.
        try
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                var eventEntity = _eventService.GetEvent(booking.EventId);
                if (eventEntity != null)
                {
                    //It is not known how many seats were booked,
                    //so we are releasing only one.
                    //It may be clarified in the next sprint.
                    //Or maybe not - he he he)
                    eventEntity.ReleaseSeats(1);
                    _eventService.UpdateEvent(booking.EventId, eventEntity);
                }
                booking.Reject();
                await _bookingStorage.UpdateAsync(booking);
                _logger.LogWarning(exception, "Booking {BookingId} rejected due to error", booking.Id);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
        catch (Exception innerEx)
        {
            _logger.LogError(innerEx, "Failed to reject booking {BookingId} and release seats", booking.Id);
        }
    }
}