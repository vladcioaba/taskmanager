using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Commands;
using TaskManagerApi.Models;

namespace TaskManagerApi.Features.Tasks.Handlers;

public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskResponseDto>
{
    private readonly TaskManagerContext _context;

    public CreateTaskCommandHandler(TaskManagerContext context)
    {
        _context = context;
    }

    public async Task<TaskResponseDto> HandleAsync(CreateTaskCommand request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Title = request.TaskCreateDto.Title,
            Description = request.TaskCreateDto.Description,
            Priority = request.TaskCreateDto.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            Priority = task.Priority
        };
    }
}