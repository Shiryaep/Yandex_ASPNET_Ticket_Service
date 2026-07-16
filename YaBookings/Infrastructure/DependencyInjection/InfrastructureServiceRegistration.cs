using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaBookings.Application.Publishers;
using YaBookings.Application.Repositories;
using YaBookings.Infrastructure.DataAccess;
using YaBookings.Infrastructure.Publishers;
using YaBookings.Infrastructure.Repositories;

namespace YaBookings.Infrastructure.DependencyInjection
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
                Acks = Acks.All,
                EnableIdempotence = true
            };

            services.AddSingleton<IProducer<string, string>>(_ =>
            {
                var builder = new ProducerBuilder<string, string>(producerConfig);
                builder.SetValueSerializer(Serializers.Utf8);
                builder.SetKeySerializer(Serializers.Utf8);
                return builder.Build();
            });

            services.AddSingleton<IDomainEventPublisher, KafkaEventPublisher>();

            return services;
        }
    }
}
