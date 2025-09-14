using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Commands;
using TaskManagerApi.Features.Tasks.Queries;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<TasksController> _logger;

        public TasksController(IDispatcher dispatcher, ILogger<TasksController> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks([FromQuery] bool? isCompleted = null, [FromQuery] Priority? priority = null)
        {
            try
            {
                var query = new GetTasksQuery(isCompleted, priority);
                var tasks = await _dispatcher.DispatchAsync(query, CancellationToken.None);
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
                var query = new GetTaskByIdQuery(id);
                var task = await _dispatcher.DispatchAsync(query, CancellationToken.None);

                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }

                return Ok(task);
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

                var command = new CreateTaskCommand(taskCreateDto);
                var taskResponse = await _dispatcher.DispatchAsync(command, CancellationToken.None);

                return CreatedAtAction(nameof(GetTask), new { id = taskResponse.Id }, taskResponse);
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
                var command = new UpdateTaskCommand(id, taskUpdateDto);
                var taskResponse = await _dispatcher.DispatchAsync(command, CancellationToken.None);

                if (taskResponse == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }

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
                var command = new DeleteTaskCommand(id);
                var success = await _dispatcher.DispatchAsync(command, CancellationToken.None);

                if (!success)
                {
                    return NotFound($"Task with ID {id} not found");
                }

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