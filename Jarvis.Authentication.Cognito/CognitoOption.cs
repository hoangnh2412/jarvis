namespace Jarvis.Authentication.Cognito;

public class CognitoOption
{
    public required string Region { get; set; }
    public Dictionary<string, string> UserPoolIds { get; set; } = [];
    public Dictionary<string, string> ClientIds { get; set; } = [];
}