using System.Threading.Tasks;
using Jarvis.Core.Extensions;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Core.Permissions
{
    public class AuthorizationCrudHandler : AuthorizationHandler<CrudRequirement>
    {
        private readonly HttpContext _httpContext;

        public AuthorizationCrudHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CrudRequirement requirement)
        {
            var result = await context.User.HasClaimAsync(_httpContext, x => x.Value == requirement.PolicyName);
            if (result)
                context.Succeed(requirement);
            else
                context.Fail();

            // return Task.CompletedTask;
        }
    }

    public class CrudRequirement : IAuthorizationRequirement
    {
        public string PolicyName { get; set; }
        
        public CrudRequirement(string policyName)
        {
            PolicyName = policyName;
        }
    }
}