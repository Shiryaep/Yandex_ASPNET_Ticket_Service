using Microsoft.EntityFrameworkCore;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;

namespace Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary>
/// Service for managing bookings
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BookingService"/> class
/// </remarks>
public class BookingService(AppDbContext context) : IBookingService
{
    private static readonly SemaphoreSlim _bookingLock = new(1, 1);
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Creates a new booking for the specified event
    /// </summary>
    /// <param name="eventId">Identifier of the event to book</param>
    /// <returns>The created booking</returns>
    public async Task<BookingInfoDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _bookingLock.WaitAsync(cancellationToken);
        try
        {
            // Get the event
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
                ?? throw new Exception($"Event with id {eventId} not found");

            // Try to reserve seat
            if (!eventEntity.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            // Create and save booking
            var booking = Booking.CreatePending(eventId);
            await _context.Bookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return ToInfo(booking);
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
    public async Task<BookingInfoDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new Exception("Booking not found");

        return ToInfo(booking);
    }

    public static BookingInfoDto ToInfo(Booking booking) => new()
    {
        Id = booking.Id,
        EventId = booking.EventId,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt
    };
}