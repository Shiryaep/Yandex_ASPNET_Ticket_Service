using Microsoft.EntityFrameworkCore;
using YaContracts;
using YaEvents.Application.Repositories;
using YaEvents.Domain;
using YaEvents.Infrastructure.DataAccess;

namespace YaEvents.Infrastructure.Repositories;

public class ProcessedBookingsRepository(AppDbContext db) : IProcessedBookingsRepository
{
    private readonly AppDbContext _db = db;

    public async Task<bool> TryMarkAsProcessedAsync(
        ProcessedBooking processedBooking,
        CancellationToken cancellationToken)
    {
        try
        {
            _db.ProcessedBookings.Add(processedBooking);
            await _db.SaveChangesAsync(cancellationToken);
            return true; // Successfull append
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Booking from Kafka already processed
            return false;
        }
    }

    public async Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return await _db.ProcessedBookings
        .AnyAsync(pb => pb.BookingId == bookingId, cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException?.Message.Contains("unique constraint") == true ||
            ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            return true;
        }

        return false;
    }
}