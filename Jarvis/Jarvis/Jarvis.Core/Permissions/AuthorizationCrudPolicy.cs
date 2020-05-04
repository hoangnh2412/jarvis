using Infrastructure.Abstractions;
using Infrastructure.Database.Entities;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Permissions
{
    public class TenantAuthorizationCrudPolicy : AuthorizationCrudPolicy<Tenant>, IAuthorizationCrudPolicy
    {
    }

    public class UserAuthorizationCrudPolicy : AuthorizationCrudPolicy<User>, IAuthorizationCrudPolicy
    {
    }

    public class RoleAuthorizationCrudPolicy : AuthorizationCrudPolicy<Role>, IAuthorizationCrudPolicy
    {
    }

    public class LabelAuthorizationCrudPolicy : AuthorizationCrudPolicy<Label>, IAuthorizationCrudPolicy
    {
    }

    public class SettingAuthorizationCrudPolicy : AuthorizationCrudPolicy<Setting>, IAuthorizationCrudPolicy
    {
    }

    public class OrganizationAuthorizationCrudPolicy : AuthorizationCrudPolicy<OrganizationUnit>, IAuthorizationCrudPolicy
    {
    }
}
