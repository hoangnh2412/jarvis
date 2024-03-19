namespace Jarvis.Application.MultiTenancy;

public interface IConnectionStringResolver
{
    Task<string> GetConnectionStringAsync(string name);
}