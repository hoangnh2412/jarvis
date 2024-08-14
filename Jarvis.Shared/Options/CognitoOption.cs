namespace Jarvis.Shared.Options;

public class CognitoOption
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Region { get; set; }
    public string SessionToken { get; set; }
    public Dictionary<string, string> UserPools { get; set; }
    public string DefaultUserPoolId { get; set; }
    public string DefaultClientId { get; set; }
}