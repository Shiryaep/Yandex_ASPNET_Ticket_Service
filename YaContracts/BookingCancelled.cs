namespace YaContracts;

public class BookingCancelled
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public int SeatsCount { get; set; }
}
