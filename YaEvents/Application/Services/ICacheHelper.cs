using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using YaEvents.Application.DTO;
using YaEvents.Domain;

namespace YaEvents.Application.Services;

public interface ICacheHelper
{
    Task UpdateEventInCacheAsync(EventInfoDto value);
    Task DeleteEventFromCacheAsync(Guid valueId);
    Task UpdateTopEventsInCacheAsync(List<EventInfoDto> topEvents);
}
