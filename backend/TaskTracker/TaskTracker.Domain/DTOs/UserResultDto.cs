namespace TaskTracker.Domain.DTOs
{
    public class UserResultDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
