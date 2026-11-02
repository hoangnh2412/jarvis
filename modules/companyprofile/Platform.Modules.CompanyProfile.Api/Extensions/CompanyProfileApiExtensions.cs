using Microsoft.Extensions.DependencyInjection;
using Platform.Modules.CompanyProfile.Api.Controllers;

namespace Platform.Modules.CompanyProfile.Api.Extensions;

public static class CompanyProfileApiExtensions
{
    public static IServiceCollection AddCompanyProfileApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddControllers()
            .AddApplicationPart(typeof(CompanyProfileController).Assembly);

        return services;
    }
}
