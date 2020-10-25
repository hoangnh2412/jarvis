using Infrastructure.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Permissions
{
    public class OrganizationUsersAuthorizationPolicy : BaseAuthorizationPolicy, IAuthorizationPolicy
    {
        public override string Name => nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Users);
    }

    public class OrganizationRolesAuthorizationPolicy : BaseAuthorizationPolicy, IAuthorizationPolicy
    {
        public override string Name => nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Roles);
    }
}
