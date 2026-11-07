using System.Text.Json.Serialization;
using Platform.Common.Enums;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.DDD.Domain.Shared.RequestResponse;
using Platform.DDD.Domain.Shared.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Platform.Mvc;

public static class HostApplicationBuilderExtension
{
    public static IMvcBuilder AddBadRequestHandler(this IMvcBuilder services)
    {
        services.ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                return new BadRequestObjectResult(new BaseResponse(
                    code: BaseErrorCode.Error,
                    error: new BaseResponseError
                    {
                        Details = actionContext.ModelState.Select(x => new BaseResponseErrorDetail
                        {
                            Field = x.Key,
                            Codes = x.Value?.Errors.Select(y => y.ErrorMessage).ToList(),
                            SystemMessages = x.Value?.Errors.Select(y => $"{y.ErrorMessage} - {ErrorCodeHelper.GetMessage(y.ErrorMessage)}").ToList(),
                        }).ToList()
                    }
                ));
            }
        );

        return services;
    }

    public static IHostApplicationBuilder AddCoreJson(this IHostApplicationBuilder builder)
    {
        var jsonOption = new JsonOption();
        builder.Configuration.GetSection("Json").Bind(jsonOption);

        builder.Services
            .AddControllers()
            .AddBadRequestHandler()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = jsonOption.IgnoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never;
                options.JsonSerializerOptions.PropertyNamingPolicy = jsonOption.NamingPolicy switch
                {
                    JsonNamingPolicy.CamelCase => System.Text.Json.JsonNamingPolicy.CamelCase,
                    JsonNamingPolicy.KebabCaseLower => System.Text.Json.JsonNamingPolicy.KebabCaseLower,
                    JsonNamingPolicy.KebabCaseUpper => System.Text.Json.JsonNamingPolicy.KebabCaseUpper,
                    JsonNamingPolicy.SnakeCaseLower => System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
                    JsonNamingPolicy.SnakeCaseUpper => System.Text.Json.JsonNamingPolicy.SnakeCaseUpper,
                    _ => System.Text.Json.JsonNamingPolicy.CamelCase,
                };
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = jsonOption.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include;
                options.SerializerSettings.ContractResolver = jsonOption.NamingPolicy switch
                {
                    JsonNamingPolicy.CamelCase => new CamelCasePropertyNamesContractResolver(),
                    _ => new DefaultContractResolver(),
                };

                JsonHelper.JsonOption = options.SerializerSettings;
            });

        return builder;
    }

    public static IHostApplicationBuilder AddCoreWebApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();

        return builder;
    }

    private static readonly string[] DefaultNonSpaPathPrefixes = ["/api", "/sse", "/hubs", "/health", "/swagger"];

    /// <summary>
    /// Serves SPA static files from wwwroot and maps unmatched page routes to <paramref name="fallbackFile"/>.
    /// Prefixes in <paramref name="nonSpaPathPrefixes"/> are not the SPA shell, so a missing controller returns 404 instead of a cacheable <c>index.html</c> (304).
    /// Bỏ qua tham số để giữ <c>/api</c>, <c>/sse</c>, <c>/hubs</c>, <c>/health</c>, <c>/swagger</c>.
    /// </summary>
    public static WebApplication UseCoreSpa(
        this WebApplication app,
        string fallbackFile = "index.html",
        IReadOnlyList<string>? nonSpaPathPrefixes = null)
    {
        var prefixes = nonSpaPathPrefixes ?? DefaultNonSpaPathPrefixes;
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapFallback(async context =>
        {
            if (IsNonSpaPath(context.Request.Path, prefixes))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.Headers.CacheControl = "no-store";
                context.Response.ContentType = "application/json";
                return;
            }

            var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            var file = Path.Combine(webRoot, fallbackFile);
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(file).ConfigureAwait(false);
        });
        return app;
    }

    private static bool IsNonSpaPath(PathString path, IReadOnlyList<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (path.StartsWithSegments(prefix))
                return true;
        }

        return false;
    }
}
