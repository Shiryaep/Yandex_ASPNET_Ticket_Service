using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaUsers.Application.Services;

namespace YaUsers.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
