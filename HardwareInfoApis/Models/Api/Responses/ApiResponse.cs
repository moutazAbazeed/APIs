using HardwareInfoApis.Models.Api;
using System;

namespace HardwareInfoApis.Api.Models.Api.Responses
{
    /// <summary>
    /// Standard API response wrapper for consistent error handling
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            var r = new ApiResponse<T>();
            r.Success = true;
            r.Message = message;
            r.Data = data;
            return r;
        }

        public static ApiResponse<T> Error(string message, ApiErrorCode errorCode)
        {
            var r = new ApiResponse<T>();
            r.Success = false;
            r.Message = message;
            r.ErrorCode = errorCode.ToString();
            return r;
        }

        public static ApiResponse<T> FromException(Exception ex, ApiErrorCode code = ApiErrorCode.ServerError)
        {
            var r = new ApiResponse<T>();
            r.Success = false;
            r.Message = "An unexpected error occurred.";
            r.ErrorCode = code.ToString();
            return r;
        }
    }

    // Convenience non-generic version
    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse Ok(string message = "Operation successful")
        {
            var r = new ApiResponse();
            r.Success = true;
            r.Message = message;
            return r;
        }

        public new static ApiResponse Error(string message, ApiErrorCode errorCode)
        {
            var r = new ApiResponse();
            r.Success = false;
            r.Message = message;
            r.ErrorCode = errorCode.ToString();
            return r;
        }
    }
}