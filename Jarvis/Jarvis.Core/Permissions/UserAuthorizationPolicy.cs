using Infrastructure.Abstractions;

namespace Jarvis.Core.Permissions
{
    public class UserLockAuthorizationPolicy : BaseAuthorizationPolicy, IAuthorizationPolicy
    {
        public override string Name => nameof(CorePolicy.UserPolicy.User_Lock);
    }

    public class UserResetPasswordAuthorizationPolicy : BaseAuthorizationPolicy, IAuthorizationPolicy
    {
        public override string Name => nameof(CorePolicy.UserPolicy.User_Reset_Password);
    }
}
