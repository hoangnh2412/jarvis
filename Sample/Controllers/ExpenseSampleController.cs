using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Sample.AuthorizationDemo;

namespace Sample.Controllers;

/// <summary>
/// Code mẫu: endpoint nghiệp vụ dùng Platform.Authorization theo 2 lớp (ADR D12, §5.11).
/// <list type="number">
/// <item>Danh sách / thống kê: <see cref="IDataScopeFilter"/> thu hẹp query trước, rồi mới lọc nghiệp vụ,
/// phân trang, projection — tất cả thành <b>một</b> câu SQL.</item>
/// <item>Thao tác trên một bản ghi: tìm trong phạm vi được xem (ngoài phạm vi → 404, không lộ sự tồn tại),
/// sau đó <see cref="IAuthorizationEngine.EvaluatePoliciesAsync"/> với permission của hành động (từ chối → 403 + lý do).</item>
/// </list>
/// User lấy từ header <c>X-Demo-User</c> (tên user demo) để dễ thử; ứng dụng thật lấy userId từ ICurrentUser / claims.
/// </summary>
[ApiController]
[Route("api/_authz-sample/expenses")]
[AllowAnonymous]
public class ExpenseSampleController(
    IAuthorizationEngine engine,
    IDataScopeFilter scopeFilter,
    SampleAuthorizationDbContext db) : ControllerBase
{
    public const string DemoUserHeader = "X-Demo-User";

    public static class Permissions
    {
        public const string View = "Expense.View";
        public const string Approve = "Expense.Approve";
    }

    public sealed record ExpenseItem(Guid Id, string Code, Guid DepartmentId, decimal Amount);

    /// <summary>Danh sách chứng từ trong phạm vi user, có lọc nghiệp vụ và phân trang.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? department,
        [FromQuery] decimal? minAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return UnknownUser();

        // Lớp 1 (RBAC): không có quyền xem → 403, phân biệt với "có quyền nhưng không có dữ liệu".
        if (!await engine.HasPermissionAsync(userId, Permissions.View, cancellationToken))
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: $"Thiếu quyền {Permissions.View}.");

        // Lớp 2 (Data Scope): thu hẹp IQueryable theo policy — chưa chạm DB.
        var query = await scopeFilter.ApplyFilterAsync(db.Expenses.AsNoTracking(), userId, Permissions.View, cancellationToken);

        // Lọc nghiệp vụ ghép tiếp vào cùng query (AND với phạm vi).
        if (department != null)
        {
            if (!SampleAuthorizationDemoData.Departments.TryGetValue(department, out var departmentId))
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Department '{department}' không tồn tại.");
            query = query.Where(e => e.DepartmentId == departmentId);
        }

        if (minAmount != null)
            query = query.Where(e => e.Amount >= minAmount);

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseItem(e.Id, e.Code, e.DepartmentId, e.Amount))
            .ToListAsync(cancellationToken);

        return Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
    }

    /// <summary>Thống kê theo phòng ban — cũng chỉ trên dữ liệu trong phạm vi.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return UnknownUser();

        var query = await scopeFilter.ApplyFilterAsync(db.Expenses.AsNoTracking(), userId, Permissions.View, cancellationToken);

        var byDepartment = await query
            .GroupBy(e => e.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count(), TotalAmount = g.Sum(e => e.Amount) })
            .ToListAsync(cancellationToken);

        return Ok(byDepartment);
    }

    /// <summary>Chi tiết: ngoài phạm vi xem → 404 (không cho biết bản ghi có tồn tại).</summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return UnknownUser();

        var expense = await FindVisibleAsync(userId, code, cancellationToken);
        return expense is null ? NotFound() : Ok(expense);
    }

    /// <summary>
    /// Duyệt: bản ghi phải nằm trong phạm vi xem, rồi engine đánh giá policy của <c>Expense.Approve</c>
    /// trên chính bản ghi đó (hạn mức, cùng phòng ban…).
    /// </summary>
    [HttpPost("{code}/approve")]
    public async Task<IActionResult> Approve(string code, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return UnknownUser();

        var expense = await FindVisibleAsync(userId, code, cancellationToken);
        if (expense is null)
            return NotFound();

        var decision = await engine.EvaluatePoliciesAsync(userId, Permissions.Approve, expense, cancellationToken);
        if (!decision.IsAllowed)
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: decision.Reason);

        // Demo: không ghi trạng thái. Ứng dụng thật cập nhật Status / ApprovedBy rồi SaveChanges.
        return Ok(new { expense.Code, Approved = true, decision.PolicyCode });
    }

    private async Task<SampleExpenseRow?> FindVisibleAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var visible = await scopeFilter.ApplyFilterAsync(db.Expenses.AsNoTracking(), userId, Permissions.View, cancellationToken);
        return await visible.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var user = SampleAuthorizationDemoData.FindUser(Request.Headers[DemoUserHeader].ToString());
        userId = user?.Id ?? Guid.Empty;
        return user != null;
    }

    private ObjectResult UnknownUser() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            detail: $"Header {DemoUserHeader} phải là một user demo: {string.Join(", ", SampleAuthorizationDemoData.Users.Select(u => u.UserName))}.");
}
