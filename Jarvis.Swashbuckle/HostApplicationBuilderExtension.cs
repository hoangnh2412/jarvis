using System.Reflection;
using System.Xml;
using Jarvis.Swashbuckle.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Jarvis.Swashbuckle;

public static class ServiceCollectionExtension
{
    public static IHostApplicationBuilder AddCoreSwagger(this IHostApplicationBuilder builder)
    {
        var swaggerSection = builder.Configuration.GetSection("Swagger");
        var swaggerOption = swaggerSection.Get<SwaggerOption>();
        if (!swaggerOption!.Enable)
            return builder;

        builder.Services.Configure<SwaggerOption>(swaggerSection);

        builder.Services.AddSwaggerGen(options =>
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

            options.OperationFilter<TraceParentHeaderOperationFilter>();
        });

        builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        return builder;
    }
}