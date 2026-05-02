using Jarvis.Mvc.ApplicationBuilders.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Mvc.ApplicationBuilders;

public static class CorsExtension
{
    public static IHostApplicationBuilder AddCoreCors(this IHostApplicationBuilder builder)
    {
        var corsOptions = builder.Configuration.GetSection("Cors").Get<Dictionary<string, CorsOption>>();
        if (corsOptions == null)
            return builder;

        builder.Services.AddCors(options =>
        {
            foreach (var c in corsOptions)
            {
                options.AddPolicy(c.Key, policy =>
                {
                    if (c.Value.AllowAnyOrigin)
                        policy.AllowAnyOrigin();
                    else
                        policy.WithOrigins(c.Value.AllowedOrigins).SetIsOriginAllowedToAllowWildcardSubdomains();

                    if (c.Value.AllowedAllHeaders)
                        policy.AllowAnyHeader();
                    else
                        policy.WithHeaders(c.Value.AllowedHeaders);

                    if (c.Value.AllowedAllMethods)
                        policy.AllowAnyMethod();
                    else
                        policy.WithMethods(c.Value.AllowedMethods);

                    if (c.Value.AllowCredentials)
                        policy.AllowCredentials();
                    else
                        policy.DisallowCredentials();
                });
            }
        });
        return builder;
    }

    public static IApplicationBuilder UseCoreCors(this IApplicationBuilder builder)
    {
        var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        var corsOptions = configuration.GetSection("Cors").Get<Dictionary<string, CorsOption>>();

        if (corsOptions == null)
            return builder;

        foreach (var c in corsOptions)
        {
            builder.UseCors(c.Key);
        }
        return builder;
    }
}