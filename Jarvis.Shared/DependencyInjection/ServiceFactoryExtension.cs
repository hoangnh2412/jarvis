using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Shared.DependencyInjection;

public static class ServiceFactoryExtension
{
    public static ServiceFactoryBuilder<T> AddByName<T>(this IServiceCollection services)
    {
        return new ServiceFactoryBuilder<T>(services);
    }

    public static TService GetService<TService>(this IServiceProvider provider, string name)
    {
        var factory = provider.GetService<IServiceFactory<TService>>();
        if (factory == null)
            throw new InvalidOperationException($"The factory {typeof(IServiceFactory<TService>)} is not registered. Please use {nameof(ServiceFactoryExtension)}.{nameof(AddByName)}() to register names.");

        return factory.GetByName(name);
    }

    public static TService GetRequiredService<TService>(this IServiceProvider provider, string name)
    {
        var factory = provider.GetService<IServiceFactory<TService>>();
        if (factory == null)
            throw new InvalidOperationException($"The factory {typeof(IServiceFactory<TService>)} is not registered. Please use {nameof(ServiceFactoryExtension)}.{nameof(AddByName)}() to register names.");

        return factory.GetRequiredByName(name);
    }
}