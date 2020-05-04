using Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Core.Services;

namespace Jarvis.Core.Permissions
{
    public class UserLockAuthorizationPolicy : IAuthorizationPolicy
    {
        public string Name => nameof(CorePolicy.UserPolicy.User_Lock);

        public AuthorizationPolicy Build(HttpContext httpContext)
        {
            AuthorizationPolicyBuilder authorizationPolicyBuilder = new AuthorizationPolicyBuilder();

            authorizationPolicyBuilder.RequireAssertion(async context =>
            {
                return await context.User.HasClaimAsync(httpContext, x => x.Type == Name);
            });

            return authorizationPolicyBuilder.Build();
        }
    }

    public class UserResetPasswordAuthorizationPolicy : IAuthorizationPolicy
    {
        public string Name => nameof(CorePolicy.UserPolicy.User_Reset_Password);

        public AuthorizationPolicy Build(HttpContext httpContext)
        {
            AuthorizationPolicyBuilder authorizationPolicyBuilder = new AuthorizationPolicyBuilder();

            authorizationPolicyBuilder.RequireAssertion(async context =>
            {
                return await context.User.HasClaimAsync(httpContext, x => x.Type == Name);
            });

            return authorizationPolicyBuilder.Build();
        }
    }
}
