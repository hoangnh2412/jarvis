using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.Services;

public class EnrichTraceService(
    IHttpContextAccessor httpContextAccessor)
    : EnrichDataService(httpContextAccessor), IEnrichTraceService
{
}