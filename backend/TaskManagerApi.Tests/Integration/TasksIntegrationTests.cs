using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Tests.Integration;

public class TasksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TasksIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TaskManagerContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a test database with unique name to avoid conflicts
                var dbName = $"TestDatabase_{Guid.NewGuid()}";
                services.AddDbContext<TaskManagerContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Build the service provider and seed the database
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TaskManagerContext>();
                
                // Ensure database is clean
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                
                // Seed test data
                SeedTestData(context);
            });
        });

        _client = _factory.CreateClient();
    }

    private static void SeedTestData(TaskManagerContext context)
    {
        // Clear existing data first
        if (context.Tasks.Any())
        {
            context.Tasks.RemoveRange(context.Tasks);
            context.SaveChanges();
        }

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Integration Test Task 1",
                Description = "Integration Test Description 1",
                IsCompleted = false,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = 2,
                Title = "Integration Test Task 2", 
                Description = "Integration Test Description 2",
                IsCompleted = true,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetTasks_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.NotNull(tasks);
        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task GetTasks_WithCompletedFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks?isCompleted=true");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.NotNull(tasks);
        Assert.Single(tasks);
        Assert.All(tasks, task => Assert.True(task.IsCompleted));
    }

    [Fact]
    public async Task GetTasks_WithPriorityFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks?priority=3"); // Priority.High = 3

        // Assert
        response.EnsureSuccessStatusCode();
        
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        Assert.NotNull(tasks);
        Assert.Single(tasks);
        Assert.All(tasks, task => Assert.Equal(Priority.High, task.Priority));
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/1");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(task);
        Assert.Equal(1, task.Id);
        Assert.Equal("Integration Test Task 1", task.Title);
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskCreateDto
        {
            Title = "New Integration Test Task",
            Description = "New Integration Test Description",
            Priority = Priority.Low
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(createdTask);
        Assert.Equal("New Integration Test Task", createdTask.Title);
        Assert.Equal("New Integration Test Description", createdTask.Description);
        Assert.Equal(Priority.Low, createdTask.Priority);
        Assert.False(createdTask.IsCompleted);
        Assert.Null(createdTask.CompletedAt);
        Assert.True(createdTask.Id > 0);

        // Verify Location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/Tasks/{createdTask.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidTask = new TaskCreateDto
        {
            Title = "", // Invalid: empty title
            Description = "Test Description",
            Priority = Priority.Low
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", invalidTask);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsUpdatedTask()
    {
        // Arrange
        var updateTask = new TaskUpdateDto
        {
            Title = "Updated Integration Test Task",
            Description = "Updated Integration Test Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/1", updateTask);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(1, updatedTask.Id);
        Assert.Equal("Updated Integration Test Task", updatedTask.Title);
        Assert.Equal("Updated Integration Test Description", updatedTask.Description);
        Assert.Equal(Priority.High, updatedTask.Priority);
        Assert.True(updatedTask.IsCompleted);
        Assert.NotNull(updatedTask.CompletedAt);
    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateTask = new TaskUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/999", updateTask);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_MarkingAsIncomplete_ClearsCompletedAt()
    {
        // Arrange
        var updateTask = new TaskUpdateDto
        {
            Title = "Integration Test Task 2",
            Description = "Integration Test Description 2", 
            Priority = Priority.Medium,
            IsCompleted = false // Mark completed task as incomplete
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/tasks/2", updateTask);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(updatedTask);
        Assert.False(updatedTask.IsCompleted);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/1");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify task was deleted
        var getResponse = await _client.GetAsync("/api/tasks/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TasksApi_CorsEnabled_AllowsOptions()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CreateTask_CompleteCrudWorkflow_WorksEndToEnd()
    {
        // Create
        var createTask = new TaskCreateDto
        {
            Title = "CRUD Workflow Task",
            Description = "Testing complete CRUD workflow",
            Priority = Priority.Medium
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createTask);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(createdTask);
        var taskId = createdTask.Id;

        // Read
        var readResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        readResponse.EnsureSuccessStatusCode();
        var readTask = await readResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(readTask);
        Assert.Equal("CRUD Workflow Task", readTask.Title);

        // Update
        var updateTask = new TaskUpdateDto
        {
            Title = "Updated CRUD Workflow Task",
            Description = "Updated description for CRUD workflow",
            Priority = Priority.High,
            IsCompleted = true
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateTask);
        updateResponse.EnsureSuccessStatusCode();
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated CRUD Workflow Task", updatedTask.Title);
        Assert.True(updatedTask.IsCompleted);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }
}