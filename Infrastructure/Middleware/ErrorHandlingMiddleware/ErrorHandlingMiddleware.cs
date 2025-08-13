using ProjectGreenLens.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ProjectGreenLens.Infrastructure.Middleware.ErrorHandlingMiddleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex) // lỗi validation
            {
                _logger.LogWarning(vex, "Validation failed");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var response = ApiResponse<object>.Fail(
                    message: "Validation failed",
                    errors: new List<string> { vex.Message }
                );

                await WriteJsonResponse(context, response);
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning(uex, "Unauthorized request");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                var response = ApiResponse<object>.Fail(
                    message: "Unauthorized",
                    errors: new List<string> { uex.Message }
                );

                await WriteJsonResponse(context, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = ApiResponse<object>.Fail(
                    message: "An unexpected error occurred",
                    errors: new List<string> { ex.Message }
                );

                await WriteJsonResponse(context, response);
            }
        }

        private async Task WriteJsonResponse<T>(HttpContext context, ApiResponse<T> response)
        {
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        }
    }
}
