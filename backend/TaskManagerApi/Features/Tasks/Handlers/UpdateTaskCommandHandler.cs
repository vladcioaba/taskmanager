using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Commands;

namespace TaskManagerApi.Features.Tasks.Handlers;

public class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand, TaskResponseDto?>
{
    private readonly TaskManagerContext _context;

    public UpdateTaskCommandHandler(TaskManagerContext context)
    {
        _context = context;
    }

    public async Task<TaskResponseDto?> HandleAsync(UpdateTaskCommand request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);
        if (task == null)
        {
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.TaskUpdateDto.Title))
        {
            task.Title = request.TaskUpdateDto.Title;
        }

        if (request.TaskUpdateDto.Description != null)
        {
            task.Description = request.TaskUpdateDto.Description;
        }

        if (request.TaskUpdateDto.IsCompleted.HasValue)
        {
            task.IsCompleted = request.TaskUpdateDto.IsCompleted.Value;
            if (task.IsCompleted && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (!task.IsCompleted)
            {
                task.CompletedAt = null;
            }
        }

        if (request.TaskUpdateDto.Priority.HasValue)
        {
            task.Priority = request.TaskUpdateDto.Priority.Value;
        }

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