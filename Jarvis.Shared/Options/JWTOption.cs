namespace Jarvis.Shared.Options;

public class JWTOption
{
    public Dictionary<string, JWTSchemaOption> Schemas { get; set; }

    public class JWTSchemaOption
    {
        public bool ValidateAudience { get; set; } = false;
        public bool ValidateIssuer { get; set; } = false;
        public bool ValidateActor { get; set; } = false;
        public bool ValidateLifetime { get; set; } = true;
        public bool ValidateIssuerSigningKey { get; set; } = true;
        public string SecretKey { get; set; }
    }
}