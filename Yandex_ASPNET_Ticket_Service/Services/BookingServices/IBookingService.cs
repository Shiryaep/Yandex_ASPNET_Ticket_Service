using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary> Service Interface for booking manipulation </summary>
public interface IBookingService
{
    /// <summary> Creates new booking on event with eventId </summary>
    public Task<Booking> CreateBookingAsync(Guid eventId);

    /// <summary> Return booking by bookingId </summary>
    public Task<Booking?> GetBookingByIdAsync(Guid bookingId);
}