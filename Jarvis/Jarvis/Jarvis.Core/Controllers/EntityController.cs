using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    public class EntityController : Controller
    {
        private readonly IEntityService _entityService;

        public EntityController(IEntityService entityService)
        {
            _entityService = entityService;
        }

        [HttpGet]
        public IActionResult Index(string slug)
        {
            var entity = _entityService.GetRouting(slug);
            if (entity == null)
                return NotFound();

            return RedirectToAction(entity.EntityType.RoutingAction, entity.EntityType.RoutingController, entity.EntityId);
        }
    }
}
