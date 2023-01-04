using System;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;

namespace Jarvis.Core.Database.SqlServer
{
    public class CoreUnitOfWork : EntityUnitOfWork<CoreDbContext>, ICoreUnitOfWork
    {
        public CoreUnitOfWork(IServiceProvider services, Func<string, IStorageContext> config) : base(services, config)
        {
        }
    }
}