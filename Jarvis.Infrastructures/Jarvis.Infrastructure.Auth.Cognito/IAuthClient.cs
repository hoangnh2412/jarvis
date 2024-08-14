using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;

namespace Jarvis.Infrastructure.Auth.Cognito;

public interface IAuthClient
{
    Task<AuthFlowResponse> GetTokenAsync(string userName, string password, string userPoolId, string clientId);

    Task<AdminGetUserResponse> GetUserAsync(AdminGetUserRequest request);

    Task<AdminCreateUserResponse> CreateUserAsync(AdminCreateUserRequest request);

    Task<AdminUpdateUserAttributesResponse> UpdateUserAsync(AdminUpdateUserAttributesRequest request);
}