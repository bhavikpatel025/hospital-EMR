using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMR.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass the request down the middleware pipeline to static files, auth, and controllers
            await _next(context);
        }
        catch (Exception ex)
        {
            // Catch any unhandled C# exception anywhere in the application
            _logger.LogError(ex, "Unhandled exception occurred while processing request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred on the server. Please try again later or contact support.";

        // Map known exception types to proper HTTP status codes and safe user messages
        if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            message = "You are not authorized to perform this action or access this resource.";
        }
        else if (exception is KeyNotFoundException || exception is FileNotFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            message = exception.Message ?? "The requested resource or file was not found.";
        }
        else if (exception is ArgumentException || exception is InvalidOperationException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        context.Response.StatusCode = statusCode;

        var responsePayload = new
        {
            success = false,
            statusCode = statusCode,
            message = message,
            detail = exception.InnerException?.Message ?? exception.Message
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(responsePayload, options);
        await context.Response.WriteAsync(json);
    }
}
