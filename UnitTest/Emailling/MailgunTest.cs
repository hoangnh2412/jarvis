using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Infrastructure.Emailing.Mailgun;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Emailling;

[TestClass]
public class MailgunTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(Configuration);

        services.AddHttpClient<IMailgunClient, MailgunClient>("mailgun");

        _serviceProvider = services.BuildServiceProvider();
    }
}