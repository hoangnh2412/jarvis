using Infrastructure.Abstractions;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Permissions
{
    public abstract class BaseAuthorizationPolicy : IAuthorizationPolicy
    {
        public abstract string Name { get; }

        public virtual AuthorizationPolicy Build(HttpContext httpContext)
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
