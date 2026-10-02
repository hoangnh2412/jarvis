using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Operands;
using Platform.Authorization.Services;
using Sample.AuthorizationDemo;

namespace Sample.Controllers;

/// <summary>
/// Kiểm thử trực tiếp Platform.Authorization trên Postgres thật (database <c>SampleAuthorization</c>).
/// Chỉ phục vụ demo: user được chọn theo tên trong request, không đi qua xác thực.
/// </summary>
[ApiController]
[Route("api/_authz-demo")]
[AllowAnonymous]
public class AuthorizationDemoController(
    IAuthorizationEngine engine,
    IOperandRegistry operands,
    OperandValidator validator,
    IDataScopeFilter scopeFilter,
    SampleAuthorizationDbContext store,
    IServiceProvider services) : ControllerBase
{
    /// <summary>
    /// Data Scope: danh sách chứng từ user được thấy với <paramref name="permission"/> — lọc bằng SQL WHERE
    /// sinh từ policy; trả kèm câu SQL để kiểm tra.
    /// </summary>
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] string user,
        [FromQuery] string permission = "Expense.View",
        CancellationToken cancellationToken = default)
    {
        var demoUser = SampleAuthorizationDemoData.FindUser(user);
        if (demoUser is null)
            return BadRequest(new { Error = $"Không có user demo '{user}'.", Users = SampleAuthorizationDemoData.Users.Select(u => u.UserName) });

        var query = (await scopeFilter.ApplyFilterAsync(store.Expenses.AsNoTracking(), demoUser.Id, permission, cancellationToken))
            .OrderBy(e => e.Code);
        var rows = await query.ToListAsync(cancellationToken);

        return Ok(new
        {
            User = demoUser.UserName,
            Permission = permission,
            Sql = query.ToQueryString(),
            Rows = rows.Select(Describe),
        });
    }

    /// <summary>
    /// Kiểm tra chéo: với mọi user × permission, danh sách lọc bằng SQL phải trùng với quyết định của engine
    /// trên từng chứng từ.
    /// </summary>
    [HttpGet("scope-check")]
    public async Task<IActionResult> CheckScope(CancellationToken cancellationToken)
    {
        var all = await store.Expenses.AsNoTracking().OrderBy(e => e.Code).ToListAsync(cancellationToken);
        var results = new List<ScopeCheckResult>();

        foreach (var user in SampleAuthorizationDemoData.Users)
        {
            foreach (var permission in SampleAuthorizationDemoData.ScopePermissions)
            {
                var scoped = await (await scopeFilter.ApplyFilterAsync(store.Expenses.AsNoTracking(), user.Id, permission, cancellationToken))
                    .OrderBy(e => e.Code)
                    .Select(e => e.Code)
                    .ToListAsync(cancellationToken);

                var allowed = new List<string>();
                foreach (var row in all)
                {
                    if (await engine.AuthorizeAsync(user.Id, permission, row, cancellationToken))
                        allowed.Add(row.Code);
                }

                results.Add(new ScopeCheckResult(user.UserName, permission, scoped, allowed, scoped.SequenceEqual(allowed)));
            }
        }

        var passed = results.Count(r => r.Pass);
        return Ok(new { Total = results.Count, Passed = passed, Failed = results.Count - passed, Results = results });
    }

    private static object Describe(SampleExpenseRow row) => new
    {
        row.Code,
        Tenant = SampleAuthorizationDemoData.Tenants.First(t => t.Value == row.TenantId).Key,
        Department = SampleAuthorizationDemoData.Departments.First(d => d.Value == row.DepartmentId).Key,
        row.Amount,
        CreatedBy = SampleAuthorizationDemoData.Users.FirstOrDefault(u => u.Id == row.CreatedBy)?.UserName,
    };

    /// <summary>Operand được phép trong rule: mặc định platform + operand Sample đăng ký.</summary>
    [HttpGet("operands")]
    public IActionResult GetOperands() =>
        Ok(operands.All
            .OrderBy(o => o.Path, StringComparer.Ordinal)
            .Select(o => new { o.Path, Source = o.Source.ToString(), Kind = o.Kind.ToString(), o.ClaimType }));

    /// <summary>Role, policy (kèm rule) và role claims đang có trong store.</summary>
    [HttpGet("store")]
    public async Task<IActionResult> GetStore(CancellationToken cancellationToken)
    {
        var claims = await store.RoleClaims.ToListAsync(cancellationToken);
        var roles = await store.Roles.OrderByDescending(r => r.RoleLevel).ToListAsync(cancellationToken);
        var policies = await store.Policies.Include(p => p.Rules).OrderBy(p => p.Code).ToListAsync(cancellationToken);

        return Ok(new
        {
            Roles = roles.Select(r => new
            {
                r.Name,
                r.RoleLevel,
                r.IsSystemRole,
                Permissions = claims.Where(c => c.RoleId == r.Id && c.ClaimType == SampleAuthorizationDemoData.PermissionClaim).Select(c => c.ClaimValue),
                Policies = claims.Where(c => c.RoleId == r.Id && c.ClaimType == SampleAuthorizationDemoData.PolicyClaim).Select(c => c.ClaimValue),
            }),
            Policies = policies.Select(p => new
            {
                p.Code,
                p.Name,
                p.IsSystemPolicy,
                Rules = p.Rules.OrderBy(r => r.Order).Select(r => $"{r.Order}: {r.LeftOperand} {r.Operator} {r.RightOperand}"),
            }),
        });
    }

    /// <summary>User demo cùng tenant, role và claims.</summary>
    [HttpGet("users")]
    public IActionResult GetUsers() =>
        Ok(SampleAuthorizationDemoData.Users.Select(u => new { u.UserName, u.Tenant, u.Claims }));

    /// <summary>Chạy toàn bộ kịch bản AZ-01..AZ-13, so sánh kết quả thực tế với kỳ vọng.</summary>
    [HttpGet("scenarios")]
    public async Task<IActionResult> RunScenarios(CancellationToken cancellationToken)
    {
        var results = new List<ScenarioResult>();
        foreach (var s in SampleAuthorizationDemoData.Scenarios)
        {
            var request = new CheckRequest(s.User, s.Permission, s.Tenant, s.Department, s.Amount, s.CreatedBy);
            var outcome = await EvaluateAsync(request, cancellationToken);
            results.Add(new ScenarioResult(
                s.Id,
                s.Description,
                request,
                s.ExpectedAllowed,
                outcome.Allowed,
                outcome.Decision.PolicyCode,
                outcome.Decision.Reason,
                outcome.Allowed == s.ExpectedAllowed));
        }

        var passed = results.Count(r => r.Pass);
        return Ok(new { Total = results.Count, Passed = passed, Failed = results.Count - passed, Results = results });
    }

    /// <summary>Kiểm tra một yêu cầu tùy ý: user + permission + chứng từ chi phí.</summary>
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckRequest request, CancellationToken cancellationToken)
    {
        if (SampleAuthorizationDemoData.FindUser(request.User) is null)
            return BadRequest(new { Error = $"Không có user demo '{request.User}'.", Users = SampleAuthorizationDemoData.Users.Select(u => u.UserName) });
        if (!SampleAuthorizationDemoData.Tenants.ContainsKey(request.Tenant) || !SampleAuthorizationDemoData.Departments.ContainsKey(request.Department))
            return BadRequest(new
            {
                Error = "Tenant hoặc Department không hợp lệ.",
                Tenants = SampleAuthorizationDemoData.Tenants.Keys,
                Departments = SampleAuthorizationDemoData.Departments.Keys,
            });

        var outcome = await EvaluateAsync(request, cancellationToken);
        return Ok(new
        {
            Request = request,
            outcome.HasPermission,
            outcome.Allowed,
            outcome.Decision.PolicyCode,
            outcome.Decision.Reason,
        });
    }

    /// <summary>Kiểm tra một rule trước khi lưu (operand đã đăng ký, toán tử hợp lệ, IN cần collection).</summary>
    [HttpPost("rules/validate")]
    public IActionResult ValidateRule([FromBody] RuleRequest rule)
    {
        var result = validator.ValidateRule(rule.Left, rule.Operator, rule.Right);
        return Ok(new { Rule = rule, result.IsValid, result.ErrorMessage });
    }

    /// <summary>Xóa và tạo lại database demo, seed lại từ đầu.</summary>
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await SampleAuthorizationExtensions.SeedAsync(services, recreate: true, cancellationToken);
        return Ok(new
        {
            Roles = await store.Roles.CountAsync(cancellationToken),
            Policies = await store.Policies.CountAsync(cancellationToken),
            Users = await store.Users.CountAsync(cancellationToken),
        });
    }

    private async Task<(bool HasPermission, bool Allowed, AccessDecision Decision)> EvaluateAsync(
        CheckRequest request,
        CancellationToken cancellationToken)
    {
        var user = SampleAuthorizationDemoData.FindUser(request.User)!;
        var creator = SampleAuthorizationDemoData.FindUser(request.CreatedBy)?.Id ?? Guid.Empty;
        var expense = new SampleExpense(
            SampleAuthorizationDemoData.Tenants[request.Tenant],
            SampleAuthorizationDemoData.Departments[request.Department],
            request.Amount,
            creator);

        var hasPermission = await engine.HasPermissionAsync(user.Id, request.Permission, cancellationToken);
        var decision = await engine.EvaluatePoliciesAsync(user.Id, request.Permission, expense, cancellationToken);
        var allowed = await engine.AuthorizeAsync(user.Id, request.Permission, expense, cancellationToken);
        return (hasPermission, allowed, decision);
    }

    public sealed record CheckRequest(string User, string Permission, string Tenant, string Department, decimal Amount, string CreatedBy);

    public sealed record RuleRequest(string Left, string Operator, string Right);

    public sealed record ScopeCheckResult(
        string User,
        string Permission,
        IReadOnlyList<string> Scoped,
        IReadOnlyList<string> EngineAllowed,
        bool Pass);

    public sealed record ScenarioResult(
        string Id,
        string Description,
        CheckRequest Request,
        bool ExpectedAllowed,
        bool Allowed,
        string? PolicyCode,
        string? Reason,
        bool Pass);
}
