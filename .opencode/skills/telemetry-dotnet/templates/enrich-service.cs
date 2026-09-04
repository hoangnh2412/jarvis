// EnrichTraceService.cs / EnrichLogService.cs — mẫu enrich từ HttpContext
// Jarvis.Domain có EnrichDataService base; hoặc implement trực tiếp IEnrich*.

using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.AspNetCore.Http;

namespace {App}.Services;

public sealed class EnrichTraceService(IHttpContextAccessor httpContextAccessor) : IEnrichTraceService
{
    public Task<Dictionary<string, string>> ExtractAsync()
    {
        var data = new Dictionary<string, string>();
        // Thêm tag ổn định, tránh high-cardinality tùy tiện
        // data.Add(UserAttributes.Id, userId);
        return Task.FromResult(data);
    }
}

public sealed class EnrichLogService(IHttpContextAccessor httpContextAccessor) : IEnrichLogService
{
    public Task<Dictionary<string, string>> ExtractAsync()
    {
        return Task.FromResult(new Dictionary<string, string>());
    }
}

// Đăng ký trong AddJarvisOpenTelemetry callback:
// services.AddScoped<IEnrichTraceService, EnrichTraceService>();
// services.AddScoped<IEnrichLogService, EnrichLogService>();
