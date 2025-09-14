using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Queries;

namespace TaskManagerApi.Features.Tasks.Handlers;

public class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskResponseDto?>
{
    private readonly TaskManagerContext _context;

    public GetTaskByIdQueryHandler(TaskManagerContext context)
    {
        _context = context;
    }

    public async Task<TaskResponseDto?> HandleAsync(GetTaskByIdQuery request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (task == null)
        {
            return null;
        }

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