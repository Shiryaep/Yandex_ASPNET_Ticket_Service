using YaEvents.Domain;

namespace YaEvents.Application.Repositories;

public interface IProcessedBookingsRepository
{
    Task<bool> TryMarkAsProcessedAsync(ProcessedBooking processedBooking, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken);
}