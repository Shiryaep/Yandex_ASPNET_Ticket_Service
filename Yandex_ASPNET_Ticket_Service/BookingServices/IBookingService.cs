using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.BookingServices;

/// <summary> Service Interface for booking manipulation </summary>
public interface IBookingService
{
    /// <summary> Creates new booking on event with eventId </summary>
    public void CreateBookingAsync(Guid eventId);

    /// <summary> Return booking by bookingId </summary>
    public Booking? GetBookingByIdAsync(Guid bookingId);
}