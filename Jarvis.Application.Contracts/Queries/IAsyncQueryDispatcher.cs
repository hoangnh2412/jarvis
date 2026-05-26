using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Queries;

/// <summary>
/// Defines an interface for asynchronously dispatching queries.
/// </summary>
public interface IAsyncQueryDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a query and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to be dispatched. It must implement the <see cref="IQuery"/> interface.</typeparam>
    /// <typeparam name="TResult">The type of the result that the query returns.</typeparam>
    /// <param name="query">The query to be dispatched.</param>
    /// <param name="cancellationToken">A token that can be used to signal cancellation of the operation (optional).</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of the dispatched query.</returns>
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery;
}