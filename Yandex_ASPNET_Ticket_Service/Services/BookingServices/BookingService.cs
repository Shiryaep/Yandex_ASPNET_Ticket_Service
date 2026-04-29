using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary>
/// Service for managing bookings
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BookingService"/> class
/// </remarks>
/// <param name="storage">Booking storage dependency</param>
/// <param name="eventService">Event service dependency</param>
public class BookingService(IBookingStorage storage, IEventService eventService) : IBookingService
{
    private readonly IBookingStorage _storage = storage;
    private readonly IEventService _eventService = eventService;
    private readonly SemaphoreSlim _bookingLock = new(1, 1);

    /// <summary>
    /// Creates a new booking for the specified event
    /// </summary>
    /// <param name="eventId">Identifier of the event to book</param>
    /// <returns>The created booking</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        await _bookingLock.WaitAsync();
        try
        {
            // Get the event
            var eventEntity = _eventService.GetEvent(eventId) ?? throw new ArgumentException($"Event with id {eventId} not found");

            // Try to reserve one seat
            if (!eventEntity.TryReserveSeats(1))
            {
                throw new NoAvailableSeatsException();
            }

            // Update the event in storage (since TryReserveSeats modified the object)
            _eventService.UpdateEvent(eventId, eventEntity);

            // Create and save booking
            var booking = new Booking(eventId);
            await _storage.AddAsync(booking);
            return booking;
        }
        finally
        {
            _bookingLock.Release();
        }
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