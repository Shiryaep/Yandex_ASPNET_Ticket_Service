using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaEvents.Application.Services.EventServices;

namespace YaEvents.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
