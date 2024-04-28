using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Shared.DependencyInjection;

public class ServiceFactoryBuilder<T>
{
    private readonly IServiceCollection _services;

    public ServiceFactoryBuilder(
        IServiceCollection services)
    {
        _services = services;
    }

    public ServiceFactoryBuilder<T> Add(string name, Type implementType, ServiceLifetime lifetime)
    {
        // _services.Add(new ServiceDescriptor(typeof(T), implementType, lifetime));
        _services.Add(new ServiceDescriptor(implementType, implementType, lifetime));

        GetRegistration().Add(name, implementType.AssemblyQualifiedName);
        return this;
    }

    public ServiceFactoryBuilder<T> Add(Type implementType, ServiceLifetime lifetime)
    {
        return Add(implementType.Name, implementType, lifetime);
    }

    public ServiceFactoryBuilder<T> Add<TImplement>(string name, ServiceLifetime lifetime)
    {
        return Add(name, typeof(TImplement), lifetime);
    }

    public ServiceFactoryBuilder<T> Add<TImplement>(ServiceLifetime lifetime)
    {
        return Add(typeof(TImplement).Name, typeof(TImplement), lifetime);
    }


    public ServiceFactoryBuilder<T> AddSingleton(string name, Type implementType)
    {
        return Add(name, implementType, ServiceLifetime.Singleton);
    }

    public ServiceFactoryBuilder<T> AddSingleton<TImplement>(string name)
    {
        return Add(name, typeof(TImplement), ServiceLifetime.Singleton);
    }

    public ServiceFactoryBuilder<T> AddSingleton(Type implementType)
    {
        return Add(implementType.Name, implementType, ServiceLifetime.Singleton);
    }

    public ServiceFactoryBuilder<T> AddSingleton<TImplement>()
    {
        return Add(typeof(TImplement).Name, typeof(TImplement), ServiceLifetime.Singleton);
    }


    public ServiceFactoryBuilder<T> AddScoped(string name, Type implementType)
    {
        return Add(name, implementType, ServiceLifetime.Scoped);
    }

    public ServiceFactoryBuilder<T> AddScoped<TImplement>(string name)
    {
        return Add(name, typeof(TImplement), ServiceLifetime.Scoped);
    }

    public ServiceFactoryBuilder<T> AddScoped(Type implementType)
    {
        return Add(implementType.Name, implementType, ServiceLifetime.Scoped);
    }

    public ServiceFactoryBuilder<T> AddScoped<TImplement>()
    {
        return Add(typeof(TImplement).Name, typeof(TImplement), ServiceLifetime.Scoped);
    }


    public ServiceFactoryBuilder<T> AddTransient(string name, Type implementType)
    {
        return Add(name, implementType, ServiceLifetime.Transient);
    }

    public ServiceFactoryBuilder<T> AddTransient<TImplement>(string name)
    {
        return Add(name, typeof(TImplement), ServiceLifetime.Transient);
    }

    public ServiceFactoryBuilder<T> AddTransient(Type implementType)
    {
        return Add(implementType.Name, implementType, ServiceLifetime.Transient);
    }

    public ServiceFactoryBuilder<T> AddTransient<TImplement>()
    {
        return Add(typeof(TImplement).Name, typeof(TImplement), ServiceLifetime.Transient);
    }


    private static IDictionary<string, string> GetRegistration()
    {
        if (!ServiceFactory.Collection.TryGetValue(typeof(T).AssemblyQualifiedName, out IDictionary<string, string> registration))
            ServiceFactory.Collection[typeof(T).AssemblyQualifiedName] = new Dictionary<string, string>();

        return ServiceFactory.Collection[typeof(T).AssemblyQualifiedName];
    }
}