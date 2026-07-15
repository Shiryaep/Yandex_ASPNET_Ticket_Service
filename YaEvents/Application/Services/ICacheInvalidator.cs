using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Domain;

namespace YaEvents.Application.Services;

public interface ICacheInvalidator
{
    Task UpdateEventInCacheAsync(Event value);
    Task DeleteEventFromCacheAsync(Guid valueId);
}
