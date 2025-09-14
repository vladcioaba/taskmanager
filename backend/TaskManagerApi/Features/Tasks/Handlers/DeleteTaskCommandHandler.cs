using TaskManagerApi.CQRS;
using TaskManagerApi.Data;
using TaskManagerApi.Features.Tasks.Commands;

namespace TaskManagerApi.Features.Tasks.Handlers;

public class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand, bool>
{
    private readonly TaskManagerContext _context;

    public DeleteTaskCommandHandler(TaskManagerContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(DeleteTaskCommand request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}