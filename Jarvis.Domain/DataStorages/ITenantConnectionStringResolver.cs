namespace Jarvis.Domain.DataStorages;

public interface ITenantConnectionStringResolver
{
    string Resolve(string? name = null);

    Task<string> ResolveAsync(string? name = null);
}