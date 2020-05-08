using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JarvisPresentation.Domains
{
    public class TestDbContext : BaseStorageContext, IStorageContext
    {
        public TestDbContext(string strConnection) : base(strConnection)
        {
        }
    }
}
