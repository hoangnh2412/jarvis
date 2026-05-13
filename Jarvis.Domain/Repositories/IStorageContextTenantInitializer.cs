namespace Jarvis.Domain.Repositories;

/// <summary>
/// Applies current tenant to <see cref="IStorageContext"/> after the EF Core <c>IDbContextFactory</c> creates the context (typically scoped per request / unit of work).
/// </summary>
public interface IStorageContextTenantInitializer
{
    void Initialize(IStorageContext context);

    ValueTask InitializeAsync(IStorageContext context, CancellationToken cancellationToken = default);
}
