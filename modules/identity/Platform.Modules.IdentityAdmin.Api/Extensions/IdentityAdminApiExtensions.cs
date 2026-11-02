using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Api.Controllers;
using Platform.Modules.IdentityAdmin.Api.Services;

namespace Platform.Modules.IdentityAdmin.Api.Extensions;

public static class IdentityAdminApiExtensions
{
    public static IServiceCollection AddIdentityAdminApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<AuthorizationPolicyAdminService>();
        services.TryAddScoped<IAuthorizationPolicyAdminService>(
            sp => sp.GetRequiredService<AuthorizationPolicyAdminService>());

        services
            .AddControllers()
            .AddApplicationPart(typeof(AppIdentityUsersController).Assembly);

        return services;
    }
}
