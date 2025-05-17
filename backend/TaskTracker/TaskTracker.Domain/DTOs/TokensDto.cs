using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Domain.DTOs
{
    public class TokensDto
    {
        [Required]
        public string? AuthToken { get; set; }
        [Required]
        public string? RefreshToken { get; set; }
    }
}
