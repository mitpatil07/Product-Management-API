using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.API.Common;

namespace ProductManagement.API.Middleware
{
    /// <summary>
    /// Middleware that catches all unhandled exceptions globally and formats them into a standardized ApiResponse structure.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            string message = _env.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred on the server.";

            var errors = _env.IsDevelopment()
                ? new[] { exception.StackTrace ?? string.Empty }
                : Array.Empty<string>();

            var apiResponse = new ApiResponse(
                success: false,
                message: message,
                statusCode: StatusCodes.Status500InternalServerError,
                data: null,
                errors: errors
            );

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(apiResponse, serializerOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
