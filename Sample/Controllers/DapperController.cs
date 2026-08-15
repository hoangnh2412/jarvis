using System.Data.Common;
using Asp.Versioning;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Jarvis.ORM.Dapper;
using Sample.Entities;

namespace Sample.Controllers;

/// <summary>
/// Sample CRUD against Master <c>Tenant</c> via <c>Jarvis.ORM.Dapper</c> (<see cref="ISqlConnectionFactory"/>).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/orm/dapper/tenants")]
[ApiVersion("1.0")]
public class DapperController(ISqlConnectionFactory connections) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var items = await connection.QueryAsync<Tenant>(new CommandDefinition(
            """
            SELECT "Id", "ConnectionString"
            FROM "Tenant"
            ORDER BY "Id"
            """,
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var item = await connection.QuerySingleOrDefaultAsync<Tenant>(new CommandDefinition(
            """
            SELECT "Id", "ConnectionString"
            FROM "Tenant"
            WHERE "Id" = @Id
            """,
            new { Id = id },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] TenantWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionString))
            return BadRequest("ConnectionString is required.");

        var tenant = new Tenant
        {
            Id = request.Id ?? Guid.NewGuid(),
            ConnectionString = request.ConnectionString.Trim()
        };

        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO "Tenant" ("Id", "ConnectionString")
            VALUES (@Id, @ConnectionString)
            """,
            tenant,
            cancellationToken: cancellationToken)).ConfigureAwait(false);

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

        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE "Tenant"
            SET "ConnectionString" = @ConnectionString
            WHERE "Id" = @Id
            """,
            new { Id = id, ConnectionString = request.ConnectionString.Trim() },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        if (affected == 0)
            return NotFound();

        return Ok(new Tenant { Id = id, ConnectionString = request.ConnectionString.Trim() });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            """
            DELETE FROM "Tenant"
            WHERE "Id" = @Id
            """,
            new { Id = id },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return affected == 0 ? NotFound() : NoContent();
    }

    private async Task<DbConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = await connections.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection as DbConnection
            ?? throw new InvalidOperationException(
                "ISqlConnectionFactory must return a DbConnection for async disposal.");
    }
}

public sealed record TenantWriteRequest(string ConnectionString, Guid? Id = null);
