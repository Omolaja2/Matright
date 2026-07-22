using System.Net;
using System.Text.Json;
using PharMarket.Exceptions;

namespace PharMarket.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            BadRequestException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            ConflictException => (int)HttpStatusCode.Conflict,
            ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var message = exception switch
        {
            NotFoundException notFoundEx => notFoundEx.Message,
            BadRequestException badRequestEx => badRequestEx.Message,
            ValidationException validationEx => validationEx.Message,
            _ => "An unexpected error occurred. Please try again later."
        };

        var isAjax = context.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || context.Request.Headers.Accept.ToString().Contains("application/json")
            || context.Request.ContentType?.Contains("application/json") == true
            || context.Request.Path.Value?.StartsWith("/Apprentice/ProcessCart") == true;

        if (isAjax || context.Request.Path.Value?.StartsWith("/Apprentice/") == true)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
        else
        {
            context.Response.StatusCode = statusCode;
            context.Response.Redirect($"/Home/Error?statusCode={statusCode}&message={Uri.EscapeDataString(message)}");
        }
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
