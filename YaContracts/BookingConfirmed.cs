namespace YaContracts;

public class BookingConfirmed
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int SeatsCount { get; set; }
}
