namespace Platform.DDD.Domain.Services;

/// <summary>Kiểm tra org thuộc tenant trước khi gắn working scope.</summary>
public interface IOrgTenantScopeValidator
{
    Task ValidateOrgBelongsToTenantAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default);
}
