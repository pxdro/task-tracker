using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Domain.DTOs
{
    public class UserRequestDto
    {
        [Required][EmailAddress]
        public string Email { get; set; }

        [Required][MinLength(6)]
        public string Password { get; set; }
    }
}
