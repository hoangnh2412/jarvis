using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Shared.DependencyInjection;

public class ServiceFactory : IServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static IDictionary<string, string> GetRegistration<T>()
    {
        if (!InstanceStorage.Services.TryGetValue(typeof(T).AssemblyQualifiedName, out IDictionary<string, string> registration))
            throw new ArgumentException($"Service {typeof(T).Name} is not registered");

        return registration;
    }

    public T GetByName<T>(string name)
    {
        if (!GetRegistration<T>().TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");

        var type = Type.GetType(implementationType);
        var service = _serviceProvider.GetService(type);
        return (T)service;
    }

    public T GetRequiredByName<T>(string name)
    {
        if (!GetRegistration<T>().TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");

        return (T)_serviceProvider.GetRequiredService(Type.GetType(implementationType));
    }

    public ICollection<string> GetNames<T>()
    {
        return GetRegistration<T>().Keys;
    }
}