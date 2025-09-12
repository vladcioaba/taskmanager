using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerApi.Controllers;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Tests.Controllers;

public class TasksControllerTests : IDisposable
{
    private readonly TaskManagerContext _context;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<TaskManagerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskManagerContext(options);
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_context, _mockLogger.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Test Task 1",
                Description = "Test Description 1",
                IsCompleted = false,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = 2,
                Title = "Test Task 2",
                Description = "Test Description 2",
                IsCompleted = true,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddHours(-1)
            },
            new TaskItem
            {
                Id = 3,
                Title = "Test Task 3",
                Description = "Test Description 3",
                IsCompleted = false,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.Tasks.AddRange(tasks);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTasks_ReturnsAllTasks_WhenNoFilterProvided()
    {
        // Act
        var result = await _controller.GetTasks();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskResponseDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Equal(3, tasks.Count());
    }

    [Fact]
    public async Task GetTasks_ReturnsFilteredTasks_WhenCompletedFilterProvided()
    {
        // Act
        var result = await _controller.GetTasks(isCompleted: true);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskResponseDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Single(tasks);
        Assert.All(tasks, task => Assert.True(task.IsCompleted));
    }

    [Fact]
    public async Task GetTasks_ReturnsFilteredTasks_WhenPriorityFilterProvided()
    {
        // Act
        var result = await _controller.GetTasks(priority: Priority.High);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskResponseDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Single(tasks);
        Assert.All(tasks, task => Assert.Equal(Priority.High, task.Priority));
    }

    [Fact]
    public async Task GetTask_ReturnsTask_WhenTaskExists()
    {
        // Act
        var result = await _controller.GetTask(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        Assert.Equal(1, task.Id);
        Assert.Equal("Test Task 1", task.Title);
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _controller.GetTask(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedTask_WhenValidDataProvided()
    {
        // Arrange
        var createDto = new TaskCreateDto
        {
            Title = "New Test Task",
            Description = "New Test Description",
            Priority = Priority.Medium
        };

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var task = Assert.IsType<TaskResponseDto>(createdResult.Value);
        
        Assert.Equal("New Test Task", task.Title);
        Assert.Equal("New Test Description", task.Description);
        Assert.Equal(Priority.Medium, task.Priority);
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAt);
        
        // Verify task was actually created in database
        var createdTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(createdTask);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var createDto = new TaskCreateDto
        {
            Title = "",
            Description = "Test Description",
            Priority = Priority.Low
        };

        // Simulate model validation error
        _controller.ModelState.AddModelError("Title", "The Title field is required.");

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateTask_ReturnsUpdatedTask_WhenValidDataProvided()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var result = await _controller.UpdateTask(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        
        Assert.Equal("Updated Task Title", task.Title);
        Assert.Equal("Updated Description", task.Description);
        Assert.Equal(Priority.High, task.Priority);
        Assert.True(task.IsCompleted);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var result = await _controller.UpdateTask(999, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateTask_SetsCompletedAt_WhenTaskIsMarkedAsCompleted()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Test Task 1",
            Description = "Test Description 1", 
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var result = await _controller.UpdateTask(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        
        Assert.True(task.IsCompleted);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public async Task UpdateTask_ClearsCompletedAt_WhenTaskIsMarkedAsIncomplete()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Test Task 2",
            Description = "Test Description 2",
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var result = await _controller.UpdateTask(2, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
    {
        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        // Verify task was actually deleted
        var deletedTask = await _context.Tasks.FindAsync(1);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateTask_HandlesDbException()
    {
        // Arrange - Dispose the current context and create a new one that will throw an exception
        _context.Dispose();
        
        var createDto = new TaskCreateDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = Priority.Low
        };

        // Create a new controller with a disposed context to simulate a database error
        var newOptions = new DbContextOptionsBuilder<TaskManagerContext>()
            .UseInMemoryDatabase(databaseName: "DisposedDb")
            .Options;
        
        using var disposedContext = new TaskManagerContext(newOptions);
        disposedContext.Dispose();
        
        var controllerWithDisposedContext = new TasksController(disposedContext, _mockLogger.Object);

        // Act
        var result = await controllerWithDisposedContext.CreateTask(createDto);

        // Assert - Should return 500 status code when database error occurs
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred while creating the task", objectResult.Value);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}