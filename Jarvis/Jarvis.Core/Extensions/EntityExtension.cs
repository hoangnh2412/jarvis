using System;
using System.Linq;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Models;

namespace Jarvis.Core.Permissions
{
    public static class EntityExtension
    {
        public static IQueryable<T> QueryByTenantCode<T>(this IQueryable<T> queryable, Guid tenantCode) where T : ITenantEntity
        {
            return queryable.Where(x => x.TenantCode == tenantCode);
        }

        public static IQueryable<T> QueryByCreatedBy<T>(this IQueryable<T> queryable, Guid userCode) where T : ILogCreatedEntity
        {
            return queryable.Where(x => x.CreatedBy == userCode);
        }

        public static IQueryable<T> QueryByDeletedBy<T>(this IQueryable<T> queryable) where T : ILogDeletedEntity
        {
            return queryable.Where(x => x.DeletedBy == null);
        }

        public static IQueryable<T> QueryByPermission<T>(this IQueryable<T> queryable, ContextModel context) where T : IPermissionEntity
        {
            if (context.Session.Type == UserType.SuperAdmin)
                return queryable;
            else
                return queryable.QueryByTenantCode(context.TenantKey);
        }
    }
}
