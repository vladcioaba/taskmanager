namespace TaskManagerApi.CQRS;

public interface IDispatcher
{
    Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken);
    Task DispatchAsync(ICommand command, CancellationToken cancellationToken);
}