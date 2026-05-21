namespace Yandex_ASPNET_Ticket_Service.Models.DTO;

public sealed record UpdateEventDto
{
    public string? Title { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public string? Description { get; init; }
}
