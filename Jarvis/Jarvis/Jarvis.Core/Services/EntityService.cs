using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Database;

namespace Jarvis.Core.Services
{
    public interface IEntityService
    {
        Entity GetRouting(string slug);
    }

    public class EntityService : IEntityService
    {
        private readonly IServiceProvider _provider;

        public EntityService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Entity GetRouting(string slug)
        {
            return null;
            //using(var scope = _provider.CreateScope())
            //{
            //    var uow = scope.ServiceProvider.GetService<ICoreUnitOfWork>();
            //    var repo = uow.GetRepository<Entity>();
            //    var entity = repo.First(
            //        predicate: x => x.Slug == slug,
            //        include: x => x.Include(y => y.EntityType));

            //    return entity;
            //}
        }
    }
}
