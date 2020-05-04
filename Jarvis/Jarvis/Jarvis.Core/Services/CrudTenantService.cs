using Jarvis.Core.Database;
using System;

namespace Jarvis.Core.Services
{
    public interface ICrudTenantService
    {
        void Update(Guid id, string theme);
    }

    public class CrudTenantService : ICrudTenantService
    {
        private readonly ICoreUnitOfWork _uow;

        public CrudTenantService(ICoreUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Update(Guid id, string theme)
        {
            // var repo = _uow.GetRepository<ITenantRepository>();
            // var tenant = repo.First(x => x.Id == id);
            // tenant.Theme = theme;
            // repo.Update(tenant);
            // _uow.Commit();

            //_cacheService.Set("Multitenant", tenant);
        }
    }
}
