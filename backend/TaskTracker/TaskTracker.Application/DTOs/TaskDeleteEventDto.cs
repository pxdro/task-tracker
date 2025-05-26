using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskDeleteEventDto
    {
        public Guid TaskId { get; init; }
        public Guid UserId { get; init; }
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
