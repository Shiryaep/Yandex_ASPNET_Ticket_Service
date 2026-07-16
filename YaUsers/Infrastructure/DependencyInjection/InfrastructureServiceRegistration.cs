using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaUsers.Application.Repositories;
using YaUsers.Application.Services;
using YaUsers.Infrastructure.DataAccess;
using YaUsers.Infrastructure.Repositories;

namespace YaUsers.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddSingleton<IJWTService, JWTService>();

            return services;
        }
    }
}
