using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities
{
    public class TaskItem : Entity
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public EnumTaskStatus Status { get; set; }
        public Guid UserId { get; set; }

        [JsonIgnore] // Ensure not returns, only for EF relation
        public User? User { get; set; }
    }
}
