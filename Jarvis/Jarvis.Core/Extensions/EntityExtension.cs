using System;
using System.Linq;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Models;
using System.Collections.Generic;

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
            //nếu là quyền đặc biệt => query theo tenantCode luôn
            if (context.Session.Claims.ContainsKey(nameof(SpecialPolicy.Special_DoEnything))
                || context.Session.Claims.ContainsKey(nameof(SpecialPolicy.Special_TenantAdmin)))
            {
                return queryable.QueryByTenantCode(context.TenantCode);
            }

            queryable = queryable.QueryByTenantCode(context.TenantCode);

            //nếu quyền cty hiện tại hoặc chi nhánh là cá nhân => query theo idUser hiện tại
            if (context.ClaimOfResource == ClaimOfResource.Owner)
                queryable = queryable.QueryByCreatedBy(context.IdUser);

            return queryable;
        }

        public static IQueryable<T> QueryByPermissionOld<T>(this IQueryable<T> queryable, ContextModel context) where T : IPermissionEntity
        {
            //nếu là quyền đặc biệt => query theo tenantCode luôn
            if (context.Session.Claims.ContainsKey(nameof(SpecialPolicy.Special_DoEnything))
                || context.Session.Claims.ContainsKey(nameof(SpecialPolicy.Special_TenantAdmin)))
            {
                return queryable.QueryByTenantCode(context.TenantCode);
            }

            //nếu quyền sử dụng chi nhánh = none (k sử dụng) và đang thao tác query ở chi nhánh => k lấy dữ liệu => query theo guid.empty
            if (context.ClaimOfChildResource == ClaimOfChildResource.None && context.TenantCode != context.Session.TenantInfo.Code)
                return queryable.QueryByTenantCode(Guid.Empty);

            queryable = queryable.QueryByTenantCode(context.TenantCode);

            //nếu quyền cty hiện tại hoặc chi nhánh là cá nhánh => query theo idUser hiện tại
            if (context.ClaimOfResource == ClaimOfResource.Owner || context.ClaimOfChildResource == ClaimOfChildResource.Owner)
                queryable = queryable.QueryByCreatedBy(context.IdUser);

            return queryable;
        }
    }
}
