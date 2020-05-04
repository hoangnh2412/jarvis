using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JarvisPresentation.Domains
{
    public class TestUnitOfWork : DapperUnitOfWork<TestDbContext>, ITestUnitOfWork
    {
        public TestUnitOfWork(IServiceProvider services, IEnumerable<IStorageContext> storageContexts) : base(services, storageContexts)
        {
        }
    }
}
