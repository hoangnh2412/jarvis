namespace Jarvis.Authentication.Cognito;

/// <summary>AWS credentials cho <see cref="CognitoClient"/> (admin SDK).</summary>
public class AwsOption
{
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public string SessionToken { get; set; } = string.Empty;
}
