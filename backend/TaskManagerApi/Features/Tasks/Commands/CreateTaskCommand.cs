using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Features.Tasks.Commands;

public record CreateTaskCommand(TaskCreateDto TaskCreateDto) : ICommand<TaskResponseDto>;