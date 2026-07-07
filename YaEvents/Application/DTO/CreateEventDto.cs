namespace Application.DTO;

public class CreateEventDto
{
    public required string? Title { get; set; }
    public string? Description { get; set; }
    public required DateTime? StartAt { get; set; } = null;
    public required DateTime? EndAt { get; set; } = null;
    public required int TotalSeats { get; set; }
}