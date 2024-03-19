using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Jarvis.Infrastructure.DistributedEvent;
using Jarvis.Infrastructure.DistributedEvent.EventBridge;
using Jarvis.Persistence;
using Jarvis.Shared.Enums;
using Jarvis.Shared.Extensions;
using Jarvis.Shared.Options;

namespace UnitTest.Infrastructures;

[TestClass]
public class EventBridgeTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(Configuration);

        services.AddHttpContextAccessor();
        services.AddCorePersistence(Configuration);
        services.AddLogging(config =>
        {
            config.AddDebug();
            config.AddConsole();
        });

        var distributedEventSection = Configuration.GetSection("DistributedEvent");
        services.Configure<DistributedEventOption>(distributedEventSection);

        var eventBridgeSection = Configuration.GetSection("DistributedEvent:EventBridge");
        services.Configure<EventBridgeOption>(eventBridgeSection);

        services.AddSingleton<IDistributedEventBus, EventBridgeClient>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task Test_EventBridge_PublishAsync()
    {
        try
        {
            var options = _serviceProvider.GetService<IOptions<DistributedEventOption>>().Value;
            var eventBus = _serviceProvider.GetService<IDistributedEventBus>();

            var id = Guid.NewGuid();
            await eventBus.PublishAsync(
                new BaseEventMessage
                {
                    Action = EventAction.Create.GetName(),
                    Sender = "TestService",
                    EntityName = EventBusName.User.GetName(),
                    EntityData = new
                    {
                        Id = id,
                        Email = "test@gmail.com",
                        FullName = "abc xyz",
                        UserName = "1234567890",
                        Gender = "male",
                        FirstName = "abc",
                        LastName = "xyz",
                        PhoneNumber = "0987654321",
                        IsActive = true,
                        PhoneNumberConfirmed = true,
                        EmailConfirmed = true
                    },
                    Id = Guid.NewGuid().ToString()
                },
                options.DefaultEventBusName);
        }
        catch (System.Exception ex)
        {
            var logger = _serviceProvider.GetService<ILogger<EventBridgeTest>>();
            logger.LogError(ex, ex.Message);
        }
    }
}