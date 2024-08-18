using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Queries;

/// <summary>
/// Defines an asynchronous handler for a query that returns a result.
/// </summary>
/// <typeparam name="TQuery">The type of query to be handled.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the query.</typeparam>
public interface IAsyncQueryHandler<TQuery, TResult> where TQuery : IQuery
{
    /// <summary>
    /// Asynchronously handles the specified query and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the specified type.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}