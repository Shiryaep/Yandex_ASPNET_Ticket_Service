using Microsoft.Extensions.Options;
using YaContracts;
using YaEvents.Application.DTO;
using YaEvents.Application.Services;
using YaEvents.Domain;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.Services;

public class RedisCacheHelper(
        ICacheService cache,
        IOptions<RedisCacheSettings> settings) : ICacheHelper
{
    private readonly ICacheService _cache = cache;
    private readonly RedisCacheSettings _settings = settings.Value;

    public async Task UpdateEventInCacheAsync(EventInfoDto value)
    {
        var cacheKey = Constants.GetEventByIdCacheKey + value.Id.ToString();

        await _cache.SetAsync(cacheKey, value, TimeSpan.FromSeconds(_settings.EventsGetEventByIdTTLSeconds));
    }

    public async Task DeleteEventFromCacheAsync(Guid valueId)
    {
        var cacheKey = Constants.GetEventByIdCacheKey + valueId.ToString();

        await _cache.RemoveAsync(cacheKey);
    }

    public async Task UpdateTopEventsInCacheAsync(List<EventInfoDto> topEvents)
    {
        await _cache.SetAsync(Constants.TopEventsCacheKey, topEvents, TimeSpan.FromMinutes(_settings.EventsGetTopEventsTTLMinutes));
    }
}
