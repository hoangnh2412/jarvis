namespace Platform.Authentication;

/// <summary>Tra password policy từ storage (tenant, org, user, …).</summary>
public interface IPasswordPolicyProvider
{
    /// <summary>Policy áp dụng cho <paramref name="scope"/>; <c>null</c> khi không có bản ghi (validator sẽ lỗi).</summary>
    Task<PasswordPolicyOptions?> GetPolicyAsync(PasswordPolicyScope scope, CancellationToken cancellationToken = default);
}
