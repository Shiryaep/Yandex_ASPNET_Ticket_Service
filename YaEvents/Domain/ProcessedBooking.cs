namespace YaEvents.Domain;

public class ProcessedBooking
{
    public Guid Id { get; private set; }

    public Guid BookingId { get; private set; }

    public Guid EventId { get; private set; }

    public int SeatsCount { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    private ProcessedBooking() { } // Для EF Core

    public ProcessedBooking(Guid bookingId, Guid eventId, int seatsCount, DateTime processedAt)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        EventId = eventId;
        SeatsCount = seatsCount;
        ProcessedAt = processedAt;
    }
}