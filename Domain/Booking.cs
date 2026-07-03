using Domain.Exceptions;

namespace Domain;

/// <summary> Booking entity </summary> 
public class Booking
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Event? Event { get; set; }
    public User? User { get; set; }

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

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new AlreadyCancelledException();
        }
        else
        {
            Status = BookingStatus.Cancelled;
        }
    }

    private Booking(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        UserId = userId;
    }

    public static Booking CreatePending(Guid eventId, Guid userId)
    {
        return new Booking(Guid.NewGuid(), eventId, userId, BookingStatus.Pending, DateTime.UtcNow);
    }

    private Booking() //Only for EF
    {
    }
}