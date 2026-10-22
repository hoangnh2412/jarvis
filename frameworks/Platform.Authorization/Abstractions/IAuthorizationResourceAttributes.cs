namespace Platform.Authorization.Abstractions;

/// <summary>
/// Thuộc tính ABAC trên resource (ADR: mọi field ngoài <c>TenantId</c>/<c>OrgId</c>).
/// Engine đọc attribute qua bag này hoặc qua property CLR cùng tên (v1 — hỗ trợ Data Scope LINQ).
/// </summary>
public interface IAuthorizationResourceAttributes
{
    bool TryGetAuthorizationAttribute(string name, out object? value);
}
