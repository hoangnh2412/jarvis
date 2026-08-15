using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Jarvis.DDD.Domain.Repositories;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/pagination-demo")]
public class PaginationDemoController : ControllerBase
{
    private readonly ITenantUnitOfWork _unitOfWork;

    public PaginationDemoController(ITenantUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// API Demo Jarvis PagedListRequest
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetListAsync([FromQuery] PagedListRequest request, CancellationToken cancellationToken)
    {
        // 1. Cấu hình bảo mật (Whitelist)
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Name", "Age", "CreatedAt", "UpdatedAt"
            // Note: Id tự động được thêm ngầm bên dưới thư viện
        };

        var options = new PagedQueryOptions<Student>
        {
            AllowedFields = allowedFields
        };

        // 2. Chuyển quyền xử lý cho Jarvis Engine
        var repo = await _unitOfWork.GetRepositoryAsync<IQueryRepository<Student>>();
        var (items, totalCount) = await repo.PaginationAsync(
            request,
            options,
            cancellationToken);

        // 3. Trả kết quả (thực tế sẽ map sang DTO, ở đây Demo trả luôn Entity)
        return Ok(new
        {
            Data = items,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }

}
