﻿using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Repositories.EntityFramework;
using Jarvis.Core.Database.Poco;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class FileRepository : EntityRepository<File>, IFileRepository
    {
        public async Task<File> GetByKeyAsync(Guid key)
        {
            return await Query.FirstOrDefaultAsync(x => x.Key == key);
        }

        public async Task<List<File>> QueryByIdsAsync(List<Guid> keys)
        {
            return await Query.Query(x => keys.Contains(x.Key)).ToListAsync();
        }

        public async Task<List<File>> QueryFileInRangeAsync(DateTime from, DateTime to)
        {
            return await Query.Query(x => x.CreatedAt >= from.Date && x.CreatedAt < to).ToListAsync();
        }
    }
}
