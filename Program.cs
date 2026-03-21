using Yandex_ASPNET_Ticket_Service.Middleware;
using Yandex_ASPNET_Ticket_Service.Models;

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

        builder.Services.AddSingleton<IEventService, EventService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Custom Middleware for error handling
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseRouting();

        app.MapControllers();

        app.Run();
    }
}
