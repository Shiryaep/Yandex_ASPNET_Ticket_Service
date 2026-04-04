using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary>
/// Service for managing bookings
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingStorage _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingService"/> class
    /// </summary>
    /// <param name="storage">Booking storage dependency</param>
    public BookingService(IBookingStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Creates a new booking for the specified event
    /// </summary>
    /// <param name="eventId">Identifier of the event to book</param>
    /// <returns>The created booking</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        var booking = new Booking(eventId);
        await _storage.AddAsync(booking);
        return booking;
    }

    /// <summary>
    /// Retrieves a booking by its identifier
    /// </summary>
    /// <param name="bookingId">Booking identifier</param>
    /// <returns>The booking if found; otherwise null</returns>
    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _storage.GetByIdAsync(bookingId);
    }
}