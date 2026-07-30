using Microsoft.EntityFrameworkCore;
using System.Text;
using YaBookings.Application.DependencyInjection;
using YaBookings.Infrastructure.DataAccess;
using YaBookings.Infrastructure.DependencyInjection;
using YaBookings.Presentation.DependencyInjection;
using YaBookings.Presentation.Middleware;

namespace YaBookings.Presentation;

/// <summary> Basis of all project </summary>
public partial class Program
{
    /// <summary> Program entery point </summary>
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation(builder.Configuration);

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
        else if (app.Environment.IsEnvironment("Docker"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
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
        app.UseAuthentication();

        //6. Authorization
        app.UseAuthorization();

        //7. Endpoints
        app.MapControllers();

        app.Run();
    }
}