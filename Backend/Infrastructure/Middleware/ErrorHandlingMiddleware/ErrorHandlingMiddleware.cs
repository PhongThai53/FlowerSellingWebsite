using FlowerSellingWebsite.Exceptions;
using FlowerSellingWebsite.Models.DTOs;
using System.Text.Json;

namespace FlowerSellingWebsite.Infrastructure.Middleware.ErrorHandlingMiddleware
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
                // Chỉ call next - không xử lý success response
                await _next(context);
            }
            catch (NotFoundException notFoundException)
            {
                _logger.LogWarning("Not found exception: {Message}", notFoundException.Message);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";

                var errorResponse = ApiResponse<object>.Fail(notFoundException.Message);
                await WriteJsonResponse(context, errorResponse);
            }
            catch (ValidationException validationException)
            {
                _logger.LogWarning("Validation exception: {Message}", validationException.Message);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var errorResponse = ApiResponse<object>.Fail(validationException.Message);
                await WriteJsonResponse(context, errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = ApiResponse<object>.Fail("Internal server error");
                await WriteJsonResponse(context, errorResponse);
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
