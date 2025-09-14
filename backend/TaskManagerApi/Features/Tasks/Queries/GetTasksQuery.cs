using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Features.Tasks.Queries;

public record GetTasksQuery(bool? IsCompleted, Priority? Priority) : IQuery<IEnumerable<TaskResponseDto>>;