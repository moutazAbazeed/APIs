using System;

namespace HardwareInfoApis.Api.Models.Api
{
    public class ApiException : Exception
    {
        public ApiErrorCode ErrorCode { get; }

        public ApiException(string message, ApiErrorCode errorCode = ApiErrorCode.ServerError)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ApiException(string message, Exception innerException, ApiErrorCode errorCode = ApiErrorCode.ServerError)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}