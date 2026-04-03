using System.Collections.Concurrent;
using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Storage;

/// <summary>
/// In-memory implementation of booking storage using concurrent dictionary
/// </summary>
public class InMemoryBookingStorage : IBookingStorage
{
    private ConcurrentDictionary<Guid, Booking> _bookingDictionary = new ();

    /// <summary>
    /// Adds a new booking to the storage
    /// </summary>
    /// <param name="booking">Booking to add</param>
    /// <exception cref="InvalidOperationException">Thrown when booking with same ID already exists</exception>
    public Task AddAsync(Booking booking)
    {
        if (!_bookingDictionary.TryAdd(booking.Id, booking))
        {
            throw new InvalidOperationException($"Booking with id {booking.Id} already exists");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a booking by its unique identifier
    /// </summary>
    /// <param name="id">Booking identifier</param>
    /// <returns>Booking if found; otherwise null</returns>
    public Task<Booking?> GetByIdAsync(Guid id)
    {
        _bookingDictionary.TryGetValue(id, out var booking);
        return Task.FromResult(booking);
    }

    /// <summary>
    /// Retrieves all bookings from the storage
    /// </summary>
    /// <returns>Collection of all bookings</returns>
    public Task<IEnumerable<Booking>> GetAllAsync()
    {
        return Task.FromResult(_bookingDictionary.Values.AsEnumerable());
    }

    /// <summary>
    /// Updates an existing booking in the storage
    /// </summary>
    /// <param name="booking">Updated booking data</param>
    /// <exception cref="KeyNotFoundException">Thrown when booking with specified ID is not found</exception>
    public Task UpdateAsync(Booking booking)
    {
        if (!_bookingDictionary.ContainsKey(booking.Id))
        {
            throw new KeyNotFoundException($"Booking with id {booking.Id} not found");
        }

        _bookingDictionary[booking.Id] = booking;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves bookings filtered by status
    /// </summary>
    /// <param name="status">Booking status to filter by</param>
    /// <returns>Collection of bookings with the specified status</returns>
    public Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status)
    {
        var result = _bookingDictionary.Values.Where(b => b.Status == status);
        return Task.FromResult(result);
    }
}