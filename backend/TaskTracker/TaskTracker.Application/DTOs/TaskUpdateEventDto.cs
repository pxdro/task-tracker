using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskUpdateEventDto
    {
        public Guid TaskId { get; init; }
        public Guid UserId { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public EnumTaskStatus Status { get; init; }
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
