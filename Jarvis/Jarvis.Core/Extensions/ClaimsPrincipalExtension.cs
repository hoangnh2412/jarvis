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

            if (session.Type == UserType.SuperAdmin)
                return true;

            if (session.Type == UserType.Admin)
                return true;

            var temp1 = session.Claims.Select(x => x.Value.Select(y => new Claim(x.Key, y)));
            var temp2 = temp1.SelectMany(x => x);
            return temp2.Any(x => match(x));
        }
    }
}
