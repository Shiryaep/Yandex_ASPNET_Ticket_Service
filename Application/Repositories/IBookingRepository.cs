using Domain;

namespace Application.Repositories;

public interface IBookingRepository
{
    public Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<List<Guid>> GetListOfPendingBookings(CancellationToken cancellationToken = default);

    public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}