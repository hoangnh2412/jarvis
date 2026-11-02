namespace Platform.Modules.Account.Abstractions;

public sealed record LoginCatalogItem(Guid Id, string Code, string Name);

public interface IPlatformLoginCatalogService
{
    Task<IReadOnlyList<LoginCatalogItem>> ListActiveTenantsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoginCatalogItem>?> ListActiveOrgsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
