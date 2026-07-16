using YaEvents.Application.DTO;

namespace YaEvents.Application.Services;

public interface ICacheHelper
{
    Task UpdateEventInCacheAsync(EventInfoDto value);
    Task DeleteEventFromCacheAsync(Guid valueId);
    Task UpdateTopEventsInCacheAsync(List<EventInfoDto> topEvents);
}
