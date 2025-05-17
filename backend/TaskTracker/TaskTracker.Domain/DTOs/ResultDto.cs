namespace TaskTracker.Domain.DTOs
{
    public class ResultDto<T>
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public T? Data { get; private set; }

        public static ResultDto<T> Ok(T data) => new() { IsSuccess = true, Data = data };
        public static ResultDto<T> Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
    }
}
