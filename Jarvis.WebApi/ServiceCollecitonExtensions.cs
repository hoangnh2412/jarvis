using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Jarvis.Persistence;
using Jarvis.Application;
using Jarvis.Application.Interfaces;
using Jarvis.Shared.Options;
using System.Text.Json.Serialization;

namespace Jarvis.WebApi;

public static class ServiceCollecitonExtensions
{
    public static IServiceCollection AddCoreDefault(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreApplication();
        services.AddCoreWebApi(configuration);
        services.AddCorePersistence(configuration);

        services.AddCoreSwagger(configuration);
        services.AddCoreLocalization(configuration);

        return services;
    }

    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, int handlerLifetimeSecond = 300, int retryCount = 5)
    {
        return builder
            .SetHandlerLifetime(TimeSpan.FromSeconds(handlerLifetimeSecond))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(1.5, retryAttempt) * 1000),
                    onRetry: (_, waitingTime) => Console.WriteLine($"----- Retrying in {waitingTime.TotalSeconds}s")));
    }

    public static IServiceCollection AddCoreLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        var localizationOption = new LocalizationOption();
        configuration.GetSection("Localization").Bind(localizationOption);

        if (localizationOption.Cultures == null || localizationOption.Cultures.Length == 0)
            localizationOption.Cultures = new string[] { "en-US", "vi-VN" };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(localizationOption.Cultures[0]);
            options.AddSupportedCultures(localizationOption.Cultures);
            options.AddSupportedUICultures(localizationOption.Cultures);
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        services
            .AddLocalization(option => option.ResourcesPath = option.ResourcesPath)
            .AddControllersWithViews()
            .AddDataAnnotationsLocalization();

        return services;
    }

    public static IServiceCollection AddCoreWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        var jsonOption = new JsonOption();
        configuration.GetSection("Json").Bind(jsonOption);

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = jsonOption.IgnoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never;
            });

        services.AddEndpointsApiExplorer();

        services.AddHttpContextAccessor();
        services.AddScoped<IWorkContext, WorkContext>();

        return services;
    }
}