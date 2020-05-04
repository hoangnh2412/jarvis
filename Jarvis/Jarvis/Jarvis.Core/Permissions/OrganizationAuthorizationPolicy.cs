using Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Permissions
{
    public class OrganizationUsersAuthorizationPolicy : IAuthorizationPolicy
    {
        public string Name => nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Users);

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

    public class OrganizationRolesAuthorizationPolicy : IAuthorizationPolicy
    {
        public string Name => nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Roles);

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
