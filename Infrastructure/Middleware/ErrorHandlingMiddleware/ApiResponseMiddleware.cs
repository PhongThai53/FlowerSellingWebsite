using ProjectGreenLens.Models.DTOs;
using System.Text.Json;

namespace ProjectGreenLens.Infrastructure.Middleware.ErrorHandlingMiddleware
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                // Read Content
                await _next(context);
                memoryStream.Position = 0;
                var body = await new StreamReader(memoryStream).ReadToEndAsync();
                memoryStream.Position = 0;

                // Read Error
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    var data = string.IsNullOrWhiteSpace(body) ? null : JsonSerializer.Deserialize<object>(body);

                    // Ensure 'data' is not null before passing it to ApiResponse<object>.Ok
                    var response = ApiResponse<object>.Ok(data ?? new object(), "Request processed successfully");

                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength = json.Length;
                    await context.Response.WriteAsync(json);
                }
                else
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
