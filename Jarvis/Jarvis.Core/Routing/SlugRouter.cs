using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Core.Services;

namespace Jarvis.Core.Routing
{
    public class SlugRouter : IRouter
    {
        private readonly IRouter _defaultRouter;

        public SlugRouter(IRouter defaultRouter)
        {
            _defaultRouter = defaultRouter;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return _defaultRouter.GetVirtualPath(context);
        }

        public async Task RouteAsync(RouteContext context)
        {
            var path = context.HttpContext.Request.Path.Value;

            //Trim the leading slash '/'
            if (path.StartsWith('/'))
                path = path.Substring(1);

            var entityService = context.HttpContext.RequestServices.GetService<IEntityService>();
            var entity = entityService.GetRouting(path);
            if (entity == null)
                return;

            context.RouteData.Values["controller"] = entity.EntityType.RoutingController;
            context.RouteData.Values["action"] = entity.EntityType.RoutingAction;
            context.RouteData.Values["id"] = entity.EntityId;
            await _defaultRouter.RouteAsync(context);
        }
    }
}
