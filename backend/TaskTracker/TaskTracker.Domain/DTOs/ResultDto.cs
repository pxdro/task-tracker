using System.Net;
using System.Text.Json.Serialization;

namespace TaskTracker.Domain.DTOs
{
    public class ResultDto<T>
    {
        [JsonIgnore] // Ensure not returned
        public bool IsSuccess { get; private set; }

        [JsonIgnore] // Ensure not returned
        public HttpStatusCode StatusCode { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Ensure returns only if not null
        public string? ErrorMessage { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Ensure returns only if not null
        public T? Data { get; private set; }

        public static ResultDto<T> Success(T data, HttpStatusCode statusCode) => new() { IsSuccess = true, StatusCode = statusCode, Data = data };
        public static ResultDto<T> Failure(string error, HttpStatusCode statusCode) => new() { IsSuccess = false, StatusCode = statusCode, ErrorMessage = error };
    }
}
