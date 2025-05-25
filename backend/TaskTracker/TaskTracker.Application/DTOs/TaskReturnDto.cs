using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskReturnDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EnumTaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
