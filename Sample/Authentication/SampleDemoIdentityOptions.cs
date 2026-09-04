namespace Sample.Authentication;

/// <summary>
/// Danh tính demo gắn vào principal sau khi API key hợp lệ — bind từ <c>Sample:DemoIdentity</c>.
/// </summary>
/// <remarks>
/// <para><b>Khi nào dùng:</b> Sample chưa có user store, nhưng các module như Notifications yêu cầu
/// claim user id dạng <see cref="Guid"/>. Options này cấp một danh tính cố định để chạy demo.</para>
/// </remarks>
public sealed class SampleDemoIdentityOptions
{
    public const string SectionName = "Sample:DemoIdentity";

    /// <summary>User id gắn vào <c>ClaimTypes.NameIdentifier</c>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Tenant id gắn vào claim <c>tenant_id</c>; bỏ trống để chạy single-tenant.</summary>
    public Guid TenantId { get; set; }
}
