using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Storage;

public interface IBookingStorage
{
    Task AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(Guid id);
    Task<IEnumerable<Booking>> GetAllAsync();
    Task UpdateAsync(Booking booking);
    Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status);
}