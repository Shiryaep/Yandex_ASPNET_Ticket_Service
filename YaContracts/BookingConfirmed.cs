namespace YaContracts;

public record BookingConfirmed
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ConfirmedAt { get; set; }
    public int SeatsCount { get; set; }
}
