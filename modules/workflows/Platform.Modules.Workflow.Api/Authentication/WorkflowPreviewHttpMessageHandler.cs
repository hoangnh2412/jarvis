using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Platform.Modules.ElsaWorkflow.Api.Authentication;

public sealed class WorkflowPreviewHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            CopyHeader(httpContext.Request.Headers.Authorization, request.Headers, "Authorization");
            CopyHeader(httpContext.Request.Headers.Cookie, request.Headers, "Cookie");
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static void CopyHeader(
        string? value,
        HttpHeaders headers,
        string name)
    {
        if (!string.IsNullOrWhiteSpace(value) && !headers.Contains(name))
            headers.TryAddWithoutValidation(name, value);
    }
}
