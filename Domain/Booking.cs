using Domain.Exceptions;

namespace Domain;

/// <summary> Booking entity </summary> 
public class Booking
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Event? Event { get; set; }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public Booking(Guid eventID)
    {
        Id = Guid.NewGuid();
        EventId = eventID;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Event = null;
    }

    private Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Booking CreatePending(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ValidationException("EventId cannot be empty");

        return new Booking(Guid.NewGuid(), eventId, BookingStatus.Pending, DateTime.UtcNow);
    }

    private Booking()
    {
    }
}