using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Entities;
using TaskTracker.Application.Enums;
using TaskTracker.Application.Interfaces;
using TaskTracker.Infrastructure.Context;

namespace TaskTracker.Infrastructure.Services
{
    public class TaskService(TaskTrackerDbContext context, IMapper mapper) : ITaskService
    {
        private readonly TaskTrackerDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<ResultDto<IEnumerable<TaskReturnDto>>> GetAllTasksAsync(Guid userId)
        {
            try
            {
                var tasks = await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();
                return ResultDto<IEnumerable<TaskReturnDto>>.Success(
                    _mapper.Map<IEnumerable<TaskReturnDto>>(tasks),
                    HttpStatusCode.OK
                );
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<IEnumerable<TaskReturnDto>>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<IEnumerable<TaskReturnDto>>> GetTasksByFieldValueAsync(string field, string value, Guid userId)
        {
            try
            {
                return (field.Trim().ToLower(), value.ToLower()) switch
                {
                    ("title", var v)
                        => ResultDto<IEnumerable<TaskReturnDto>>.Success(
                            await FilterAsync(t => v == null ? t.Title == null : (t.Title != null && t.Title.ToLower().Contains(v)),
                                userId), HttpStatusCode.OK),
                    ("description", var v)
                        => ResultDto<IEnumerable<TaskReturnDto>>.Success(
                            await FilterAsync(t => v == null ? t.Description == null : (t.Description != null && t.Description.ToLower().Contains(v)),
                                userId), HttpStatusCode.OK),
                    ("status", var v) when Enum.TryParse<EnumTaskStatus>(v, true, out var status)
                        => ResultDto<IEnumerable<TaskReturnDto>>.Success(
                            await FilterAsync(t => t.Status == status, userId), HttpStatusCode.OK),
                    _
                        => ResultDto<IEnumerable<TaskReturnDto>>.Failure(
                            $"Invalid field", HttpStatusCode.BadRequest)
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<IEnumerable<TaskReturnDto>>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<TaskReturnDto>> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                var userOk = task?.UserId == userId;

                if (task == null)
                    return ResultDto<TaskReturnDto>.Failure("Task not found", HttpStatusCode.NotFound);
                else if (!userOk)
                    return ResultDto<TaskReturnDto>.Failure("Access denied", HttpStatusCode.Unauthorized);
                else
                    return ResultDto<TaskReturnDto>.Success(_mapper.Map<TaskReturnDto>(task), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<TaskReturnDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<TaskReturnDto>> CreateTaskAsync(TaskRequestDto taskRequestDto, Guid userId)
        {
            try
            {
                TaskItem task = _mapper.Map<TaskItem>(taskRequestDto);
                task.UserId = userId;
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();

                return ResultDto<TaskReturnDto>.Success(_mapper.Map<TaskReturnDto>(task), HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<TaskReturnDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<TaskReturnDto>> UpdateTaskAsync(Guid taskId, TaskRequestDto taskRequestDto, Guid userId)
        {
            try
            {
                TaskItem? task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                bool userOk = task?.UserId == userId;

                if (task == null)
                    return ResultDto<TaskReturnDto>.Failure("Task not found", HttpStatusCode.NotFound);
                else if (!userOk)
                    return ResultDto<TaskReturnDto>.Failure("Access denied", HttpStatusCode.Unauthorized);

                task.Title = taskRequestDto.Title;
                task.Description = taskRequestDto.Description;
                task.Status = taskRequestDto.Status;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ResultDto<TaskReturnDto>.Success(_mapper.Map<TaskReturnDto>(task), HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<TaskReturnDto>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<ResultDto<bool>> DeleteTaskAsync(Guid taskId, Guid userId)
        {
            try
            {
                TaskItem? task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                bool userOk = task?.UserId == userId;

                if (task == null)
                    return ResultDto<bool>.Failure("Task not found", HttpStatusCode.NotFound);
                else if (!userOk)
                    return ResultDto<bool>.Failure("Access denied", HttpStatusCode.Unauthorized);

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return ResultDto<bool>.Success(true, HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Server error");
                return ResultDto<bool>.Failure(
                    "Server error",
                    HttpStatusCode.InternalServerError
                );
            }
        }

        private async Task<IEnumerable<TaskReturnDto>> FilterAsync(Expression<Func<TaskItem, bool>> predicate, Guid userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .Where(predicate)
                .ToListAsync();
            return _mapper.Map<IEnumerable<TaskReturnDto>>(tasks);
        }
    }
}
