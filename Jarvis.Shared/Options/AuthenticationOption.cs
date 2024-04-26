namespace Jarvis.Shared.Options;

public class AuthenticationOption
{
    public string[] ApiKeys { get; set; }
    public JWTOption JWT { get; set; }
    public CognitoOption Cognito { get; set; }
}