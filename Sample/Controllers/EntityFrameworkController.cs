using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Repositories;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Controllers;

/// <summary>
/// Sample CRUD against Master <c>Tenant</c> via <c>Jarvis.ORM.EntityFramework</c>
/// (<see cref="IMasterUnitOfWork"/> + <see cref="IRepository{TEntity}"/>).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/orm/ef/tenants")]
[ApiVersion("1.0")]
public class EntityFrameworkController(IMasterUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var repo = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken)
            .ConfigureAwait(false);
        var items = await repo.GetQuery()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken)
            .ConfigureAwait(false);
        var item = await repo.GetByIdAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] TenantWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
            return BadRequest("ConnectionString is required.");

        var repo = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken)
            .ConfigureAwait(false);

        var tenant = await repo.InsertAsync(new Tenant
        {
            Id = request.Id ?? Guid.NewGuid(),
            ConnectionString = request.ConnectionString.Trim()
        }, cancellationToken).ConfigureAwait(false);

        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = tenant.Id, version = "1.0" }, tenant);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] TenantWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
            return BadRequest("ConnectionString is required.");

        var repo = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken)
            .ConfigureAwait(false);
        var tenant = await repo.GetByIdAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
            return NotFound();

        tenant.ConnectionString = request.ConnectionString.Trim();
        await repo.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(tenant);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await uow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken)
            .ConfigureAwait(false);
        var tenant = await repo.GetByIdAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
            return NotFound();

        await repo.DeleteAsync(tenant, cancellationToken).ConfigureAwait(false);
        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}
