using Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Abstractions
{
    public abstract class AuthorizationCrudPolicy<T> : IAuthorizationCrudPolicy<T>
    {
        public string Name => typeof(T).Name;

        public AuthorizationPolicy Build(HttpContext httpContext)
        {
            AuthorizationPolicyBuilder authorizationPolicyBuilder = new AuthorizationPolicyBuilder();

            authorizationPolicyBuilder.RequireAssertion(async context =>
            {
                return await context.User.HasClaimAsync(httpContext, x => x.Value == Name);
            });

            return authorizationPolicyBuilder.Build();
        }
    }
}
