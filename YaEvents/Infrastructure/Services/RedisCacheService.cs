using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using YaEvents.Application.Services;

namespace YaEvents.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger)
    {
        _redis = connection.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _redis.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return System.Text.Json.JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Get failed for key '{Key}'. Cache degraded.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await _redis.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Set failed for key '{Key}'. Cache degraded.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _redis.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Remove failed for key '{Key}'. Cache degraded.", key);
        }
    }
}
