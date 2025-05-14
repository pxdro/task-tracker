using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAll(string userEmail);
        Task<TaskItem?> GetByTitle(string userEmail, string title);
        Task<List<TaskItem>> Where(string userEmail, string field, string? value);
        Task<TaskItem> Add(string userEmail, TaskItem task);
        Task<TaskItem?> Update(string userEmail, string title, TaskItem newTask);
        Task<bool> Delete(string userEmail, string title);
    }
}
