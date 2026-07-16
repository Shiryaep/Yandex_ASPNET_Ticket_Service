using YaBookings.Domain;

namespace YaBookings.Application.Repositories;

public interface IBookingRepository
{
    public Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<List<Guid>> GetListOfPendingBookings(CancellationToken cancellationToken = default);

    public Task<int> GetCountOfUserBookings(Guid userId, Guid eventId, CancellationToken cancellationToken = default);

    public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}