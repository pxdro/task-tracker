using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Context;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Domain
{
    public class TaskServiceTests : IDisposable
    {
        private readonly TaskTrackerDbContext _dbContext;
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPublishEndpoint> _publisherMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            // InMemory Db context
            var options = new DbContextOptionsBuilder<TaskTrackerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TaskTrackerDbContext(options);

            // Configs for AutoMapper
            _mapperMock.Setup(x => x.Map<TaskReturnDto>(It.IsAny<TaskItem>()))
                .Returns((TaskItem source) => new TaskReturnDto
                {
                    Id = source.Id,
                    Title = source.Title,
                    Description = source.Description,
                    Status = source.Status,
                    CreatedAt = source.CreatedAt,
                    UpdatedAt = source.UpdatedAt,
                });
            _mapperMock.Setup(x => x.Map<IEnumerable<TaskReturnDto>>(It.IsAny<IEnumerable<TaskItem>>()))
                .Returns((IEnumerable<TaskItem> source) =>
                    source.Select(t => new TaskReturnDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }));
            _mapperMock.Setup(x => x.Map<TaskItem>(It.IsAny<TaskRequestDto>()))
                .Returns((TaskRequestDto source) => new TaskItem
                {
                    Title = source.Title,
                    Description = source.Description,
                });

            // Configs for MassTransit
            _publisherMock = new Mock<IPublishEndpoint>();
            _publisherMock
                .Setup(x => x.Publish(
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask);

            _taskService = new TaskService(_dbContext, _mapperMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task GetAllTasksAsync_HavingTasks_ReturnsAllTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tasks = new List<TaskItem>
            {
                new() { UserId = userId, Title = "Task 1" },
                new() { UserId = userId, Title = "Task 2" }
            };
            await _dbContext.Tasks.AddRangeAsync(tasks);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskService.GetAllTasksAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(2, result.Data?.Count());
        }

        [Fact]
        public async Task GetTasksByFieldValueAsync_HavingTasks_ReturnsFilteredTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var findField = "title";
            var findValue = "Task 1";
            var tasks = new List<TaskItem>
            {
                new() { UserId = userId, Title = findValue, Description = "Description 1" },
                new() { UserId = userId, Title = "Task 2", Description = "Description 2" },
            };
            await _dbContext.Tasks.AddRangeAsync(tasks);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTasksByFieldValueAsync(findField, findValue, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(1, result.Data?.Count());
            Assert.Equal(findValue, result.Data?.First().Title);
        }

        [Fact]
        public async Task GetTasksByFieldValueAsync_NoExistentField_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var findField = "teste";
            var findValue = "teste";

            // Act
            var result = await _taskService.GetTasksByFieldValueAsync(findField, findValue, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Invalid field", result.ErrorMessage);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskExistsAndUserOwner_ReturnsTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskItem { UserId = userId };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTaskByIdAsync(task.Id, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(task.Id, result.Data?.Id);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _taskService.GetTaskByIdAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Equal("Task not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskExistsButNotOwner_ReturnsUnauthorized()
        {
            // Arrange
            var task = new TaskItem { UserId = Guid.NewGuid() };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskService.GetTaskByIdAsync(task.Id, Guid.NewGuid()); // Different user

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Access denied", result.ErrorMessage);
            Assert.NotNull(result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task CreateTaskAsync_ValidData_ReturnsCreatedTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var requestDto = new TaskRequestDto { Title = "New Task", Description = "Desc" };

            // Act
            var result = await _taskService.CreateTaskAsync(requestDto, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(requestDto.Title, result.Data?.Title);
            Assert.Equal(requestDto.Description, result.Data?.Description);
            Assert.Equal(EnumTaskStatus.Active, result.Data?.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_ValidUpdate_UpdatesTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Old Title",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(task).State = EntityState.Detached;

            var updateDto = new TaskRequestDto { Title = "New Title", Description = "New Desc", Status = EnumTaskStatus.Completed };

            // Act
            var result = await _taskService.UpdateTaskAsync(task.Id, updateDto, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Equal("New Title", result.Data?.Title);
            Assert.Equal("New Desc", result.Data?.Description);
            Assert.Equal(EnumTaskStatus.Completed, result.Data?.Status);
            Assert.Equal(task.CreatedAt, result.Data?.CreatedAt);
            Assert.True(result.Data?.UpdatedAt > task.UpdatedAt);
        }

        [Fact]
        public async Task UpdateTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new TaskRequestDto { Title = "New Title", Description = "New Desc" };

            // Act
            var result = await _taskService.UpdateTaskAsync(Guid.NewGuid(), updateDto, Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Equal("Task not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateTaskAsync_TaskExistButNotOwner_ReturnsUnauthorized()
        {
            // Arrange
            var task = new TaskItem { UserId = Guid.NewGuid() };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(task).State = EntityState.Detached;
            var updateDto = new TaskRequestDto { Title = "New Title", Description = "New Desc" };

            // Act
            var result = await _taskService.UpdateTaskAsync(task.Id, updateDto, Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.NotNull(result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Equal("Access denied", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteTaskAsync_ValidTask_DeletesTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = new TaskItem { UserId = userId };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskService.DeleteTaskAsync(task.Id, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.True(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Null(await _dbContext.Tasks.FindAsync(task.Id));
        }

        [Fact]
        public async Task DeleteTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _taskService.DeleteTaskAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.False(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Task not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteTaskAsync_TaskExistButNotOwner_ReturnsUnauthorized()
        {
            // Arrange
            var task = new TaskItem { UserId = Guid.NewGuid() };
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(task).State = EntityState.Detached;

            // Act
            var result = await _taskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.False(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Access denied", result?.ErrorMessage);
        }

        [Fact]
        public async Task AnyMethod_DatabaseError_ReturnsInternalServerError()
        {
            // Arrange
            var invalidDbContext = new TaskTrackerDbContext(
                new DbContextOptionsBuilder<TaskTrackerDbContext>()
                    .UseInMemoryDatabase("InvalidDb")
                    .Options);
            invalidDbContext.Dispose(); // Force database error

            var service = new TaskService(invalidDbContext, _mapperMock.Object, _publisherMock.Object);

            // Act
            var result = await service.GetAllTasksAsync(Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Server error", result.ErrorMessage);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
