using System.Reflection;
using System.Xml;
using Jarvis.Swashbuckle.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Jarvis.Swashbuckle;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCoreSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var isEnableConfig = configuration.GetSection("Swagger:Enable").Value;
        if (string.IsNullOrEmpty(isEnableConfig))
            return services;

        if (!bool.Parse(isEnableConfig))
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
                    options.SwaggerDoc(version, new OpenApiInfo { Title = Assembly.GetEntryAssembly()?.GetName().Name, Version = version });
                }
            }

            string rootPath = AppContext.BaseDirectory;
            var paths = Directory.GetFiles(rootPath, "*.xml");
            foreach (var path in paths)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                if (xmlDoc.SelectSingleNode("doc") == null)
                    continue;

                options.IncludeXmlComments(path);
            }

            if (swaggerOption.SecuritySchemes != null && swaggerOption.SecuritySchemes.Length > 0)
            {
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
            }
        });

        return services;
    }
}