using System.Reflection;
using System.Xml;
using Jarvis.Swashbuckle.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Jarvis.Swashbuckle;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddCoreSwagger(this IHostApplicationBuilder builder)
    {
        var swaggerSection = builder.Configuration.GetSection("Swagger");
        var swaggerOption = swaggerSection.Get<SwaggerOption>();
        if (!swaggerOption!.Enable)
            return builder;

        builder.Services.Configure<SwaggerOption>(swaggerSection);
        builder.Services.AddTransient<ProducesBaseResponseOperationFilter>();

        builder.Services.AddSwaggerGen(options =>
        {
            if (swaggerOption.Versions is { Length: > 0 })
            {
                foreach (var version in swaggerOption.Versions)
                {
                    options.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = Assembly.GetEntryAssembly()?.GetName().Name,
                        Version = version
                    });
                }

                options.DocInclusionPredicate((documentName, apiDescription) =>
                {
                    if (string.IsNullOrEmpty(apiDescription.GroupName))
                        return true;

                    return string.Equals(documentName, apiDescription.GroupName, StringComparison.OrdinalIgnoreCase);
                });
            }

            var rootPath = AppContext.BaseDirectory;
            foreach (var path in Directory.GetFiles(rootPath, "*.xml"))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                if (xmlDoc.SelectSingleNode("doc") == null)
                    continue;

                options.IncludeXmlComments(path);
            }

            options.SchemaFilter<BaseResponseSchemaFilter>();

            if (swaggerOption.SecuritySchemes is { Length: > 0 })
            {
                var apiKeyHeader = string.IsNullOrWhiteSpace(swaggerOption.ApiKeyHeaderName)
                    ? "X-API-KEY"
                    : swaggerOption.ApiKeyHeaderName.Trim();

                foreach (var scheme in swaggerOption.SecuritySchemes)
                {
                    if (scheme == "JWT")
                    {
                        options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
                        {
                            Description = "JWT Authorization header (Bearer). Example: `Bearer <token>`",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT"
                        });
                    }

                    if (scheme == "API_KEY")
                    {
                        options.AddSecurityDefinition("API_KEY", new OpenApiSecurityScheme
                        {
                            Name = apiKeyHeader,
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey,
                            Description = $"API key sent in the `{apiKeyHeader}` request header.",
                        });
                    }
                }

                options.OperationFilter<AuthenticationOperationFilter>(string.Join('|', swaggerOption.SecuritySchemes));
            }

            options.OperationFilter<TraceParentHeaderOperationFilter>();
            options.OperationFilter<ProducesBaseResponseOperationFilter>();
        });

        builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        return builder;
    }
}
