using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities
{
    public class TaskItem : Entity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EnumTaskStatus Status { get; set; }
    }
}
