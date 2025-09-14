using System.Reflection;

namespace TaskManagerApi.CQRS;

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for query type {queryType.Name}");
        }

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"HandleAsync method not found on handler for {queryType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { query, cancellationToken });
        
        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {queryType.Name} did not return expected Task<{typeof(TResult).Name}>");
    }

    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command type {commandType.Name}");
        }

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"HandleAsync method not found on handler for {commandType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { command, cancellationToken });
        
        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {commandType.Name} did not return expected Task<{typeof(TResult).Name}>");
    }

    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command type {commandType.Name}");
        }

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"HandleAsync method not found on handler for {commandType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { command, cancellationToken });
        
        if (result is Task task)
        {
            await task;
            return;
        }

        throw new InvalidOperationException($"Handler for {commandType.Name} did not return expected Task");
    }
}