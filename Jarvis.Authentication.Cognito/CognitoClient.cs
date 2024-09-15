using Amazon;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Cognito;

public class CognitoClient : IAuthClient
{
    public AmazonCognitoIdentityProviderClient IdentityProviderClient;
    private readonly AwsOption _awsOption;
    private readonly CognitoOption _cognitoOption;

    public CognitoClient(
        IOptions<AwsOption> awsOption,
        IOptions<CognitoOption> cognitoOption)
    {
        _awsOption = awsOption.Value;
        _cognitoOption = cognitoOption.Value;

        if (string.IsNullOrEmpty(_awsOption.SessionToken))
        {
            IdentityProviderClient = new AmazonCognitoIdentityProviderClient(
                _awsOption.AccessKey,
                _awsOption.SecretKey,
                RegionEndpoint.GetBySystemName(_cognitoOption.Region)
            );
        }
        else
        {
            IdentityProviderClient = new AmazonCognitoIdentityProviderClient(
                _awsOption.AccessKey,
                _awsOption.SecretKey,
                _awsOption.SessionToken,
                RegionEndpoint.GetBySystemName(_cognitoOption.Region)
            );
        }
    }
}
