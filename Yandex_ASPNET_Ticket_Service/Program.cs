using Application.DependencyInjection;
using Application.Repositories;
using Application.Services.BookingServices;
using Application.Services.EventServices;
using Application.Services.HostedServices;
using Infrastructure.DataAccess;
using Infrastructure.DependencyInjection;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Middleware;

namespace Presentation;

/// <summary> Basis of all project </summary>
public class Program
{
    /// <summary> Program entery point </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            var autoSwagger = bool.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_SWAGGER_AUTO_OPEN")?.ToLower() ?? "false");
            if (autoSwagger)
            {
                var port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT") ?? "5000";
                var swaggerUrl = $"https://localhost:{port}/swagger/index.html";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(swaggerUrl) { UseShellExecute = true });
            }
        }

        //1. HTTPS redirection
        app.UseHttpsRedirection();

        //2. Static Files Usage

        //2.5 Error handling
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        //3. Routing
        app.UseRouting();

        //4. CORS
        //5. Authentication
        //6. Authorization

        //7. Endpoints
        app.MapControllers();

        app.Run();
    }
}