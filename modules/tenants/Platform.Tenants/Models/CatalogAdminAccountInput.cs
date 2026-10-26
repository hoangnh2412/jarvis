namespace Platform.Tenants.Models;

/// <summary>Tài khoản quản trị bootstrap khi tạo tenant/org từ catalog nền tảng.</summary>
public sealed class CatalogAdminAccountInput
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
}

public sealed class CatalogAdminProvisionResult
{
    public Guid UserId { get; init; }

    public string Username { get; init; } = string.Empty;
}
