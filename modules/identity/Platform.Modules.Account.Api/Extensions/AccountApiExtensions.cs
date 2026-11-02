using Microsoft.Extensions.DependencyInjection;
using Platform.Modules.Account.Api.Controllers;

namespace Platform.Modules.Account.Api.Extensions;

public static class AccountApiExtensions
{
    public static IServiceCollection AddAccountApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddControllers()
            .AddApplicationPart(typeof(AppAccountController).Assembly);

        return services;
    }
}
