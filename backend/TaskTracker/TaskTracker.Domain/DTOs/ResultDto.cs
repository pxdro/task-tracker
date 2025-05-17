namespace TaskTracker.Domain.DTOs
{
    public class ResultDto<T>
    {
        public bool Success { get; private set; }
        public string? Error { get; private set; }
        public T? Data { get; private set; }

        public static ResultDto<T> Ok(T data) => new() { Success = true, Data = data };
        public static ResultDto<T> Fail(string error) => new() { Success = false, Error = error };
    }
}
