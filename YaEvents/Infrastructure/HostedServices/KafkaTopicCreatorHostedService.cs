using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.HostedServices;

public class KafkaTopicCreatorHostedService : IHostedService
{
    private readonly ILogger<KafkaTopicCreatorHostedService> _logger;
    private readonly KafkaConsumerSettings _settings;

    public KafkaTopicCreatorHostedService(
        ILogger<KafkaTopicCreatorHostedService> logger,
        IOptions<KafkaConsumerSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Создание топиков Kafka");

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _settings.BootstrapServers
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topicExists = metadata.Topics.Any(t => t.Topic == _settings.Topics.BookingConfirmed);

            if (topicExists)
            {
                _logger.LogInformation("Топик {TopicName} уже существует", _settings.Topics.BookingConfirmed);
                return;
            }

            var topicSpecification = new TopicSpecification
            {
                Name = _settings.Topics.BookingConfirmed,
                NumPartitions = 3,
                ReplicationFactor = 1
            };

            await adminClient.CreateTopicsAsync(new[] { topicSpecification });
            _logger.LogInformation("Топик {TopicName} успешно создан", _settings.Topics.BookingConfirmed);
        }
        catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.IsError && r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation("Топик {TopicName} уже существует (конкурентное создание)", _settings.Topics.BookingConfirmed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось создать топик {TopicName}. Подписчик продолжит работу", _settings.Topics.BookingConfirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}