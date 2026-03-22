using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Yandex_ASPNET_Ticket_Service.Middleware;

/// <summary>
/// Middleware for global exception handling
/// Intercepts unhandled exceptions and returns a structured response in the "Problem Details (RFC 7807)" format
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Constructor for ExceptionHandlingMiddleware
    /// </summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Attempt to pass the request further along the pipeline
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapExceptionToProblemDetails(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = GetProblemType(statusCode)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }

    private static (int statusCode, string title, string detail) MapExceptionToProblemDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException or InvalidOperationException => 
                ((int)HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            KeyNotFoundException or FileNotFoundException => 
                ((int)HttpStatusCode.NotFound, "Not Found", exception.Message),
            UnauthorizedAccessException => 
                ((int)HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
            NotImplementedException => 
                ((int)HttpStatusCode.NotImplemented, "Not Implemented", exception.Message),
            _ => 
                ((int)HttpStatusCode.InternalServerError, "Internal Server Error", 
                 "An unexpected error occurred. Please try again later.")
        };
    }

    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5"
        };
    }
}