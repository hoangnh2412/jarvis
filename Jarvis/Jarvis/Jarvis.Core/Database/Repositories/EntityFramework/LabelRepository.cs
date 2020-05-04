﻿using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using Infrastructure.Database.Models;
using System;
using System.Linq;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class LabelRepository : EntityRepository<Label>, ILabelRepository
    {
        public async Task<Paged<Label>> PagingAsync(ContextModel context, Paging paging)
        {
            var paged = await Query
                .Query(
                    filter: (queryable) =>
                    {
                        queryable = queryable.QueryByDeletedBy();
                        queryable = queryable.QueryByPermission(context);
                        return queryable;
                    },
                    order: x => x.OrderByDescending(y => y.CreatedAt),
                    include: null)
                .ToPaginationAsync(paging);
            return paged;
        }

        public async Task<Label> GetByCodeAsync(ContextModel context, Guid code)
        {
            IQueryable<Label> query = DbSet.Where(x => x.Code == code);
            query = query.QueryByPermission(context);
            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }
    }
}
