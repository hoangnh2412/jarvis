using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Interfaces;

namespace UnitTest.Common;

[TestClass]
public class WorkContextTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IWorkContext, TestWorkContext>(sp =>
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("Authorization", "Bearer eyJraWQiOiJEK21MU0krcWVHWVpZVGUrTURKNVNFNzRHMjNpTFljcExMU1JjRFduWjUwPSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiJkODA0MTJhNS1hNDIwLTQ5MDItODdjYi05MWEzNTYyNDE1NjgiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiaXNzIjoiaHR0cHM6XC9cL2NvZ25pdG8taWRwLnVzLWVhc3QtMS5hbWF6b25hd3MuY29tXC91cy1lYXN0LTFfRkUwc0pCZnpXIiwicGhvbmVfbnVtYmVyX3ZlcmlmaWVkIjp0cnVlLCJVc2VySW5mbyI6IntcIklkXCI6XCIwMGI4MDBlNy1hNzMyLTQ5YzctYmVkYS02MmY1MzhiYzA5OTNcIixcIlVzZXJOYW1lXCI6XCIwODQyNDgwMjg5MjE4XCIsXCJGaXJzdE5hbWVcIjpcIkNsZW9cIixcIkxhc3ROYW1lXCI6XCJNY0dseW5uXCIsXCJFbWFpbFwiOlwiTWNHbHlubi5DbGVvQHRvcnVzLnZuXCIsXCJQaG9uZU51bWJlclwiOlwiKzg0OTM2OTE5MDkxXCJ9IiwiY29nbml0bzp1c2VybmFtZSI6IjA4NDI0ODAyODkyMTgiLCJnaXZlbl9uYW1lIjoiTmd1eWVuIiwib3JpZ2luX2p0aSI6ImNkY2Q0OTBiLTZjNDItNDg2Yy1hNGQxLWYwYzhkMzMxNWE5ZCIsImF1ZCI6IjJhNTEwbWpxZWM5NDJ1NDlpN25rZW1hdGFhIiwiZXZlbnRfaWQiOiJmYTA3NGVjMS1kNWM4LTRkMDctYjE3Mi0zZTM1MWFjYmYxOGQiLCJ0b2tlbl91c2UiOiJpZCIsImF1dGhfdGltZSI6MTcxMDc1MjA0MCwicGhvbmVfbnVtYmVyIjoiKzg0OTM2OTE5MDkxIiwiZXhwIjoxNzEwNzU1NjM5LCJpYXQiOjE3MTA3NTIwNDAsImZhbWlseV9uYW1lIjoiSG9hbmciLCJqdGkiOiI0MWQ4YTE2Ny1mN2I1LTQ3ODYtODM1NC03YzJhOThhMGNkZmIiLCJlbWFpbCI6ImhvYW5nLm5ndXllbkB0b3J1cy52biJ9.Q9ai-8T144ETkil_wXuXMH5mdjRz_Uk16FT8QZp_4gT7eqysJD5pj4r90F2AE9aalxD-_S27Hd5JZybymaIe_rpfr3xdfNquxyKZyKlPqDni95k8jCNfC8Bt2Yfrd1ULR3uRrJPrGnYH2zXT-n5CxSn4V_NyyyBuybrpJFAskzjW6R2R48JSrzKPY8EfCqFqrRFxJCHfpZcKEWfv48AAI6ggtE8pia66Bu14vk8tYL2L34mA78xfJtDJLPGughAWoN6Eb6lSkUbw8wNefC2K6wVZnqdLMQaEywoh_YkvBVcKCWSS1qtTXHc6XVikAnTYd4-3tqdxlkspoOnbvQskWA");

            var accessor = new HttpContextAccessor();
            accessor.HttpContext = context;
            return new TestWorkContext(accessor);
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public void Test_WorkContext()
    {
        var workContext = _serviceProvider.GetService<IWorkContext>();
        Assert.IsNotNull(workContext.UserInfo);
        Assert.IsNotNull(workContext.UserId);
        Assert.IsNotNull(workContext.UserName);
        Assert.IsNotNull(workContext.FullName);
    }
}