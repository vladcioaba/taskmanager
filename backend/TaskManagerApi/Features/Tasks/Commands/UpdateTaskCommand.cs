using TaskManagerApi.CQRS;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Features.Tasks.Commands;

public record UpdateTaskCommand(int Id, TaskUpdateDto TaskUpdateDto) : ICommand<TaskResponseDto?>;