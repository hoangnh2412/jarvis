using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Platform.DDD.Application.Queries;

public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : IQuery
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.Handle(query);
    }
}