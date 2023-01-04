using System;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;

namespace Jarvis.Core.Database.MySql
{
    public class CoreUnitOfWork : EntityUnitOfWork<CoreDbContext>, ICoreUnitOfWork
    {
        public CoreUnitOfWork(IServiceProvider services, Func<string, IStorageContext> config) : base(services, config)
        {
        }
    }
}