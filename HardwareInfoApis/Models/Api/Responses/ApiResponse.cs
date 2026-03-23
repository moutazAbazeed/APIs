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
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message, ApiErrorCode errorCode)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode.ToString()
            };
        }
    }

    // Convenience non-generic version
    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse Success(string message = "Operation successful")
        {
            return new ApiResponse
            {
                
                Message = message,
            };
        }

        public new static ApiResponse Error(string message, ApiErrorCode errorCode)
        {
            return new ApiResponse
            {
                Message = message,
                ErrorCode = errorCode.ToString()
            };
        }
    }
}