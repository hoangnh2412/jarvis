using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.Services;

public class EnrichLogService(
    IHttpContextAccessor httpContextAccessor)
    : EnrichDataService(httpContextAccessor), IEnrichLogService
{
}