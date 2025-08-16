using FlowerSellingWebsite.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
                await _next(context);
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var response = ApiResponse<object>.Fail(
                    "Validation failed",
                    new List<string> { vex.Message }
                );

                await WriteJsonResponse(context, response);
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning(uex, "Unauthorized request");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                var response = ApiResponse<object>.Fail(
                    "Unauthorized",
                    new List<string> { uex.Message }
                );

                await WriteJsonResponse(context, response);
            }
            catch (KeyNotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Resource not found");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                var response = ApiResponse<object>.Fail(
                    "Resource not found",
                    new List<string> { nfex.Message }
                );

                await WriteJsonResponse(context, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = ApiResponse<object>.Fail(
                    "An unexpected error occurred",
                    new List<string> { ex.Message }
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
