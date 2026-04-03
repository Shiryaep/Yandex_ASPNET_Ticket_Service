using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary> Service for events manipulation </summary>
public class BookingService : IBookingService
{
    private readonly IBookingStorage _storage;

    public BookingService(IBookingStorage storage)
    {
        _storage = storage;
    }

    /// <summary> Creates new booking on event with eventId </summary>
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        var booking = new Booking(eventId);
        await _storage.AddAsync(booking);
        return booking;
    }

    /// <summary> Return booking by bookingId </summary>
    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _storage.GetByIdAsync(bookingId);
    }
}