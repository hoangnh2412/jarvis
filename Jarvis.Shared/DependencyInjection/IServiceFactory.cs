namespace Jarvis.Shared.DependencyInjection;

public interface IServiceFactory<T>
{
    T GetByName(string name);

    T GetRequiredByName(string name);

    ICollection<string> GetNames();
}