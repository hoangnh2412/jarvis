﻿using Microsoft.Extensions.Options;
using Amazon.CognitoIdentityProvider;
using Amazon;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Jarvis.Shared.Options;

namespace Jarvis.Infrastructure.Auth.Cognito;

public class CognitoClient : IAuthClient
{
    public AmazonCognitoIdentityProviderClient IdentityProvider;
    private readonly CognitoOption _options;

    public CognitoClient(
        IOptions<CognitoOption> options)
    {
        _options = options.Value;

        IdentityProvider = InitProvider();
    }

    private AmazonCognitoIdentityProviderClient InitProvider()
    {
        if (string.IsNullOrEmpty(_options.SessionToken))
        {
            return new AmazonCognitoIdentityProviderClient(
                awsAccessKeyId: _options.AccessKey,
                awsSecretAccessKey: _options.SecretKey,
                region: RegionEndpoint.GetBySystemName(_options.Region));
        }
        else
        {
            return new AmazonCognitoIdentityProviderClient(
                awsAccessKeyId: _options.AccessKey,
                awsSecretAccessKey: _options.SecretKey,
                awsSessionToken: _options.SessionToken,
                region: RegionEndpoint.GetBySystemName(_options.Region));
        }
    }

    public async Task<AdminGetUserResponse> GetUserAsync(AdminGetUserRequest request)
    {
        return await IdentityProvider.AdminGetUserAsync(request);
    }

    public async Task<AdminCreateUserResponse> CreateUserAsync(AdminCreateUserRequest request)
    {
        return await IdentityProvider.AdminCreateUserAsync(request);
    }

    public async Task<AdminUpdateUserAttributesResponse> UpdateUserAsync(AdminUpdateUserAttributesRequest request)
    {
        return await IdentityProvider.AdminUpdateUserAttributesAsync(request);
    }

    public async Task<AuthFlowResponse> GetTokenAsync(string userName, string password, string userPoolId, string clientId)
    {
        if (string.IsNullOrEmpty(userPoolId))
            userPoolId = _options.DefaultUserPoolId;

        if (string.IsNullOrEmpty(clientId))
            clientId = _options.DefaultClientId;

        var cognitoUserPool = new CognitoUserPool(userPoolId, clientId, IdentityProvider);
        var cognitoUser = new CognitoUser(userName, clientId, cognitoUserPool, IdentityProvider);

        return await cognitoUser.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
        {
            Password = password,
        });
    }
}
