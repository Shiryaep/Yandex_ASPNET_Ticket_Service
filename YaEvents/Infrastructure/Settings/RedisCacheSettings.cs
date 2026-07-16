namespace YaEvents.Infrastructure.Settings;

public class RedisCacheSettings
{
    public int EventsGetEventByIdTTLSeconds { get; set; } = 10;
    public int EventsGetTopEventsTTLMinutes { get; set; } = 60;
}
