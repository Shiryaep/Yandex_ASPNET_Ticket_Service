using Application.DTO;

namespace Application.Services.BookingServices;

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
    public Task<BookingInfoDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a booking by its identifier
    /// </summary>
    /// <param name="bookingId">Booking identifier</param>
    /// <returns>The booking if found; otherwise throw an NotFoundException</returns>
    public Task<BookingInfoDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    public Task<bool> CancelBookingByIdAsync(Guid bookingId, Guid userId, string userRole, CancellationToken cancellationToken = default);
}