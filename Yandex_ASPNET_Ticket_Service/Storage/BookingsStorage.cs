using System.Collections.Concurrent;
using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Storage;

public class InMemoryBookingStorage : IBookingStorage
{
    private ConcurrentDictionary<Guid, Booking> _bookingDictionary = new ();

    public Task AddAsync(Booking booking)
    {
        if (!_bookingDictionary.TryAdd(booking.Id, booking))
        {
            throw new InvalidOperationException($"Booking with id {booking.Id} already exists");
        }

        return Task.CompletedTask;
    }

    public Task<Booking?> GetByIdAsync(Guid id)
    {
        _bookingDictionary.TryGetValue(id, out var booking);
        return Task.FromResult(booking);
    }

    public Task<IEnumerable<Booking>> GetAllAsync()
    {
        return Task.FromResult(_bookingDictionary.Values.AsEnumerable());
    }

    public Task UpdateAsync(Booking booking)
    {
        if (!_bookingDictionary.ContainsKey(booking.Id))
        {
            throw new KeyNotFoundException($"Booking with id {booking.Id} not found");
        }

        _bookingDictionary[booking.Id] = booking;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status)
    {
        var result = _bookingDictionary.Values.Where(b => b.Status == status);
        return Task.FromResult(result);
    }
}