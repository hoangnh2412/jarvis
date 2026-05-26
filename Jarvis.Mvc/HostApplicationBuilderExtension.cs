using System.Text.Json.Serialization;
using Jarvis.Common.Enums;
using Jarvis.Domain.Shared.Enums;
using Jarvis.Domain.Shared.ExceptionHandling;
using Jarvis.Domain.Shared.RequestResponse;
using Jarvis.Domain.Shared.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Jarvis.Mvc;

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
}
