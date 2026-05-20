using Jarvis.Caching;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("api/cache-demo")]
public class CacheDemoController(ICacheService cacheService) : ControllerBase
{
    [HttpGet("product/{tenantId}/{id}")]
    public async Task<IActionResult> GetProductAsync(string tenantId, string id, CancellationToken cancellationToken)
    {
        var param = CacheParam.Create("ProductDemo")
            .WithParam("tenantId", tenantId)
            .WithParam("id", id);

        var value = await cacheService.GetOrSetAsync(param, async ct =>
        {
            await Task.Delay(10, ct).ConfigureAwait(false);
            return new ProductCacheDto(tenantId, id, $"Product-{id}@{tenantId}", DateTimeOffset.UtcNow);
        }, cancellationToken).ConfigureAwait(false);

        return Ok(value);
    }

    [HttpDelete("product/{tenantId}/{id}")]
    public async Task<IActionResult> InvalidateProductAsync(string tenantId, string id, CancellationToken cancellationToken)
    {
        var param = CacheParam.Create("ProductDemo")
            .WithParam("tenantId", tenantId)
            .WithParam("id", id);

        await cacheService.RemoveAsync(param, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    public sealed record ProductCacheDto(string TenantId, string Id, string Name, DateTimeOffset LoadedAt);
}
