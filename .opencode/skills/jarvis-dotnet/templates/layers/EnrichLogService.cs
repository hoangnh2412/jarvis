using Jarvis.Domain.Services;
using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;

namespace {Product}.Host.Services;

public sealed class EnrichLogService(IHttpContextAccessor httpContextAccessor)
  : EnrichDataService(httpContextAccessor), IEnrichLogService;
