using Application.Repositories;
using Domain;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    private readonly AppDbContext _db = db;

    public Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<List<Guid>> GetListOfPendingBookings(CancellationToken cancellationToken = default)
    {
        return _db.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        return _db.Bookings.AddAsync(booking, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> GetCountOfUserBookings(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return _db.Bookings.Where(b => b.UserId == userId && b.EventId == eventId).CountAsync(cancellationToken);
    }
}