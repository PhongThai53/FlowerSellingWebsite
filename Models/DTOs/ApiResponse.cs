using System.Text.Json;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public ApiResponse(bool succeeded, string message, T data, List<string>? errors = null)
        {
            Succeeded = succeeded;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public ApiResponse()
        {
            Message = string.Empty;
        }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>(false, message, default(T)!, errors);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
