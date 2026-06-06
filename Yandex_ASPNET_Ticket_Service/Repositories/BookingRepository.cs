using Microsoft.EntityFrameworkCore;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Repositories;

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
}