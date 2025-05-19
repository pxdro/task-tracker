using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    /*public class TaskServiceTests
    {
        private readonly TaskService _service = new();
        private const string User = "user@example.com";

        private static TaskItem CreateTask(string title, string description, EnumTaskStatus status) =>
            new() { Title = title, Description = description, Status = status };

        [Fact]
        public async Task Add_Should_Add_Task()
        {
            // Arrange
            var task = CreateTask("Test", "Desc", EnumTaskStatus.Active);

            // Act
            var result = await _service.Add(User, task);
            var all = await _service.GetAll(User);

            // Assert
            Assert.Single(all);
            Assert.Equal(task.Title, result.Title);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_User_Tasks()
        {
            // Arrange
            await _service.Add(User, CreateTask("One", "A", EnumTaskStatus.Active));
            await _service.Add(User, CreateTask("Two", "B", EnumTaskStatus.Completed));

            // Act
            var tasks = await _service.GetAll(User);

            // Assert
            Assert.Equal(2, tasks.Count);
        }

        [Theory]
        [InlineData("title", "write", 1)]
        [InlineData("description", "home", 2)]
        [InlineData("status", "Completed", 2)]
        public async Task GetAll_Should_Filter_Tasks(string field, string value, int expected)
        {
            // Arrange
            await _service.Add(User, CreateTask("Write report", "Work", EnumTaskStatus.Active));
            await _service.Add(User, CreateTask("Buy groceries", "For home", EnumTaskStatus.Completed));
            await _service.Add(User, CreateTask("Dump the thrash", "For home", EnumTaskStatus.Completed));

            // Act
            var filtered = await _service.GetAll(User, field, value);

            // Assert
            Assert.Equal(expected, filtered.Count);
        }

        [Fact]
        public async Task FirstOrDefault_Should_Return_Task()
        {
            // Arrange
            var task = CreateTask("Read book", "Study", EnumTaskStatus.Active);
            await _service.Add(User, task);

            // Act
            var result = await _service.FirstOrDefault(User, "title", "read book");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.Title, result?.Title);
        }

        [Fact]
        public async Task Update_Should_Modify_Existing_Task()
        {
            // Arrange
            await _service.Add(User, CreateTask("Initial", "Old", EnumTaskStatus.Active));
            var updated = CreateTask("Updated", "New", EnumTaskStatus.Completed);

            // Act
            var result = await _service.Update(User, "initial", updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result?.Title);
            Assert.Equal("New", result?.Description);
            Assert.Equal(EnumTaskStatus.Completed, result?.Status);
        }

        [Fact]
        public async Task ChangeStatus_Should_Update_Task_Status()
        {
            // Arrange
            await _service.Add(User, CreateTask("Initial", "Old", EnumTaskStatus.Active));

            // Act
            var result = await _service.ChangeStatus(User, "initial", EnumTaskStatus.Completed);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Initial", result?.Title);
            Assert.Equal("Old", result?.Description);
            Assert.Equal(EnumTaskStatus.Completed, result?.Status);
        }

        [Fact]
        public async Task Delete_Should_Remove_Task()
        {
            // Arrange
            await _service.Add(User, CreateTask("Delete me", "Trash", EnumTaskStatus.Active));

            // Act
            var deleted = await _service.Delete(User, "delete me");
            var all = await _service.GetAll(User);

            // Assert
            Assert.True(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public async Task Delete_Nonexistent_Task_Should_Return_False()
        {
            // Act
            var result = await _service.Delete(User, "nonexistent");

            // Assert
            Assert.False(result);
        }
    }*/
}
