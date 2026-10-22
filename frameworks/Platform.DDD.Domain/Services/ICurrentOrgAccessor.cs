namespace Platform.DDD.Domain.Services;

/// <summary>
/// Ambient working org id cho luồng async hiện tại. Dùng <see cref="BeginScope"/> để gán/khôi phục.
/// </summary>
public interface ICurrentOrgAccessor
{
    Guid? OrgId { get; }

    IDisposable BeginScope(Guid orgId);
}
