using Elsa.Common.Multitenancy;
using Platform.DDD.Domain.Services;

namespace Platform.Workflow.Multitenancy;

/// <summary>
/// Adapter giữa tenant context của Platform và tenant context mà Elsa persistence yêu cầu.
/// Platform là nguồn tenant duy nhất; Elsa chỉ nhận tenant đã được chuẩn hóa từ Platform.
/// </summary>
public sealed class PlatformTenantAccessor(ICurrentTenantAccessor currentTenantAccessor) : ITenantAccessor
{
    public string? TenantId => currentTenantAccessor.TenantId?.ToString("D");

    public Tenant? Tenant => TenantId is { } tenantId
        ? new Tenant { Id = tenantId, Name = tenantId }
        : null;

    public IDisposable PushContext(Tenant? tenant)
    {
        if (tenant?.Id is not { } tenantId || !Guid.TryParse(tenantId, out var parsedTenantId))
            return NoopScope.Instance;

        return currentTenantAccessor.BeginScope(parsedTenantId);
    }

    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();

        public void Dispose()
        {
        }
    }
}
