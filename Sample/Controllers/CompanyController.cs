using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.Repositories;
using Sample.Entities;
using Sample.Persistence;
using Asp.Versioning;

namespace Sample.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/company")]
[ApiVersion("1.0")]
public class CompanyController(TenantDbContext dbContext) : ControllerBase
{
    private readonly TenantDbContext _dbContext = dbContext;
    
    /// <summary>
    /// API 1: Basic Pipeline (Chỉ lấy Employee đang Active, cấm filter các cột nhạy cảm)
    /// </summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] PagedListRequest request)
    {
        var options = new PagedQueryOptions<Employee>
        {
            // Allowed: Danh sách các cột được phép query, sort, select (Whitelist)
            AllowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "Id", "FullName", "IsActive", "BaseSalary",
                "Department", "Position", "DepartmentId", "PositionId"
            },
            // Denied: Chặn tuyệt đối không cho query, sort, select các cột nhạy cảm này
            DeniedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "IdentityNumber", "BankAccountNumber", "PasswordHash" },
        };

        // Static scope trên IQueryable (RLS / business rules) — không dùng PagedQueryOptions.Predicate
        var query = _dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => e.IsActive == true)
            .Where(e => e.BaseSalary > 0
                     && e.Position.Level >= 2
                     && (e.FullName.StartsWith("Nguyễn") || e.FullName.Contains("Trần"))
                     && ((e.BaseSalary * 1.5m) < 20000m))
            .OrderBy(e => e.Id);

        var (items, total) = await PagedListExecutor.ExecuteAsync(query, request, options);

        return Ok(new { Total = total, Items = items });
    }

    /// <summary>
    /// API 3: Custom Filter and Sort (Tìm nhân viên theo tên phòng ban và sắp xếp theo chức vụ)
    /// Thử nhập Filter = "IT" trên Swagger để xem kết quả.
    /// </summary>
    [HttpGet("employees/list-custom")]
    public async Task<IActionResult> GetEmployeesCustomList(
        [FromQuery] PagedListRequest request, 
        [FromQuery] Guid? currentEmployeeId)
    {
        // 1. Phân tích quyền lợi từ Employee đang đăng nhập
        Employee? currentUser = null;
        if (currentEmployeeId.HasValue)
        {
            currentUser = await _dbContext.Employees
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == currentEmployeeId.Value);
        }

        var query = _dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .AsQueryable();

        // RLS trên IQueryable (bảo mật cứng, không bị CustomFilter override)
        if (currentUser != null)
        {
            if (currentUser.Position.Level >= 3)
                query = query.Where(e => e.DepartmentId == currentUser.DepartmentId);
            else
                query = query.Where(e => e.Id == currentUser.Id);
        }
        else if (currentEmployeeId.HasValue)
        {
            query = query.Where(e => false);
        }

        var options = new PagedQueryOptions<Employee>
        {
            AllowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "Id", "FullName", "IsActive", "BaseSalary",
                "Department", "Department.Name",
                "Position", "Position.Title", "Position.Level"
            },
            
            // Tùy biến bộ lọc (Override):
            CustomFilter = q => 
            {
                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    // Lớp chặn tùy biến: Lương cơ bản (BaseSalary) nằm trong Whitelist nhưng bị cấm filter tại endpoint này
                    if (request.Filter.Contains("BaseSalary", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("Filtering by 'BaseSalary' is not allowed on this custom endpoint.");
                    }
                    
                    // Tìm kiếm đa từ khóa (Multi-term Global Search):
                    // Cho phép gõ tìm kiếm kết hợp, ví dụ "IT Developer" hoặc "Employee 1 Manager"
                    var terms = request.Filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in terms)
                    {
                        q = q.Where(e => 
                            e.Department.Name.Contains(term) || 
                            e.FullName.Contains(term) || 
                            e.Position.Title.Contains(term));
                    }
                    return q;
                }
                return q;
            },
            // Tùy biến sắp xếp: Level cao nhất (Manager) lên đầu, tiếp đến là lương
            CustomSort = q => q
                .OrderByDescending(e => e.Position.Level)
                .ThenByDescending(e => e.BaseSalary)
        };

        var (items, total) = await PagedListExecutor.ExecuteAsync(query, request, options);

        return Ok(new { Total = total, Items = items });
    }
}
