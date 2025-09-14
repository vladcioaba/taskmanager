using TaskManagerApi.CQRS;

namespace TaskManagerApi.Features.Tasks.Commands;

public record DeleteTaskCommand(int Id) : ICommand<bool>;