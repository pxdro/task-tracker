using TaskTracker.Application.DTOs;
using TaskTracker.Application.Enums;

namespace TaskTracker.Application.Interfaces
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
