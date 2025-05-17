using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Domain.Entities
{
    public class User : Entity
    {
        [Required][EmailAddress][MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
