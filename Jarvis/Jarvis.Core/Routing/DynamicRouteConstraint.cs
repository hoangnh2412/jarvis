using Jarvis.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Core.Routing
{
    public class DynamicRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var path = httpContext.Request.Path.Value;

            //Trim the leading slash '/'
            if (path.StartsWith('/'))
                path = path.Substring(1);

            var entityService = httpContext.RequestServices.GetService<IEntityService>();
            var entity = entityService.GetRouting(path);
            if (entity == null)
                return false;

            return true;
        }
    }
}
