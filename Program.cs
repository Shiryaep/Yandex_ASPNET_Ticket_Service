
namespace Yandex_ASPNET_Ticket_Service;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("event", typeof(Event));
        });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IEventService, EventService>();

        var app = builder.Build();

        app.MapControllers();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}
