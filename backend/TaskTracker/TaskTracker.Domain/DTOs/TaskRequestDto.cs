using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.DTOs
{
    public class TaskRequestDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
