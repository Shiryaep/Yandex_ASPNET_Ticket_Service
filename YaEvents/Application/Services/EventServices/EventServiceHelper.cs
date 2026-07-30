using YaEvents.Application.DTO;
using YaEvents.Domain;

namespace YaEvents.Application.Services.EventServices
{
    public static class EventServiceHelper
    {
        public static EventInfoDto ToInfo(Event @event) => new()
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats
        };
    }
}
