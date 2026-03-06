
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.Services;

public class WorkContext(
    IHttpContextAccessor httpContextAccessor) : IWorkContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? GetTenantId()
    {
        return Guid.NewGuid();
    }

    public Guid? GetTokenId()
    {
        return Guid.NewGuid();
    }

    public Guid? GetUserId()
    {
        return Guid.NewGuid();
    }

    public string? GetUserName()
    {
        return "sample_user";
    }
}