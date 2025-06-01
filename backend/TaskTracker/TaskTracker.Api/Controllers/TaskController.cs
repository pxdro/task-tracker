using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController(ITaskService taskService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly ITaskService _taskService = taskService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation($"GET {nameof(GetAll)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.GetAllTasksAsync(userId);
            LogResult<IEnumerable<TaskReturnDto>>("GET", nameof(GetAll), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetByFilter(string field, string value)
        {
            _logger.LogInformation($"GET {nameof(GetByFilter)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.GetTasksByFieldValueAsync(field, value, userId);
            LogResult<IEnumerable<TaskReturnDto>>("GET", nameof(GetByFilter), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetById(Guid taskId)
        {
            _logger.LogInformation($"GET {nameof(GetById)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.GetTaskByIdAsync(taskId, userId);
            LogResult<TaskReturnDto>("GET", nameof(GetById), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskRequestDto dto)
        {
            _logger.LogInformation($"POST {nameof(Create)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.CreateTaskAsync(dto, userId);
            LogResult<TaskReturnDto>("POST", nameof(Create), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> Update(Guid taskId, TaskRequestDto dto)
        {
            _logger.LogInformation($"PUT {nameof(Update)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.UpdateTaskAsync(taskId, dto, userId);
            LogResult<TaskReturnDto>("PUT", nameof(Update), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> Delete(Guid taskId)
        {
            _logger.LogInformation($"DELETE {nameof(Delete)} called");
            var userId = GetCurrentUserId();
            var result = await _taskService.DeleteTaskAsync(taskId, userId);
            LogResult<bool>("DELETE", nameof(Delete), result);
            return StatusCode((int)result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("smoke")]
        public async Task<IActionResult> SmokeTest()
        {
            _logger.LogInformation($"POST {nameof(SmokeTest)} called");
            var userId = GetCurrentUserId();
            var dto = new TaskRequestDto
            {
                Title = "Smoke Test",
                Description = "RabbitMQ publish test",
            };
            var result = await _taskService.CreateTaskAsync(dto, userId);
            LogResult<TaskReturnDto>("POST", nameof(SmokeTest), result);
            return StatusCode((int)result.StatusCode, result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID not found in claims");
                throw new UnauthorizedAccessException("User ID not found in claims");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid User ID");
                throw new FormatException("Invalid User ID");
            }

            return userId;
        }

        private void LogResult<T>(string method, string action, ResultDto<T> result)
        {
            if (result.StatusCode == HttpStatusCode.InternalServerError)
                _logger.LogError("{Method} {Action} internal error with message {ErrorMessage}", method, action, result.ErrorMessage);
            else if (result.IsSuccess)
                _logger.LogInformation("{Method} {Action} succeeded with code {StatusCode} and data {@Data}", method, action, (int)result.StatusCode, result.Data);
            else
                _logger.LogInformation("{Method} {Action} failed with code {StatusCode} and message {ErrorMessage}", method, action, (int)result.StatusCode, result.ErrorMessage);
        }
    }
}
