using Application.Repositories;
using Confluent.Kafka;
using Infrastructure.Consumers;
using Infrastructure.DataAccess;
using Infrastructure.HostedServices;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEventRepository, EventRepository>();

            KafkaConsumerSettings consumerSettings = new KafkaConsumerSettings()
            {
                BootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Value,
                ConsumerGroup = configuration.GetSection("Kafka:ConsumerGroup").Value
            };

            services.AddSingleton(consumerSettings);

            //services.Configure<KafkaConsumerSettings>(configuration.GetSection("Kafka"));

            //var settings = configuration.GetSection("Kafka").Get<KafkaConsumerSettings>()
            //    ?? throw new InvalidOperationException("Kafka settings not found");

            // Регистрируем Consumer как Singleton (тяжелый объект)
            services.AddSingleton<IConsumer<string, string>>(_ =>
            {
                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = consumerSettings.BootstrapServers,
                    GroupId = consumerSettings.ConsumerGroup,
                    // AutoOffsetReset.Earliest — начинать чтение с самого начала, если нет сохраненного offset
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    // EnableAutoCommit = false — подтверждаем вручную после успешной обработки
                    EnableAutoCommit = false
                };

                var builder = new ConsumerBuilder<string, string>(consumerConfig);
                builder.SetValueDeserializer(Deserializers.Utf8);
                builder.SetKeyDeserializer(Deserializers.Utf8);

                return builder.Build();
            });

            // ВАЖНО: Порядок регистрации имеет значение!
            // 1. Сначала регистрируем TopicCreator (запустится первым)
            services.AddHostedService<KafkaTopicCreatorHostedService>();

            // 2. Потом регистрируем Subscriber (запустится вторым)
            services.AddHostedService<BookingConfirmedSubscriber>();

            return services;
        }
    }
}
