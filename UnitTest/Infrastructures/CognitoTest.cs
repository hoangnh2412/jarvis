using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Jarvis.Persistence;
using Jarvis.Infrastructure.Auth.Cognito;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Jarvis.Shared.Options;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace UnitTest.Cognitos;

[TestClass]
public class CognitoTest : BaseTest
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

        services.Configure<CognitoOption>(Configuration.GetSection("AppSetting:Authentication:Cognito"));
        services.AddSingleton<IAuthClient, CognitoClient>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task Test_Cognito_CreateUserAsync()
    {
        try
        {
            var option = _serviceProvider.GetService<IOptions<CognitoOption>>().Value;
            var client = _serviceProvider.GetService<IAuthClient>();

            var input = new AdminCreateUserRequest
            {
                Username = System.Text.RegularExpressions.Regex.Replace(DateTime.UtcNow.TimeOfDay.ToString(), "[^0-9]", ""),
                MessageAction = MessageActionType.SUPPRESS,
                UserAttributes = new List<AttributeType> {
                    new AttributeType { Name = "email", Value = "xobruniffibe-2174@yopmail.com" },
                    new AttributeType { Name = "email_verified", Value = "true" },
                },
                DesiredDeliveryMediums = new List<string>(),
                UserPoolId = option.DefaultUserPoolId
            };

            input.DesiredDeliveryMediums.Add("EMAIL");

            await client.CreateUserAsync(input);
        }
        catch (System.Exception ex)
        {
            var logger = _serviceProvider.GetService<ILogger<CognitoTest>>();
            logger.LogError(ex, ex.Message);
        }
    }
}