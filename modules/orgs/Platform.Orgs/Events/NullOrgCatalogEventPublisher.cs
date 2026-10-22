using Platform.Orgs.Abstractions.Events;

namespace Platform.Orgs.Events;

/// <summary>No-op mặc định khi host chưa đăng ký bus/outbox.</summary>
public sealed class NullOrgCatalogEventPublisher : IOrgCatalogEventPublisher
{
    public Task PublishAsync(OrgCatalogEventBase @event, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
