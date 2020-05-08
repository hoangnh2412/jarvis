using Jarvis.Core.Permissions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Jarvis.Core.Constants
{
    public static class ClaimOfTenantAdmin
    {
        public static string claimOfResourceTenantAdmin = $"[{ClaimOfResource.Tenant.ToString()}, {ClaimOfChildResource.Tenant.ToString()}]";

        public static List<Claim> Claims = new List<Claim> {
                    new Claim(nameof(CorePolicy.UserPolicy.User_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.UserPolicy.User_Create), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.UserPolicy.User_Update), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.UserPolicy.User_Delete), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.UserPolicy.User_Lock), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.UserPolicy.User_Reset_Password), claimOfResourceTenantAdmin),

                    new Claim(nameof(CorePolicy.TenantPolicy.Tenant_Create), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.TenantPolicy.Tenant_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.TenantPolicy.Tenant_Delete), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.TenantPolicy.Tenant_Update), claimOfResourceTenantAdmin),

                    new Claim(nameof(CorePolicy.RolePolicy.Role_Create), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.RolePolicy.Role_Delete), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.RolePolicy.Role_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.RolePolicy.Role_Update), claimOfResourceTenantAdmin),

                    new Claim(nameof(CorePolicy.SettingPolicy.Setting_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.SettingPolicy.Setting_Update), claimOfResourceTenantAdmin),

                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Create), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Delete), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Roles), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Users), claimOfResourceTenantAdmin),

                    new Claim(nameof(CorePolicy.LabelPolicy.Label_Create), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.LabelPolicy.Label_Delete), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.LabelPolicy.Label_Read), claimOfResourceTenantAdmin),
                    new Claim(nameof(CorePolicy.LabelPolicy.Label_Update), claimOfResourceTenantAdmin),

                    new Claim("Dashboard_Read", claimOfResourceTenantAdmin),
                };

    }
}
