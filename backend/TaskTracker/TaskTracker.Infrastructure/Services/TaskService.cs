using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly Dictionary<string, List<TaskItem>> _db = [];

        public Task<List<TaskItem>> GetAll(string userEmail, string? field = null, string? value = null)
        {
            if (!_db.TryGetValue(userEmail, out List<TaskItem>? tasks))
                return Task.FromResult(new List<TaskItem>());

            _ = Enum.TryParse(value, out EnumTaskStatus valueEnum);
            return field?.ToLower() switch
            {
                null => Task.FromResult(tasks),
                "title" => Task.FromResult(
                    value == null ? tasks.Where(t => t.Title == null).ToList()
                        : tasks.Where(t => t.Title != null && t.Title.Contains(value, StringComparison.CurrentCultureIgnoreCase)).ToList()),
                "description" => Task.FromResult(
                    value == null ? tasks.Where(t => t.Description == null).ToList()
                        : tasks.Where(t => t.Description != null && t.Description.Contains(value, StringComparison.CurrentCultureIgnoreCase)).ToList()),
                "status" => Task.FromResult(tasks.Where(t => t.Status == valueEnum).ToList()),
                _ => Task.FromResult(new List<TaskItem>())
            };
        }

        public Task<TaskItem?> FirstOrDefault(string userEmail, string? field, string? value)
        {
            if (!_db.TryGetValue(userEmail, out List<TaskItem>? tasks))
                return Task.FromResult(default(TaskItem?));

            _ = Enum.TryParse(value, out EnumTaskStatus valueEnum);
            return field.ToLower() switch
            {
                "title" => Task.FromResult(
                    value == null ? tasks.FirstOrDefault(t => t.Title == null)
                        : tasks.FirstOrDefault(t => t.Title != null && t.Title.Contains(value, StringComparison.CurrentCultureIgnoreCase))),
                "description" => Task.FromResult(
                    value == null ? tasks.FirstOrDefault(t => t.Description == null)
                        : tasks.FirstOrDefault(t => t.Description != null && t.Description.Contains(value, StringComparison.CurrentCultureIgnoreCase))),
                "status" => Task.FromResult(tasks.FirstOrDefault(t => t.Status == valueEnum)),
                _ => Task.FromResult(default(TaskItem?))
            };
        }

        public Task<TaskItem> Add(string userEmail, TaskItem task)
        {
            if (!_db.ContainsKey(userEmail))
                _db[userEmail] = [];

            _db[userEmail].Add(task);
            return Task.FromResult(task);
        }

        public Task<TaskItem?> Update(string userEmail, string title, TaskItem newTask)
        {
            var task = FirstOrDefault(userEmail, "title", title)?.Result;
            if (task == null) return Task.FromResult(task);

            task.Title = newTask.Title;
            task.Description = newTask.Description;
            task.Status = newTask.Status;

            return Task.FromResult(task);
        }

        public Task<TaskItem?> ChangeStatus(string userEmail, string title, EnumTaskStatus status)
        {
            var task = FirstOrDefault(userEmail, "title", title)?.Result;
            if (task == null) return Task.FromResult(task);

            task.Status = status;

            return Task.FromResult(task);
        }

        public Task<bool> Delete(string userEmail, string title)
        {
            var task = FirstOrDefault(userEmail, "title", title)?.Result;
            if (task == null) return Task.FromResult(false);

            _db[userEmail].Remove(task);
            return Task.FromResult(true);
        }
        
        public void Reset() => _db.Clear();

    }
}
