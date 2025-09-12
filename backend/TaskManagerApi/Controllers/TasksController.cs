using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagerContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskManagerContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks([FromQuery] bool? isCompleted = null, [FromQuery] Priority? priority = null)
        {
            try
            {
                var query = _context.Tasks.AsQueryable();

                if (isCompleted.HasValue)
                {
                    query = query.Where(t => t.IsCompleted == isCompleted.Value);
                }

                if (priority.HasValue)
                {
                    query = query.Where(t => t.Priority == priority.Value);
                }

                var tasks = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new TaskResponseDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.IsCompleted,
                        CreatedAt = t.CreatedAt,
                        CompletedAt = t.CompletedAt,
                        Priority = t.Priority
                    })
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }

                var taskResponse = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    CompletedAt = task.CompletedAt,
                    Priority = task.Priority
                };

                return Ok(taskResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task");
            }
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto taskCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = new TaskItem
                {
                    Title = taskCreateDto.Title,
                    Description = taskCreateDto.Description,
                    Priority = taskCreateDto.Priority,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                var taskResponse = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    CompletedAt = task.CompletedAt,
                    Priority = task.Priority
                };

                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "An error occurred while creating the task");
            }
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, TaskUpdateDto taskUpdateDto)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(taskUpdateDto.Title))
                {
                    task.Title = taskUpdateDto.Title;
                }

                if (taskUpdateDto.Description != null)
                {
                    task.Description = taskUpdateDto.Description;
                }

                if (taskUpdateDto.IsCompleted.HasValue)
                {
                    task.IsCompleted = taskUpdateDto.IsCompleted.Value;
                    if (task.IsCompleted && task.CompletedAt == null)
                    {
                        task.CompletedAt = DateTime.UtcNow;
                    }
                    else if (!task.IsCompleted)
                    {
                        task.CompletedAt = null;
                    }
                }

                if (taskUpdateDto.Priority.HasValue)
                {
                    task.Priority = taskUpdateDto.Priority.Value;
                }

                await _context.SaveChangesAsync();

                var taskResponse = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    CompletedAt = task.CompletedAt,
                    Priority = task.Priority
                };

                return Ok(taskResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task");
            }
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }
    }
}