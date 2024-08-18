using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Queries;

/// <summary>
/// Defines a handler for a query that returns a result.
/// </summary>
/// <typeparam name="TQuery">The type of query to be handled.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the query.</typeparam>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery
{
    /// <summary>
    /// Handles the specified query and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <returns>The result of handling the query.</returns>
    TResult Handle(TQuery query);
}