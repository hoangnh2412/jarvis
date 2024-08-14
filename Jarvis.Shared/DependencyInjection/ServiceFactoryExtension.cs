using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Shared.DependencyInjection;

public static class ServiceFactoryExtension
{
    public static IServiceCollection Add(this IServiceCollection services, string name, Type serviceType, Type implementType, ServiceLifetime lifetime)
    {
        services.Add(new ServiceDescriptor(implementType, implementType, lifetime));
        GetRegistration(serviceType).Add(name, implementType.AssemblyQualifiedName);
        return services;
    }

    public static IServiceCollection Add<TService, TImplement>(this IServiceCollection services, string name, ServiceLifetime lifetime)
    {
        return services.Add(name, typeof(TService), typeof(TImplement), lifetime);
    }

    public static IServiceCollection AddByName(this IServiceCollection services, Type serviceType, Type implementType, ServiceLifetime lifetime)
    {
        return services.Add(implementType.Name, serviceType, implementType, lifetime);
    }

    public static IServiceCollection AddByName<TService, TImplement>(this IServiceCollection services, ServiceLifetime lifetime)
    {
        return services.Add(typeof(TImplement).Name, typeof(TService), typeof(TImplement), lifetime);
    }


    public static IServiceCollection AddSingleton(this IServiceCollection services, string name, Type serviceType, Type implementType)
    {
        return services.Add(name, serviceType, implementType, ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddSingleton<TService, TImplement>(this IServiceCollection services, string name)
    {
        return services.Add(name, typeof(TService), typeof(TImplement), ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddSingletonByName(this IServiceCollection services, Type serviceType, Type implementType)
    {
        return services.Add(implementType.Name, serviceType, implementType, ServiceLifetime.Singleton);
    }

    public static IServiceCollection AddSingletonByName<TService, TImplement>(this IServiceCollection services)
    {
        return services.Add(typeof(TImplement).Name, typeof(TService), typeof(TImplement), ServiceLifetime.Singleton);
    }


    public static IServiceCollection AddScoped(this IServiceCollection services, string name, Type serviceType, Type implementType)
    {
        return services.Add(name, serviceType, implementType, ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddScoped<TService, TImplement>(this IServiceCollection services, string name)
    {
        return services.Add(name, typeof(TService), typeof(TImplement), ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddScopedByName(this IServiceCollection services, Type serviceType, Type implementType)
    {
        return services.Add(implementType.Name, serviceType, implementType, ServiceLifetime.Scoped);
    }

    public static IServiceCollection AddScopedByName<TService, TImplement>(this IServiceCollection services)
    {
        return services.Add(typeof(TImplement).Name, typeof(TService), typeof(TImplement), ServiceLifetime.Scoped);
    }


    public static IServiceCollection AddTransient(this IServiceCollection services, string name, Type serviceType, Type implementType)
    {
        return services.Add(name, serviceType, implementType, ServiceLifetime.Transient);
    }

    public static IServiceCollection AddTransient<TService, TImplement>(this IServiceCollection services, string name)
    {
        return services.Add(name, typeof(TService), typeof(TImplement), ServiceLifetime.Transient);
    }

    public static IServiceCollection AddTransientByName(this IServiceCollection services, Type serviceType, Type implementType)
    {
        return services.Add(implementType.Name, serviceType, implementType, ServiceLifetime.Transient);
    }

    public static IServiceCollection AddTransientByName<TService, TImplement>(this IServiceCollection services)
    {
        return services.Add(typeof(TImplement).Name, typeof(TService), typeof(TImplement), ServiceLifetime.Transient);
    }





    public static TService GetService<TService>(this IServiceProvider provider, string name)
    {
        var factory = provider.GetService<IServiceFactory>();
        if (factory == null)
            throw new InvalidOperationException($"The factory {typeof(IServiceFactory)} is not registered. Please use {nameof(ServiceFactoryExtension)}.{nameof(AddByName)}() to register names.");

        return factory.GetByName<TService>(name);
    }

    public static TService GetRequiredService<TService>(this IServiceProvider provider, string name)
    {
        var factory = provider.GetService<IServiceFactory>();
        if (factory == null)
            throw new InvalidOperationException($"The factory {typeof(IServiceFactory)} is not registered. Please use {nameof(ServiceFactoryExtension)}.{nameof(AddByName)}() to register names.");

        return factory.GetRequiredByName<TService>(name);
    }

    private static IDictionary<string, string> GetRegistration(Type type)
    {
        if (!InstanceStorage.Services.ContainsKey(type.AssemblyQualifiedName))
            InstanceStorage.Services[type.AssemblyQualifiedName] = new Dictionary<string, string>();

        return InstanceStorage.Services[type.AssemblyQualifiedName];
    }
}