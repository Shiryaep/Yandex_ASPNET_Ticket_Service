using Microsoft.Extensions.Options;
using YaEvents.Application.Services;
using YaEvents.Domain;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.Services;

public class RedisCacheInvalidator(
        ICacheService cache,
        IOptions<RedisCacheSettings> settings) : ICacheInvalidator
{
    private readonly ICacheService _cache = cache;
    private readonly RedisCacheSettings _settings = settings.Value;

    public async Task UpdateEventInCacheAsync(Event value)
    {
        var cacheKey = $"event:{value.Id}";

        await _cache.SetAsync(cacheKey, value, TimeSpan.FromSeconds(_settings.EventsGetEventByIdTTLSeconds));
    }

    public async Task DeleteEventFromCacheAsync(Guid valueId)
    {
        var cacheKey = $"event:{valueId}";

        await _cache.RemoveAsync(cacheKey);
    }
}
