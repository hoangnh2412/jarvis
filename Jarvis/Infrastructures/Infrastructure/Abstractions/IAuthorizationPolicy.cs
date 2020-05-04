using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Abstractions
{
    public interface IAuthorizationPolicy
    {
        string Name { get; }

        AuthorizationPolicy Build(HttpContext httpContext);
    }
}
