using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Core.Services;
using System.Linq;
using Jarvis.Core.Permissions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Jarvis.Core.Extensions
{
    public static class IdentityResultExtension
    {
        public static string ToMessage(this IdentityResult source)
        {
            return string.Join(';', source.Errors.Select(x => x.Description).ToList());
        }
    }
}
