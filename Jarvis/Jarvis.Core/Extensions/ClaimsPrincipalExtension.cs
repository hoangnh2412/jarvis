using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Core.Services;
using System.Linq;
using Jarvis.Core.Permissions;
using System.Threading.Tasks;
using Jarvis.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Jarvis.Core.Models;

namespace Jarvis.Core.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        //https://stackoverflow.com/questions/58565574/reading-the-authorizationfiltercontext-in-netcore-api-3-1
        //https://github.com/aspnet/AspNetCore.Docs/issues/12564
        //https://github.com/aspnet/AspNetCore/issues/11075
        //https://stackoverflow.com/questions/57973981/net-core-3-custom-authorization-policy-access-to-action-verb
        //https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
        public static async Task<bool> HasClaimAsync(this ClaimsPrincipal user, HttpContext httpContext, Predicate<Claim> match)
        {
            var workContext = httpContext.RequestServices.GetService<IWorkContext>();
            var session = await workContext.GetSessionAsync();
            if (session == null)
                return false;

            var claims = session.Claims.Select(x => new Claim(x.Key, x.Value.ToString())).ToList();

            var result = claims.Any(x => x.Type == nameof(SpecialPolicy.Special_DoEnything)
                            || (x.Type == nameof(SpecialPolicy.Special_TenantAdmin) && ClaimOfTenantAdmin.Claims.Any(y => match(y)))
                            || match(x));

            return result;
        }
    }
}
