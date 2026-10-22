namespace Platform.DDD.Domain.Entities;

/// <summary>
/// Marker: bản ghi persistence thuộc một tenant.
/// Entity Framework gắn query filter cho các type implement interface này.
/// </summary>
public interface ITenantEntity
{
    /// <summary>Tenant sở hữu bản ghi.</summary>
    Guid TenantId { get; set; }
}
