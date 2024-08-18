using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Queries;

/// <summary>
/// Defines an interface for dispatching queries.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to be dispatched. It must implement the <see cref="IQuery"/> interface.</typeparam>
    /// <typeparam name="TResult">The type of the result that the query returns.</typeparam>
    /// <param name="query">The query to be dispatched.</param>
    /// <returns>The result of the dispatched query.</returns>
    TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : IQuery;
}