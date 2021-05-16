using System;
using System.Collections.Generic;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;

namespace Jarvis.Core.Database.Oracle
{
    public class CoreUnitOfWork : EntityUnitOfWork<CoreDbContext>, ICoreUnitOfWork
    {
        public CoreUnitOfWork(IServiceProvider services, IEnumerable<IStorageContext> storageContexts) : base(services, storageContexts)
        {
        }
    }
}