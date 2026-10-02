namespace UnitTest.IdentityAdmin;

using Microsoft.AspNetCore.Identity;
using Moq;

/// <summary>
/// Helper tạo Mock&lt;UserManager&lt;TUser&gt;&gt; / Mock&lt;RoleManager&lt;TRole&gt;&gt; — pattern chuẩn vì
/// hai class này không có interface, nhưng toàn bộ member public đều <c>virtual</c> nên Moq override được qua store giả.
/// </summary>
internal static class IdentityMockHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        return new Mock<RoleManager<TRole>>(store.Object, null!, null!, null!, null!);
    }
}
