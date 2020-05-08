using System;

namespace Infrastructure.Abstractions.ChainOfResponsibilityPattern
{
    public interface IHandler
    {

    }

    public interface IHandler<TInput, TOutput> : IHandler
    {
        TOutput Handle(TInput input, Func<TInput, TOutput> next);
    }
}