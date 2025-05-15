using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAll(string userEmail, string? field = null, string? value = null);
        Task<TaskItem?> FirstOrDefault(string userEmail, string? field = null, string? value = null);
        Task<TaskItem> Add(string userEmail, TaskItem task);
        Task<TaskItem?> Update(string userEmail, string title, TaskItem newTask);
        Task<TaskItem?> ChangeStatus(string userEmail, string title, EnumTaskStatus status);
        Task<bool> Delete(string userEmail, string title);
    }
}
