using System.Reflection;
using Jarvis.Domain.Shared.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Jarvis.Swashbuckle.Filters;

/// <summary>
/// Documents standard HTTP statuses aligned with <c>ApiResponseWrapperMiddleware</c> using <see cref="BaseResponse"/> bodies.
/// </summary>
public sealed class ProducesBaseResponseOperationFilter(IOptions<SwaggerOption> swaggerOptions) : IOperationFilter
{
    private readonly SwaggerOption _options = swaggerOptions.Value;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = "/" + (context.ApiDescription.RelativePath ?? string.Empty).TrimStart('/');
        var prefixes = _options.ApiResponseDocumentationPathPrefixes ?? [];
        if (prefixes.Length > 0 && !PathMatchesAnyPrefix(relativePath, prefixes))
            return;

        AddSuccessResponse(operation, context);
        AddErrorResponse(operation, context, "400", "Bad request; validation or deserialization failures may return `BaseResponse` with field errors.");
        AddErrorResponse(operation, context, "401", "Unauthorized.");
        AddErrorResponse(operation, context, "403", "Forbidden.");
        AddErrorResponse(operation, context, "404", "Resource not found.");
        AddErrorResponse(operation, context, "409", "Conflict.");
        AddErrorResponse(operation, context, "422",
            "**422 Unprocessable Entity** — business rule failed. Body: `application/json` matching `BaseResponse`. " +
            "**Always includes `code`** (string): machine-readable error id = `{entryAssembly}:{suffix}` from `IErrorCode`. " +
            "Also `error.message`, `error.systemMessage`; optional `data` when the exception carried a payload.");
        AddErrorResponse(operation, context, "429", "Too many requests (rate limited).");
        AddErrorResponse(operation, context, "500", "Internal server error.");
    }

    private static void AddSuccessResponse(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses.ContainsKey("200"))
            return;

        var dataType = GetActionResultDataType(context.MethodInfo);
        Type responseClrType = dataType != null
            ? typeof(BaseResponse<>).MakeGenericType(dataType)
            : typeof(BaseResponse<>).MakeGenericType(typeof(object));

        var schema = context.SchemaGenerator.GenerateSchema(responseClrType, context.SchemaRepository, memberInfo: context.MethodInfo);
        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "Success",
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["application/json"] = new() { Schema = schema }
            }
        };
    }

    private static void AddErrorResponse(OpenApiOperation operation, OperationFilterContext context, string statusCode, string description)
    {
        if (operation.Responses.ContainsKey(statusCode))
            return;

        var schema = context.SchemaGenerator.GenerateSchema(typeof(BaseResponse), context.SchemaRepository, memberInfo: context.MethodInfo);
        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["application/json"] = new() { Schema = schema }
            }
        };
    }

    private static Type? GetActionResultDataType(MethodInfo methodInfo)
    {
        var type = methodInfo.ReturnType;
        type = UnwrapTask(type);

        if (type.IsGenericType)
        {
            var def = type.GetGenericTypeDefinition();
            if (def == typeof(ActionResult<>))
                return type.GetGenericArguments()[0];
        }

        if (typeof(IActionResult).IsAssignableFrom(type) || type == typeof(ActionResult))
            return null;

        if (type == typeof(void))
            return null;

        return type;
    }

    private static bool PathMatchesAnyPrefix(string relativePath, string[] prefixes)
    {
        var path = relativePath.TrimEnd('/');
        foreach (var raw in prefixes)
        {
            var p = (raw.StartsWith('/') ? raw : "/" + raw).TrimEnd('/');
            if (path.Equals(p, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static Type UnwrapTask(Type type)
    {
        while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            type = type.GetGenericArguments()[0];
        return type;
    }
}
