using System.Threading.Tasks;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    public class TaskServiceTests
    {/*
        [Fact]
        public async Task CreateTaskAsync_ShouldAddTask()
        {
            // Arrange
            var service = new TaskService();
            var task = new TaskItem { Title = "Task", Status = EnumTaskStatus.Active };

            // Act
            var result = await service.CreateTaskAsync(task);

            // Assert
            Assert.True(result);
            var tasks = await service.GetTasksByStatusAsync(EnumTaskStatus.Active);
            Assert.Single(tasks);
            Assert.Equal(task.Id, tasks.First().Id);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldReturnFalse_IfTaskExists()
        {
            var service = new TaskService();
            var task = new TaskItem { Title = "Task", Status = EnumTaskStatus.Active };

            await service.CreateTaskAsync(task);
            var result = await service.CreateTaskAsync(task);

            Assert.False(result);
        }

        [Fact]
        public async Task GetTasksByStatusAsync_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var service = new TaskService();
            var task1 = new TaskItem { Title = "Active", Status = EnumTaskStatus.Active };
            var task2 = new TaskItem { Title = "Completed", Status = EnumTaskStatus.Completed };
            await service.CreateTaskAsync(task1);
            await service.CreateTaskAsync(task2);

            // Act
            var activeTasks = await service.GetTasksByStatusAsync(EnumTaskStatus.Active);
            var completedTasks = await service.GetTasksByStatusAsync(EnumTaskStatus.Completed);

            // Assert
            Assert.Single(activeTasks);
            Assert.Single(completedTasks);
            Assert.Equal(task1.Id, activeTasks.First().Id);
            Assert.Equal(task2.Id, completedTasks.First().Id);
        }

        [Fact]
        public async Task MarkAsCompletedAsync_ShouldUpdateStatus()
        {
            // Arrange
            var service = new TaskService();
            var task = new TaskItem
            {
                Title = "Write report",
                Status = EnumTaskStatus.Active
            };
            await service.CreateTaskAsync(task);

            // Act
            var result = await service.MarkAsCompletedAsync(task.Id);

            // Assert
            Assert.True(result);

            var updated = await service.GetTasksByStatusAsync(EnumTaskStatus.Completed);
            Assert.Single(updated);
            Assert.Equal(task.Id, updated.First().Id);
        }*/
    }
}
