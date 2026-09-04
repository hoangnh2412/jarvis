namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// End-user related attributes.
/// See https://opentelemetry.io/docs/specs/semconv/registry/enduser/
/// </summary>
public static class UserAttributes
{
    public const string Id = "enduser.id";
    public const string Role = "enduser.role";
    public const string Scope = "enduser.scope";
    public const string UserName = "enduser.username";
    public const string TenantId = "enduser.tenant_id";
}
