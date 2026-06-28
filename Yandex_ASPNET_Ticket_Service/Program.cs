using Application.DependencyInjection;
using Infrastructure.DataAccess;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Presentation.Middleware;
using System.Text;

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
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5 * 60),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

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
        app.UseAuthentication();

        //6. Authorization
        app.UseAuthorization();

        //7. Endpoints
        app.MapControllers();

        app.Run();
    }
}