using Application.DTO;
using Application.Repositories;
using Domain;
using Domain.Exceptions;

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
            // Get the event
            // МЫ ТИПО УВЕРЕНЫ ЧТО СОБЫТИЕ ТОЧНО ТАКОГО АЙДИ, ЧТО У НАС ЕСТЬ)))

            // И МЫ ТОЧНО УВЕРЕНЫ, ЧТО СОБЫТИЕ В БУДУЩЕМ

            // А ЕЩЕ МЫ УВЕРЕНЫ ЧТО ПОЛЬЗОВАТЕЛЬ ТОЧНО СУЩЕСТВУЕТ 

            // А ЕЩЕ МЫ ТОЧНО ПРОВЕРИЛИ, ЧТО КОЛИЧЕСТВО БУКИНГОВ У ЧЕЛОВЕКА МЕНЬШЕ 10!!!

            // ЭТО НАДО ПЕРЕНЕСТИ В ОБРАБОТКУ НАЧАВШЕГОСЯ БУКИНГА В ИВЕНТ СЕРВИС

            // Create and save booking
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
    public async Task<bool> CancelBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found");

        // ТУТ МЫ УВЕРЕНЫ ЧТО И ПОЛЬЗОВАТЕЛЬ СУЩЕСТВУЕТ И ИВЕНТ ВАЛИДНЫЙ
        // ТУТ НАДО КАК-ТО ПРОВЕРИТЬ ЧТО ЛИБО ЮЗЕР ТАКОЙ ЖЕ, ЛИБО ПОЛЬЗОВАТЕЛЬ АДМИН - КАК ВАРИК ВЗЯТЬ ИЗ ТОКЕНА
        //if (user.Id != booking.UserId && user.Role != UserRoles.Admin)
        //throw new LackOfRightsException();

        booking.Cancel();

        // ЗАРЕЛИЗИТЬ МЕСТА!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // ЭТО НАДО СДЕЛАТЬ В СЕРВИСЕ ИВЕНТОВ ПОСЛЕ ВЫСЫЛКИ СООБЩЕНИЯ О ТОМ, ЧТО ВСЕ ОКЕЙ

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