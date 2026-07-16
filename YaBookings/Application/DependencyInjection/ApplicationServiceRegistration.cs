using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaBookings.Application.Services.BookingServices;
using YaBookings.Application.Services.HostedServices;

namespace YaBookings.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBookingService, BookingService>();

            services.AddHostedService<BookingBackgroundService>();

            return services;
        }
    }
}
