using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.Services;

public class EnrichTraceService(
    IHttpContextAccessor httpContextAccessor)
    : EnrichDataService(httpContextAccessor), IEnrichTraceService
{
}