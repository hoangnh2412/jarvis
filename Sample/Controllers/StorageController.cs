using Asp.Versioning;
using Jarvis.Domain.Repositories;
using Jarvis.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Controllers;

[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/storage")]
[ApiController]
public class StorageController(ISampleUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var repo = await uow.GetRepositoryAsync<IRepository<Student>>().ConfigureAwait(false);
        var items = await repo.GetQuery().ToListAsync(cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] Student student,
        CancellationToken cancellationToken)
    {
        var repo = await uow.GetRepositoryAsync<IRepository<Student>>().ConfigureAwait(false);
        await repo.InsertAsync(new Student
        {
            Id = Guid.NewGuid(),
            Name = StringExtension.GenerateRandom(10, true, true, true, true),
            Age = Random.Shared.Next(1, 100),
        });
        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Ok();
    }
}