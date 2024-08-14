using Jarvis.Infrastructure.Emailing;
using Jarvis.Infrastructure.Emailing.Mailkit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Emailling;

[TestClass]
public class MailkitTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(Configuration);

        services.AddSingleton<IEmailSender, MailkitSender>();

        _serviceProvider = services.BuildServiceProvider();
    }
}