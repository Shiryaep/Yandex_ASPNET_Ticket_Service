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
