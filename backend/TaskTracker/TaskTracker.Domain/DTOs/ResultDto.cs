using System.Net;
using System.Text.Json.Serialization;

namespace TaskTracker.Domain.DTOs
{
    public class ResultDto<T>
    {
        public bool IsSuccess { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        [JsonInclude][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Ensure returns in API response, but only if not null
        public string? ErrorMessage { get; private set; }

        [JsonInclude][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Ensure returns in API response, but only if not null
        public T? Data { get; private set; }

        public static ResultDto<T> Success(T data, HttpStatusCode statusCode) => new() { IsSuccess = true, StatusCode = statusCode, Data = data };
        public static ResultDto<T> Failure(string error, HttpStatusCode statusCode) => new() { IsSuccess = false, StatusCode = statusCode, ErrorMessage = error };
    }
}
