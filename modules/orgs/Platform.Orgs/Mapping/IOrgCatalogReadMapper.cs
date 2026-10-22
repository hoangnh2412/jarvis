namespace Platform.Orgs.Mapping;

/// <summary>
/// Map entity catalog host (<see cref="IOrgCatalogEntity"/>) sang read model của sản phẩm (DTO có cột mở rộng).
/// </summary>
public interface IOrgCatalogReadMapper<TOrg, TReadModel>
    where TOrg : class, IOrgCatalogEntity
{
    TReadModel Map(TOrg entity);
}
