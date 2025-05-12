namespace TaskTracker.Domain.DTOs
{
    public class LoginResultDto
    {
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
    }
}
