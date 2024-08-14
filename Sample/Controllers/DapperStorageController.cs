using Dapper;
using Microsoft.AspNetCore.Mvc;
using Sample.Application.DTOs;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("dapper")]
public class DapperStorageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public DapperStorageController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("basic")]
    public async Task<IActionResult> BasicAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var items = await uow.GetConnection().QueryAsync<User>("SELECT * FROM users");
        return Ok(items);
    }

    [HttpGet("advance")]
    public async Task<IActionResult> AdvanceAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var items = await uow.GetConnection().QueryAsync<UserTenantDto>("select u.\"Id\", u.\"Name\", u.\"Age\", t.id as TenantId, t.connectionstring from users u join tenants t on u.TenantId = t.Id where u.tenantid = @TenantId", new
        {
            TenantId = Guid.Parse("f55ba439-9cc0-43fe-ba4f-0939463e6b76"),
            TenantName = "tenant2"
        });
        return Ok(items);
    }
}