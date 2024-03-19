using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Jarvis.WebApi.Swagger;

namespace Jarvis.WebApi;

public static class SwaggeExtension
{
    public static IServiceCollection AddCoreSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        if (!bool.Parse(configuration.GetSection("Swagger:Enable").Value))
            return services;

        var swaggerSection = configuration.GetSection("Swagger");
        SwaggerOption swaggerOption = new SwaggerOption();
        swaggerSection.Bind(swaggerOption);
        services.Configure<SwaggerOption>(swaggerSection);

        services.AddSwaggerGen(options =>
        {
            if (swaggerOption.Versions != null && swaggerOption.Versions.Length > 0)
            {
                foreach (var version in swaggerOption.Versions)
                {
                    options.SwaggerDoc(version, new OpenApiInfo { Title = Assembly.GetEntryAssembly().GetName().Name, Version = version });
                }
            }

            foreach (var scheme in swaggerOption.SecuritySchemes)
            {
                if (scheme == "JWT")
                {
                    options.AddSecurityDefinition(scheme, new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                }

                if (scheme == "API_KEY")
                {
                    options.AddSecurityDefinition(scheme, new OpenApiSecurityScheme()
                    {
                        Name = "X-API-KEY",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Description = "Authorization by X-API-KEY inside request's header",
                        Scheme = "ApiKey"
                    });
                }
            }

            options.OperationFilter<AuthenticationOperationFilter>(string.Join('|', swaggerOption.SecuritySchemes));
        });

        return services;
    }

    public static IApplicationBuilder UseCoreSwagger(this IApplicationBuilder app)
    {
        var swaggerOption = app.ApplicationServices.GetService<IOptions<SwaggerOption>>().Value;
        if (!swaggerOption.Enable)
            return app;

        app.UseSwagger(options =>
        {
            options.RouteTemplate = swaggerOption.Prefix + "/swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            if (swaggerOption.Versions == null || swaggerOption.Versions.Length == 0)
                return;

            foreach (var version in swaggerOption.Versions)
            {
                if (string.IsNullOrEmpty(swaggerOption.Prefix))
                {
                    options.SwaggerEndpoint($"{swaggerOption.Prefix}/swagger/{version}/swagger.json", $"{Assembly.GetEntryAssembly().GetName().Name} API {version}");
                }
                else
                {
                    options.SwaggerEndpoint($"/{swaggerOption.Prefix}/swagger/{version}/swagger.json", $"{Assembly.GetEntryAssembly().GetName().Name} API {version}");
                    options.RoutePrefix = $"{swaggerOption.Prefix}/swagger";
                }
            }
        });
        return app;
    }

    private class AuthenticationOperationFilter : IOperationFilter
    {
        private readonly string[] _types;

        public AuthenticationOperationFilter(string types)
        {
            _types = types.Split('|');
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var controllerAttrs = context.MethodInfo.ReflectedType.GetCustomAttributes(true);
            var actionAttrs = context.MethodInfo.GetCustomAttributes(true);

            var scopes = new List<string>();

            scopes.AddRange(controllerAttrs.OfType<AuthorizeAttribute>().Select(attr => attr.Policy).Distinct());
            scopes.AddRange(actionAttrs.OfType<AuthorizeAttribute>().Select(attr => attr.Policy).Distinct());
            if (scopes.Count == 0)
                return;

            var allows = actionAttrs.OfType<AllowAnonymousAttribute>().Distinct().ToList();
            if (allows.Count > 0)
                return;

            allows = controllerAttrs.OfType<AllowAnonymousAttribute>().Distinct().ToList();
            if (allows.Count > 0)
                return;

            var requirements = new List<OpenApiSecurityRequirement>();
            foreach (var item in _types)
            {
                var scheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = item
                    }
                };
                requirements.Add(new OpenApiSecurityRequirement()
                {
                    [scheme] = scopes
                });
            }

            operation.Security = requirements;
        }
    }
}