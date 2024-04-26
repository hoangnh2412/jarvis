namespace Jarvis.Shared.Options;

public class JWTOption
{
    public bool ValidateAudience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateActor { get; set; }
    public bool ValidateLifetime { get; set; }
    public bool ValidateIssuerSigningKey { get; set; }
    public string SecretKey { get; set; }
}