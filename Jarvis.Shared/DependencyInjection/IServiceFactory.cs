namespace Jarvis.Shared.DependencyInjection;

public interface IServiceFactory
{
    T GetByName<T>(string name);

    T GetRequiredByName<T>(string name);

    ICollection<string> GetNames<T>();
}