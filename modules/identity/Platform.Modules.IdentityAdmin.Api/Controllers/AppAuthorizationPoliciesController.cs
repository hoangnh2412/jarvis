using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Modules.IdentityAdmin.Abstractions;

namespace Platform.Modules.IdentityAdmin.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/app/authorization/policies")]
public sealed class AppAuthorizationPoliciesController(IAuthorizationPolicyAdminService policies) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await policies.ListPoliciesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("operands")]
    public IActionResult Operands() => Ok(policies.ListOperands());

    [HttpGet("operators")]
    public IActionResult Operators() => Ok(policies.ListOperators());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await policies.GetPolicyAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AuthorizationPolicyWriteDto input, CancellationToken cancellationToken)
    {
        var created = await policies.CreatePolicyAsync(input, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AuthorizationPolicyWriteDto input, CancellationToken cancellationToken)
    {
        var updated = await policies.UpdatePolicyAsync(id, input, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await policies.DeletePolicyAsync(id, cancellationToken);
        return Ok();
    }
}
