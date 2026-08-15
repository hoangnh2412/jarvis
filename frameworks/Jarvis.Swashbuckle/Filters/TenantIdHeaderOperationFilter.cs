using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Jarvis.Swashbuckle.Filters;

/// <summary>
/// Thêm header tenant vào mỗi operation trên Swagger UI (mặc định <c>X-Tenant-Id</c>).
/// </summary>
public class TenantIdHeaderOperationFilter : IOperationFilter
{
    public const string DefaultHeaderName = "X-Tenant-Id";

    private readonly string _headerName;

    public TenantIdHeaderOperationFilter()
        : this(DefaultHeaderName)
    {
    }

    public TenantIdHeaderOperationFilter(string headerName)
    {
        _headerName = string.IsNullOrWhiteSpace(headerName)
            ? DefaultHeaderName
            : headerName.Trim();
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        if (operation.Parameters.Any(p =>
                p.In == ParameterLocation.Header &&
                string.Equals(p.Name, _headerName, StringComparison.OrdinalIgnoreCase)))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _headerName,
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            Description = $"Tenant id (Guid). Đọc bởi HeaderTenantIdResolver; mặc định header `{_headerName}`.",
        });
    }
}
