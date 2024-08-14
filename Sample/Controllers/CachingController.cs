using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.Caching;
using Jarvis.Persistence.Caching.Interfaces;
using Jarvis.Persistence.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("caching")]
public class CachingController : ControllerBase
{
    [HttpGet("single")]
    public async Task<IActionResult> SingleAsync(
        [FromServices] IMultiCachingService cacheService,
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var user = await cacheService.GetAsync<User>("Auth", async () =>
        {
            var repo = uow.GetRepository<IEFRepository<User>>();
            return await repo.GetQuery().FirstOrDefaultAsync(x => x.Name == "sample");
        });
        return Ok(user);
    }

    [HttpGet("multithread")]
    public async Task<IActionResult> MultitrheadAsync(
        [FromServices] IMultiCachingService cacheService,
        [FromServices] ISampleUnitOfWork uow,
        [FromServices] IServiceScopeFactory serviceScopeProvider
    )
    {
        var user = await cacheService.GetAsync<User>("Auth", async () =>
        {
            var repo = uow.GetRepository<IEFRepository<User>>();
            return await repo.GetQuery().FirstOrDefaultAsync(x => x.Name == "sample");
        });

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using (var scope = serviceScopeProvider.CreateScope())
                {
                    var uowSamle = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
                    var multiCachingService = scope.ServiceProvider.GetService<IMultiCachingService>();

                    var user = await multiCachingService.GetAsync<User>("Auth", async () =>
                    {
                        var repo = uowSamle.GetRepository<IEFRepository<User>>();
                        return await repo.GetQuery().FirstOrDefaultAsync(x => x.Name == "sample");
                    });
                }
            }));
        }
        await Task.WhenAll(tasks);

        return Ok(user);
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckAsync(
        [FromServices] IEnumerable<ICachingService> cacheServices,
        [FromServices] IOptions<CacheOption> cacheOption
    )
    {
        var memoryCacheService = (MemoryCacheService)cacheServices.FirstOrDefault(x => x.GetType().AssemblyQualifiedName == typeof(MemoryCacheService).AssemblyQualifiedName);
        var userMemory = await memoryCacheService.GetAsync<User>("Auth");

        var entry = cacheOption.Value.Entries.FirstOrDefault(x => x.Key == "Auth");
        var distributedCacheService = cacheServices.FirstOrDefault(x => x.GetType().AssemblyQualifiedName != typeof(MemoryCacheService).AssemblyQualifiedName && x.Name == entry.StorageLocation);
        var userDist = await distributedCacheService.GetAsync<User>("Auth");

        return Ok(new
        {
            Memory = userMemory,
            Redis = userDist
        });
    }
}