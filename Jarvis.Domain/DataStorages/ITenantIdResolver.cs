namespace Jarvis.Domain.DataStorages;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdResolver
{
    string? GetTenantId();

    Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default);
}