using Microsoft.EntityFrameworkCore;
using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Commands;
using TaskManagerApi.Features.Tasks.Queries;
using TaskManagerApi.Features.Tasks.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add CQRS services
builder.Services.AddScoped<IDispatcher, Dispatcher>();

// Register query handlers
builder.Services.AddScoped<IQueryHandler<GetTasksQuery, IEnumerable<TaskResponseDto>>, GetTasksQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetTaskByIdQuery, TaskResponseDto?>, GetTaskByIdQueryHandler>();

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreateTaskCommand, TaskResponseDto>, CreateTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTaskCommand, TaskResponseDto?>, UpdateTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteTaskCommand, bool>, DeleteTaskCommandHandler>();

// Add Entity Framework
builder.Services.AddDbContext<TaskManagerContext>(options =>
    options.UseInMemoryDatabase("TaskManagerDb"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskManagerContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReactApp");

// Map controllers
app.MapControllers();

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
