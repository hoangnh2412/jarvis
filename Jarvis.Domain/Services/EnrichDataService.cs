using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Domain.Services;

public abstract class EnrichDataService(
    IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Task<Dictionary<string, string>> ExtractAsync()
    {
        var data = new Dictionary<string, string>();

        var userId = Guid.Empty;
        var userName = "anonymous";
        var tenantId = Guid.Empty;

        var workContext = _httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<IWorkContext>();

        if (workContext != null)
        {
            userId = workContext.GetUserId() ?? Guid.Empty;
            userName = string.IsNullOrEmpty(workContext.GetUserName()) ? "anonymous" : workContext.GetUserName()!.ToString();
            tenantId = workContext.GetTenantId() ?? Guid.Empty;
        }

        data.Add(UserAttributes.Id, userId.ToString());
        data.Add(UserAttributes.UserName, userName);
        data.Add(UserAttributes.TenantId, tenantId.ToString());

        return Task.FromResult(data);
    }
}