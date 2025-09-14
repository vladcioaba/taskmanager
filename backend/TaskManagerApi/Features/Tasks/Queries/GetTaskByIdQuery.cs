using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Features.Tasks.Queries;

public record GetTaskByIdQuery(int Id) : IQuery<TaskResponseDto?>;