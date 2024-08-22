namespace Jarvis.OpenTelemetry.SemanticConvention;

/// <summary>
/// Semantic convention attributes in user. <br />
/// https://opentelemetry.io/docs/specs/semconv/attributes-registry/enduser/
/// </summary>
public class UserAttributes
{
    /// <summary>
    /// Username or client_id extracted from the access token or Authorization header in the inbound request from outside the system. <br />
    /// Ex: username
    /// </summary>
    public const string Id = "enduser.id";

    /// <summary>
    /// Actual/assumed role the client is making the request under extracted from token or application security context. <br />
    /// Ex: orgadmin, member
    /// </summary>
    public const string Role = "enduser.role";

    /// <summary>
    /// Scopes or granted authorities the client currently possesses extracted from token or application security context. The value would come from the scope associated with an OAuth 2.0 Access Token or an attribute value in a SAML 2.0 Assertion. <br />
    /// Ex: read:message, write:files
    /// </summary>
    public const string Scope = "enduser.scope";
}