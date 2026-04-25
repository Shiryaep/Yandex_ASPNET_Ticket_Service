using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Storage;

/// <summary>
/// Interface for booking storage operations
/// </summary>
public interface IBookingStorage
{
    /// <summary>
    /// Adds a new booking to the storage
    /// </summary>
    /// <param name="booking">Booking to add</param>
    Task AddAsync(Booking booking);

    /// <summary>
    /// Retrieves a booking by its unique identifier
    /// </summary>
    /// <param name="id">Booking identifier</param>
    /// <returns>Booking if found; otherwise null</returns>
    Task<Booking?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all bookings from the storage
    /// </summary>
    /// <returns>Collection of all bookings</returns>
    Task<IEnumerable<Booking>> GetAllAsync();

    /// <summary>
    /// Updates an existing booking in the storage
    /// </summary>
    /// <param name="booking">Updated booking data</param>
    Task UpdateAsync(Booking booking);

    /// <summary>
    /// Retrieves bookings filtered by status
    /// </summary>
    /// <param name="status">Booking status to filter by</param>
    /// <returns>Collection of bookings with the specified status</returns>
    Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status);
}