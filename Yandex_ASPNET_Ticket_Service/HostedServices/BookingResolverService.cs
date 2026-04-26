using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace Yandex_ASPNET_Ticket_Service.HostedServices;

/// <summary>
/// Background service that periodically resolves pending bookings by confirming them
/// </summary>
public class BookingResolverService (IBookingStorage bookingStorage) : BackgroundService
{
    private readonly IBookingStorage _bookingStorage = bookingStorage;

    /// <summary>
    /// Executes the background processing loop
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the service</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var allPendingBookings = await _bookingStorage.GetByStatusAsync(BookingStatus.Pending);

            foreach (var booking in allPendingBookings)
            {
                await Task.Delay(2000, cancellationToken);
                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
                await _bookingStorage.UpdateAsync(booking);
            }
            await Task.Delay(3000, cancellationToken);
        }
    }

}