using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using YaBookings.Domain.Exceptions;

namespace YaBookings.Presentation.Middleware;

/// <summary>
/// Middleware for global exception handling
/// Intercepts unhandled exceptions and returns a structured response in the "Problem Details (RFC 7807)" format
/// </summary>
/// <remarks>
/// Constructor for ExceptionHandlingMiddleware
/// </remarks>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
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
            //400
            ArgumentException or
            ValidationException or
            InvalidOperationException or
            AlreadyEndedEventException =>
                ((int)HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            //401
            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
            //404
            KeyNotFoundException or
            FileNotFoundException or
            NotFoundException =>
                ((int)HttpStatusCode.NotFound, "Not Found", exception.Message),
            //409
            AlreadyCancelledException or
            BookingLimitExceededException =>
                ((int)HttpStatusCode.Conflict, "Conflict", exception.Message),
            //501
            NotImplementedException =>
                ((int)HttpStatusCode.NotImplemented, "Not Implemented", exception.Message),
            //500
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
            403 => "https://tools.ietf.org/html/rfc7235#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5"
        };
    }
}