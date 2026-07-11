using Application.Publishers;
using Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YaContracts;
using YaContracts.Enums;

namespace Application.Services.HostedServices;

/// <summary>
/// Background service that periodically resolves pending bookings by confirming or rejecting them
/// </summary>
public class BookingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingBackgroundService> logger,
        IDomainEventPublisher domainEventPublisher) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<BookingBackgroundService> _logger = logger;
    private readonly IDomainEventPublisher _domainEventPublisher = domainEventPublisher;
    private readonly int _pollingInterval = 3000;
    private readonly int _processingDelay = 2000;

    /// <summary>
    /// Executes the background processing loop
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the service</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                List<Guid> pendingBookingIds;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    pendingBookingIds = await bookingRepository.GetListOfPendingBookings(cancellationToken);
                }

                var tasks = pendingBookingIds.Select(id =>
                    ProcessBookingAsync(id, cancellationToken));

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing pending bookings");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a single booking asynchronously
    /// </summary>
    /// <param name="bookingId">Booking to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_processingDelay, cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (booking == null || booking.Status != BookingStatus.Pending)
                return;

            booking.Confirm();
            await bookingRepository.SaveChangesAsync(cancellationToken);

            var bookingConfirmedEvent = new BookingConfirmed()
            {
                BookingId = bookingId,
                EventId = booking.EventId,
                UserId = booking.UserId,
                ConfirmedAt = booking.ProcessedAt ?? DateTime.UtcNow,
                SeatsCount = 1
            };

            await _domainEventPublisher.PublishAsync(Constants.BookingConfirmedTopicName, bookingConfirmedEvent, booking.EventId.ToString());

            _logger.LogInformation("Booking {BookingId} confirmed", booking.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            await RejectBookingOnErrorAsync(bookingId, ex, cancellationToken);
        }
    }

    /// <summary>
    /// Rejects a booking when an error occurs during processing
    /// </summary>
    private async Task RejectBookingOnErrorAsync(Guid bookingId, Exception ex, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var booking = await bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (booking == null)
                return;

            booking.Reject();

            await bookingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex,
                "Booking {BookingId} rejected due to processing error",
                bookingId);
        }
        catch (Exception releaseEx)
        {
            _logger.LogError(releaseEx,
                "Failed to reject booking {BookingId} after error",
                bookingId);
        }
    }
}