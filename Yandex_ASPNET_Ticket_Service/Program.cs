using Yandex_ASPNET_Ticket_Service.Middleware;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;
using Yandex_ASPNET_Ticket_Service.Storage;

namespace Yandex_ASPNET_Ticket_Service;

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

        builder.Services.AddSingleton<IBookingService, BookingService>();
        builder.Services.AddSingleton<IEventService, EventService>();
        builder.Services.AddSingleton<IBookingStorage, InMemoryBookingStorage>();

        var app = builder.Build();

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
