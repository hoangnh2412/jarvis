using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Authentication;
using Platform.DDD.Domain;
using Platform.DDD.Domain.Services;
using Platform.Multitenancy;
using Platform.Mvc;
using Platform.Mvc.ApplicationBuilders;
using Sample.Services;

namespace Sample.Extensions;

public static class SampleWebExtensions
{
    public static WebApplicationBuilder AddSampleWeb(this WebApplicationBuilder builder)
    {
        builder.AddCoreJson();
        builder.AddCoreCors();
        builder.AddCoreDomain();
        builder.AddCurrentUser<CurrentUserInfo>();
        builder.AddCurrentTenant<CurrentTenantInfo>();
        builder.Services.TryAddSingleton<ICurrentUserStore<CurrentUserInfo>, SampleCurrentUserStore>();
        builder.Services.TryAddSingleton<ICurrentTenantStore<CurrentTenantInfo>, SampleCurrentTenantStore>();
        builder.AddCoreWebApi();

        return builder;
    }
}
