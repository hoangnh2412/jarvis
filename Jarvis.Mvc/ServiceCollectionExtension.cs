using System.Text.Json.Serialization;
using Jarvis.Domain.Shared.Enums;
using Jarvis.Domain.Shared.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Jarvis.Mvc;

public static class ServiceCollectionExtension
{
    public static IMvcBuilder AddBadRequestHandler(this IMvcBuilder services)
    {
        #nullable disable
        services.ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                return new BadRequestObjectResult(new BaseResponse(
                    requestId: actionContext.HttpContext.TraceIdentifier,
                    httpStatusCode: System.Net.HttpStatusCode.BadRequest,
                    code: BaseErrorCode.Default,
                    error: new BaseResponseError
                    {
                        Details = actionContext.ModelState
                            .Where(x => x.Value != null)
                            .Select(x => new KeyValuePair<string, IList<string>>(
                                x.Key,
                                x.Value?.Errors.Select(y => y.ErrorMessage).ToList())
                            )
                            .ToDictionary(x => x.Key, x => x.Value)
                    }
                ));
            }
        );

        return services;
    }

    public static IServiceCollection AddCoreWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        var jsonOption = new JsonOption();
        configuration.GetSection("Json").Bind(jsonOption);

        services
            .AddControllers()
            .AddBadRequestHandler()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = jsonOption.IgnoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = jsonOption.IgnoreNull ? NullValueHandling.Ignore : NullValueHandling.Include;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddHealthChecks();

        return services;
    }
}
