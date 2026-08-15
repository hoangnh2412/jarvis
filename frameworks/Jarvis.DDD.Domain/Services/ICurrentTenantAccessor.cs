namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Ambient tenant id cho luồng async hiện tại (HTTP, background job, …).
/// Dùng <see cref="BeginScope"/> để gán/khôi phục. Đọc khi mở connection DB tenant,
/// không lấy từ resolve tenant của <see cref="Repositories.IUnitOfWork"/>.
/// </summary>
public interface ICurrentTenantAccessor
{
    Guid? TenantId { get; }

    /// <summary>
    /// Gán <see cref="TenantId"/> cho async context hiện tại đến khi dispose scope trả về.
    /// </summary>
    IDisposable BeginScope(Guid tenantId);
}
