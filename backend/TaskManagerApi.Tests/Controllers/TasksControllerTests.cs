using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerApi.Controllers;
using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockDispatcher = new Mock<IDispatcher>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_mockDispatcher.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTasks_ReturnsAllTasks_WhenNoFiltersProvided()
    {
        // Arrange
        var expectedTasks = new List<TaskResponseDto>
        {
            new() { Id = 1, Title = "Task 1", Description = "Description 1", Priority = Priority.High, IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Task 2", Description = "Description 2", Priority = Priority.Medium, IsCompleted = true, CreatedAt = DateTime.UtcNow }
        };

        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IQuery<IEnumerable<TaskResponseDto>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedTasks);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskResponseDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(okResult.Value);
        Assert.Equal(2, tasks.Count());

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<IQuery<IEnumerable<TaskResponseDto>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTask_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        var expectedTask = new TaskResponseDto
        {
            Id = 1, 
            Title = "Test Task", 
            Description = "Test Description", 
            Priority = Priority.High, 
            IsCompleted = false, 
            CreatedAt = DateTime.UtcNow
        };

        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IQuery<TaskResponseDto?>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedTask);

        // Act
        var result = await _controller.GetTask(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        Assert.Equal(1, task.Id);
        Assert.Equal("Test Task", task.Title);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<IQuery<TaskResponseDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<IQuery<TaskResponseDto?>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TaskResponseDto?)null);

        // Act
        var result = await _controller.GetTask(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<IQuery<TaskResponseDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedTask()
    {
        // Arrange
        var createDto = new TaskCreateDto
        {
            Title = "New Task",
            Description = "New Description",
            Priority = Priority.Medium
        };

        var createdTask = new TaskResponseDto
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            Priority = Priority.Medium,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var task = Assert.IsType<TaskResponseDto>(createdResult.Value);
        Assert.Equal("New Task", task.Title);
        Assert.Equal(Priority.Medium, task.Priority);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_ReturnsUpdatedTask()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        var updatedTask = new TaskResponseDto
        {
            Id = 1,
            Title = "Updated Task",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto?>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.UpdateTask(1, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var task = Assert.IsType<TaskResponseDto>(okResult.Value);
        Assert.Equal("Updated Task", task.Title);
        Assert.True(task.IsCompleted);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = false
        };

        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto?>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TaskResponseDto?)null);

        // Act
        var result = await _controller.UpdateTask(999, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TaskResponseDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<ICommand<TaskResponseDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<ICommand<bool>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<ICommand<bool>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockDispatcher.Setup(x => x.DispatchAsync(It.IsAny<ICommand<bool>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Task with ID 999 not found", notFoundResult.Value);

        _mockDispatcher.Verify(x => x.DispatchAsync(It.IsAny<ICommand<bool>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}