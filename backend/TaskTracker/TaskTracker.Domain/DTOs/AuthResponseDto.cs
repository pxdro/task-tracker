namespace TaskTracker.Domain.DTOs
{
    public class AuthResponseDto
    {
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
