namespace Jarvis.Authentication.Jwt;

public class AuthenticationJwtOption
{
    public string? Authority { get; set; }

    public string? Audience { get; set; }

    public bool? RequireHttpsMetadata { get; set; }

    public string[] IssuerSigningKeys { get; set; } = [];

    public bool ValidateActor { get; set; }

    public bool ValidateSignatureLast { get; set; } = true;

    public bool ValidateWithLKG { get; set; }

    public bool ValidateTokenReplay { get; set; }

    public bool ValidateAudience { get; set; } = true;

    public string[] ValidAudiences { get; set; } = [];

    public bool ValidateIssuerSigningKey { get; set; } = true;

    public bool ValidateIssuer { get; set; }

    public string[] ValidIssuers { get; set; } = [];

    public int MaxExpireMinutes { get; set; }
}
