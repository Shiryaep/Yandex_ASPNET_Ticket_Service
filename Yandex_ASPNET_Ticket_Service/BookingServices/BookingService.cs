using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.BookingServices;

/// <summary> Service for events manipulation </summary>
public class BookingService : IBookingService
{
    private List<Booking> bookings = new List<Booking>();

    /// <summary> Creates new booking on event with eventId </summary>
    public void CreateBookingAsync(Guid eventId)
    {
        bookings.Add(new Booking(eventId));
    }

    /// <summary> Return booking by bookingId </summary>
    public Booking? GetBookingByIdAsync(Guid bookingId)
    {
        return bookings.FirstOrDefault(b => b.Id == bookingId);
    }
}