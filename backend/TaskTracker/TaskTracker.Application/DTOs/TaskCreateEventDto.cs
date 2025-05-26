namespace TaskTracker.Application.DTOs
{
    public class TaskCreateEventDto
    {
        public Guid TaskId { get; init; }
        public Guid UserId { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
