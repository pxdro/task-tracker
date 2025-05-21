using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Application.DTOs
{
    public class TokensDto
    {
        [Required]
        public string? AuthToken { get; set; }
        [Required]
        public string? RefreshToken { get; set; }
    }
}
