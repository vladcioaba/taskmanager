using Microsoft.EntityFrameworkCore;
using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Features.Tasks.Queries;

namespace TaskManagerApi.Features.Tasks.Handlers;

public class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, IEnumerable<TaskResponseDto>>
{
    private readonly TaskManagerContext _context;

    public GetTasksQueryHandler(TaskManagerContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskResponseDto>> HandleAsync(GetTasksQuery request, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks.AsQueryable();

        if (request.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == request.IsCompleted.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
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
            .ToListAsync(cancellationToken);

        return tasks;
    }
}