using Jarvis.Realtime.SignalR.Groups;

namespace UnitTest.Realtime;

public class RealtimeGroupNamesTests
{
    [Fact]
    public void ForUser_Uses_D_Format()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Assert.Equal("user:11111111-1111-1111-1111-111111111111", RealtimeGroupNames.ForUser(id));
    }

    [Fact]
    public void ForTenant_Uses_D_Format()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        Assert.Equal("tenant:22222222-2222-2222-2222-222222222222", RealtimeGroupNames.ForTenant(id));
    }
}
