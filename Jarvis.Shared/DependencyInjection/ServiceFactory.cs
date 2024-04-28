using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Shared.DependencyInjection;

public static class ServiceFactory
{
    public static Dictionary<string, IDictionary<string, string>> Collection = new Dictionary<string, IDictionary<string, string>>();
}

public class ServiceFactory<T> : IServiceFactory<T>
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static IDictionary<string, string> GetRegistration()
    {
        if (!ServiceFactory.Collection.TryGetValue(typeof(T).AssemblyQualifiedName, out IDictionary<string, string> registration))
            throw new ArgumentException($"Service {typeof(T).Name} is not registered");

        return registration;
    }

    public T GetByName(string name)
    {
        if (!GetRegistration().TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");

        var type = Type.GetType(implementationType);
        var service = _serviceProvider.GetService(type);
        return (T)service;
    }

    public T GetRequiredByName(string name)
    {
        if (!GetRegistration().TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");

        return (T)_serviceProvider.GetRequiredService(Type.GetType(implementationType));
    }

    public ICollection<string> GetNames()
    {
        return GetRegistration().Keys;
    }
}