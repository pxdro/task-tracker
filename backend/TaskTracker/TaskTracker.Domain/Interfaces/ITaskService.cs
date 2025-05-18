using TaskTracker.Domain.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Interfaces
{
    public interface ITaskService
    {
        Task<ResultDto<IEnumerable<TaskReturnDto>>> GetAllTasksAsync(Guid userId);
        Task<ResultDto<TaskReturnDto>> GetTaskByIdAsync(Guid taskId, Guid userId);
        Task<ResultDto<TaskReturnDto>> CreateTaskAsync(TaskRequestDto taskRequestDto, Guid userId);
        Task<ResultDto<TaskReturnDto>> UpdateTaskAsync(Guid taskId, TaskRequestDto taskRequestDto, Guid userId);
        Task<ResultDto<TaskReturnDto>> ChangeTaskStatusAsync(Guid taskId, Guid userId, EnumTaskStatus newStatus);
        Task<ResultDto<bool>> DeleteTaskAsync(Guid taskId, Guid userId);
    }
}
