using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary>
/// Service interface for booking management operations
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Creates a new booking for the specified event
    /// </summary>
    /// <param name="eventId">Identifier of the event to book</param>
    /// <returns>The created booking</returns>
    public Task<Booking> CreateBookingAsync(Guid eventId);

    /// <summary>
    /// Retrieves a booking by its identifier
    /// </summary>
    /// <param name="bookingId">Booking identifier</param>
    /// <returns>The booking if found; otherwise null</returns>
    public Task<Booking?> GetBookingByIdAsync(Guid bookingId);
}