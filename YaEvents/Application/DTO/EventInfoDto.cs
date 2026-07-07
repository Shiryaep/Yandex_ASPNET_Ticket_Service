namespace Application.DTO;

public class EventInfoDto
{
    public required Guid Id { get; set; }
    public required string? Title { get; set; }
    public string? Description { get; set; }
    public required DateTime? StartAt { get; set; } = null;
    public required DateTime? EndAt { get; set; } = null;
    public required int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}