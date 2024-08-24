namespace Jarvis.Authentication.Jwt;

public class AuthenticationJwtOption
{
    public string[] IssuerSigningKeys { get; set; } = [];
    public bool ValidateActor { get; set; }
    public bool ValidateSignatureLast { get; set; }
    public bool ValidateWithLKG { get; set; }
    public bool ValidateTokenReplay { get; set; }
    public bool ValidateAudience { get; set; }
    public string[] ValidAudiences { get; set; } = [];
    public bool ValidateIssuerSigningKey { get; set; }
    public bool ValidateIssuer { get; set; }
    public string[] ValidIssuers { get; set; } = [];
    public int MaxExpireMinutes { get; set; }
}
