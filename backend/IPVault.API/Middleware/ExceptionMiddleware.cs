using System.Net;
using System.Text.Json;
using IPVault.API.DTOs.Common;
using IPVault.API.Helpers;

namespace IPVault.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false
        };

        if (exception is AppException appEx)
        {
            context.Response.StatusCode = (int)appEx.StatusCode;
            response.StatusCode = (int)appEx.StatusCode;
            response.Message = appEx.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.Message = exception.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.Message = "Unauthorized access.";
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Message = "An unexpected internal server error occurred. Please try again later.";
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
