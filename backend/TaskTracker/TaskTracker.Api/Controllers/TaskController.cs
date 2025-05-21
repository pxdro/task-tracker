using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Enums;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController(ITaskService taskService) : ControllerBase
    {
        private readonly ITaskService _taskService = taskService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.GetAllTasksAsync(userId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetById(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.GetTaskByIdAsync(taskId, userId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.CreateTaskAsync(dto, userId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> Update(Guid taskId, TaskRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.UpdateTaskAsync(taskId, dto, userId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPatch("{taskId}/active")]
        public async Task<IActionResult> MarkAsActive(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.ChangeTaskStatusAsync(taskId, userId, EnumTaskStatus.Active);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPatch("{taskId}/completed")]
        public async Task<IActionResult> MarkAsCompleted(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.ChangeTaskStatusAsync(taskId, userId, EnumTaskStatus.Completed);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> Delete(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var result = await _taskService.DeleteTaskAsync(taskId, userId);
            return StatusCode((int)result.StatusCode, result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("ID do usuário não encontrado nas claims");

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new FormatException("ID do usuário em formato inválido");

            return userId;
        }
    }
}
