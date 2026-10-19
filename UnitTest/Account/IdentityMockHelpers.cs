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
        var userManager = new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.AccessFailedAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.ResetAccessFailedCountAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.IsLockedOutAsync(It.IsAny<TUser>())).ReturnsAsync(false);
        return userManager;
    }
}
