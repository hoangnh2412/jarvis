namespace UnitTest.Account;

using Microsoft.AspNetCore.Identity;
using Moq;

/// <summary>
/// Helper tạo Mock&lt;UserManager&lt;TUser&gt;&gt; — pattern chuẩn vì UserManager không có interface,
/// nhưng toàn bộ member public đều <c>virtual</c> nên Moq override được qua store giả.
/// </summary>
internal static class IdentityMockHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
