namespace Platform.Orgs.Abstractions.Events;

/// <summary>Port publish sau <c>SaveChanges</c> thành công. Mặc định no-op tại host.</summary>
public interface IOrgCatalogEventPublisher
{
    Task PublishAsync(OrgCatalogEventBase @event, CancellationToken cancellationToken = default);
}
