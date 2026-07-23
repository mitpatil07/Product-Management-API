using System.Collections.Generic;

namespace ProductManagement.API.Common
{
    /// <summary>
    /// Unified response format returned by all API endpoints.
    /// </summary>
    /// <typeparam name="T">Type of the payload data.</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public T? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, int statusCode, T? data = default, IEnumerable<string>? errors = null)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
            Data = data;
            Errors = errors ?? new List<string>();
        }
    }

    /// <summary>
    /// Non-generic variant of ApiResponse for endpoints without payload return data.
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, int statusCode, object? data = null, IEnumerable<string>? errors = null)
            : base(success, message, statusCode, data, errors)
        {
        }
    }
}
