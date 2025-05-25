using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskRequestDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public EnumTaskStatus Status { get; set; }
    }
}
