using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using YaEvents.Application.Repositories;
using YaEvents.Application.Services;
using YaEvents.Infrastructure.Consumers;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.HostedServices;
using YaEvents.Infrastructure.Repositories;
using YaEvents.Infrastructure.Services;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IProcessedBookingsRepository, ProcessedBookingsRepository>();

            services.Configure<KafkaConsumerSettings>(configuration.GetSection("Kafka"));

            var settings = configuration.GetSection("Kafka").Get<KafkaConsumerSettings>()
                ?? throw new InvalidOperationException("Kafka settings not found");

            services.AddSingleton<IConsumer<string, string>>(_ =>
            {
                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = settings.BootstrapServers,
                    GroupId = settings.ConsumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                var builder = new ConsumerBuilder<string, string>(consumerConfig);
                builder.SetValueDeserializer(Deserializers.Utf8);
                builder.SetKeyDeserializer(Deserializers.Utf8);

                return builder.Build();
            });

            services.AddHostedService<KafkaTopicCreatorHostedService>();

            services.AddHostedService<BookingConfirmedSubscriber>();

            return services;
        }
    }
}
