using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Module.Notifications.Controllers;
using Module.Notifications.Extensions;

namespace UnitTest.Notifications;

public class NotificationModuleRegistrationTests
{
    [Fact]
    public void AddNotificationModule_Registers_Controller_AssemblyPart()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers();
        builder.AddNotificationModule();

        using var app = builder.Build();
        // Kích hoạt IConfigureOptions<MvcOptions> (đăng ký ApplicationPart).
        _ = app.Services.GetRequiredService<IOptions<MvcOptions>>().Value;

        var partManager = app.Services.GetRequiredService<ApplicationPartManager>();
        var assembly = typeof(NotificationsController).Assembly;
        Assert.Contains(
            partManager.ApplicationParts.OfType<AssemblyPart>(),
            p => p.Assembly == assembly);
    }
}
