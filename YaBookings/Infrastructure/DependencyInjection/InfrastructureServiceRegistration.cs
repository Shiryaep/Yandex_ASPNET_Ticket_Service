using Application.Publishers;
using Application.Repositories;
using Confluent.Kafka;
using Infrastructure.DataAccess;
using Infrastructure.Publishers;
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

            services.AddScoped<IBookingRepository, BookingRepository>();

            var bootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Value;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                // Рекомендуется для надежности (ждать подтверждения от всех реплик)
                Acks = Acks.All,
                // Включаем идемпотентность для защиты от дублей при пересылке
                EnableIdempotence = true
            };

            // 3.1. Регистрируем продюсер как Singleton. 
            // IProducer в Confluent.Kafka потокобезопасен и спроектирован быть единственным на приложение.
            services.AddSingleton<IProducer<string, string>>(_ =>
            {
                var builder = new ProducerBuilder<string, string>(producerConfig);
                // По умолчанию для строк используется UTF-8 сериализатор, но можно указать явно
                builder.SetValueSerializer(Serializers.Utf8);
                builder.SetKeySerializer(Serializers.Utf8);
                return builder.Build();
            });

            // Регистрируем наш издатель
            services.AddSingleton<IDomainEventPublisher, KafkaEventPublisher>();

            return services;
        }
    }
}
