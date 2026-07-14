using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Publishers;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(string topic, T @event, string key, CancellationToken cancellationToken = default);
}
