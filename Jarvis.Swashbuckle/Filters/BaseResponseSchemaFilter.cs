using System.Linq;
using System.Reflection;
using System.Text;
using Jarvis.Domain.Shared.ExceptionHandling;
using Jarvis.Domain.Shared.RequestResponse;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Jarvis.Swashbuckle.Filters;

/// <summary>
/// Enriches OpenAPI schemas for <see cref="BaseResponse"/> / <see cref="BaseResponse{T}"/> (e.g. <c>Code</c> pattern and known error codes).
/// </summary>
public sealed class BaseResponseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type == typeof(BaseResponse))
        {
            schema.Description = BuildEnvelopeDescription();
            AppendCodeDocumentation(schema);
            return;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseResponse<>))
        {
            schema.Description = BuildEnvelopeDescription();
            AppendCodeDocumentation(schema);
        }
    }

    private static string BuildEnvelopeDescription() =>
        "Standard JSON envelope for API responses. " +
        "When the server returns HTTP 422 (Unprocessable Entity), the body uses this shape: the **code** property is always set to the application error identifier so clients can branch on it programmatically. " +
        "Format is `{entryAssembly}:{suffix}` where `suffix` matches `IErrorCode` constants.";

    private static void AppendCodeDocumentation(OpenApiSchema schema)
    {
        var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "{AssemblyName}";
        var catalog = ErrorCodeHelper.GetErrorCodeCatalog();
        OpenApiSchema? codeProperty = null;
        if (schema.Properties != null)
        {
            if (!schema.Properties.TryGetValue("code", out codeProperty))
                schema.Properties.TryGetValue("Code", out codeProperty);
        }

        if (codeProperty == null)
            return;

        codeProperty.Example = new OpenApiString($"{entryAssemblyName}:99999");

        var sb = new StringBuilder();
        sb.AppendLine("**Application error id** — always read this on HTTP **422** (and other error statuses) to know which business rule failed.");
        sb.AppendLine();
        sb.AppendLine(codeProperty.Description);
        sb.AppendLine();
        sb.AppendLine($"Runtime format: `{entryAssemblyName}:{{suffix}}` where suffix comes from error constants (see `BaseResponse.GenerateCode`).");
        sb.AppendLine();
        sb.AppendLine("Known shared suffix values (message from `IErrorCode`):");
        foreach (var pair in catalog.OrderBy(p => p.Key, StringComparer.Ordinal))
            sb.AppendLine($"- **{pair.Key}**: {pair.Value}");

        codeProperty.Description = sb.ToString().TrimEnd();
    }
}
