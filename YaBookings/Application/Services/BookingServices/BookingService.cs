using Application.DTO;
using Application.Repositories;
using Domain;
using Domain.Exceptions;
using YaContracts.Enums;

namespace Application.Services.BookingServices;

/// <summary>
/// Service for managing bookings
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BookingService"/> class
/// </remarks>
public class BookingService(IBookingRepository bookingRepository) : IBookingService
{
    private static readonly SemaphoreSlim _bookingLock = new(1, 1);
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <summary>
    /// Creates a new booking for the specified event
    /// </summary>
    /// <param name="eventId">Identifier of the event to book</param>
    /// <returns>The created booking</returns>
    public async Task<BookingInfoDto> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _bookingLock.WaitAsync(cancellationToken);
        try
        {
            var booking = Booking.CreatePending(eventId, userId);
            await _bookingRepository.AddBookingAsync(booking, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);

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
    /// <returns>The booking if found; otherwise throw an NotFoundException</returns>
    public async Task<BookingInfoDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found");

        return ToInfo(booking);
    }

    /// <summary>
    /// Cancel Booking and Release reserved seats
    /// </summary>
    public async Task<bool> CancelBookingByIdAsync(Guid bookingId, Guid userId, string userRole, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found");

        if (userId != booking.UserId && userRole != UserRoles.Admin.ToString())
            throw new LackOfRightsException();

        booking.Cancel();

        await _bookingRepository.SaveChangesAsync();
        return true;
    }

    public static BookingInfoDto ToInfo(Booking booking) => new()
    {
        Id = booking.Id,
        EventId = booking.EventId,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        ProcessedAt = booking.ProcessedAt,
        UserId = booking.UserId
    };
}