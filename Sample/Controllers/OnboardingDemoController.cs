using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;
using Platform.Workflow.Abstractions;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/onboarding")]
[ApiVersion("1.0")]
public class OnboardingDemoController(
    IMasterUnitOfWork uow,
    MasterDbContext dbContext,
    IWorkflowService? workflowService = null,
    IWorkflowDefinitionService? workflowDefinitionService = null,
    IWorkflowInstanceService? workflowInstanceService = null) : ControllerBase
{
    public const string ProcessCode = "ONBOARDING_PROCESS";
    public const string SharedWorkflowDefinitionId = "36456fc5c91084b7";
    public const string SharedWorkflowName = "Quy trình Onboarding Nhân sự";

    private static readonly (int Order, string StepCode, string Title, string Department, string Description, string AssignedTo)[] DefaultSeedData =
    [
        (1, "HR_HANDOVER_DEVICE", "HR bàn giao thiết bị làm việc", "HR", "Kiểm tra danh sách và bàn giao máy tính, phụ kiện, thẻ ra vào cho nhân sự mới.", "Nguyễn Thị HR"),
        (2, "IT_CREATE_AD", "IT admin tạo và bàn giao tài khoản AD", "IT Admin", "Khởi tạo tài khoản Windows/Active Directory, email công ty và cấp quyền cơ bản.", "Trần IT Admin"),
        (3, "IT_CREATE_JIRA", "IT admin tạo và bàn giao tài khoản Jira", "IT Admin", "Cấp tài khoản Jira và thêm nhân viên vào dự án tương ứng.", "Trần IT Admin"),
        (4, "IT_CREATE_CONFLUENCE", "IT admin tạo và bàn giao tài khoản Confluence", "IT Admin", "Cấp quyền truy cập hệ thống wiki tài liệu nội bộ Confluence.", "Trần IT Admin"),
        (5, "IT_CREATE_BITBUCKET", "IT admin tạo và bàn giao tài khoản Bitbucket", "IT Admin", "Cấu hình tài khoản Bitbucket, quyền truy cập git repositories.", "Trần IT Admin"),
        (6, "HR_SIGN_PROBATION_CONTRACT", "HR ký hợp đồng thử việc", "HR", "Ký kết thỏa thuận bảo mật và hợp đồng lao động thử việc với nhân sự mới.", "Nguyễn Thị HR"),
        (7, "EMPLOYEE_CONFIRM_ONBOARD", "Nhân sự mới xác nhận đã onboard", "Nhân sự mới", "Nhân sự kiểm tra và bấm xác nhận hoàn tất các bước tiếp nhận công việc.", "Nhân sự mới")
    ];

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        var existingUsers = await dbContext.AppUsers.ToListAsync(cancellationToken).ConfigureAwait(false);
        var existingSteps = await dbContext.OnboardingSteps
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingMappings = await dbContext.BusinessWorkflowMappings
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        if (existingUsers.Count == 0)
        {
            var defaultUser = new AppUser
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Username = "nguyenvana",
                FullName = "Nguyễn Văn A",
                Email = "nguyenvana@lexora.vn",
                Department = "Kỹ thuật",
                Position = "Backend Software Engineer",
                IsActive = true,
                CreatedAt = now
            };
            dbContext.AppUsers.Add(defaultUser);
            existingUsers.Add(defaultUser);
        }

        var primaryUser = existingUsers.First();

        // Gán UserId cho các steps cũ nếu chưa có
        foreach (var oldStep in existingSteps.Where(s => s.UserId == null))
        {
            oldStep.UserId = primaryUser.Id;
        }

        if (existingSteps.Count == 0)
        {
            foreach (var item in DefaultSeedData)
            {
                var step = new OnboardingStep
                {
                    Id = Guid.NewGuid(),
                    UserId = primaryUser.Id,
                    StepCode = item.StepCode,
                    Title = item.Title,
                    Department = item.Department,
                    Description = item.Description,
                    Order = item.Order,
                    Status = item.Order == 1 ? "InProgress" : "Pending",
                    AssignedTo = item.AssignedTo,
                    CreatedAt = now
                };
                dbContext.OnboardingSteps.Add(step);
            }
        }

        // Mỗi user có 1 workflow onboarding
        BusinessWorkflowMapping? processMapping = existingMappings.FirstOrDefault(m =>
            m.UserId == primaryUser.Id ||
            (m.UserId == null && (m.StepCode == ProcessCode || m.WorkflowDefinitionId == SharedWorkflowDefinitionId)));

        if (processMapping == null)
        {
            processMapping = new BusinessWorkflowMapping
            {
                Id = Guid.NewGuid(),
                UserId = primaryUser.Id,
                StepId = null,
                StepCode = ProcessCode,
                WorkflowDefinitionId = SharedWorkflowDefinitionId,
                InstanceId = Guid.NewGuid().ToString("N")[..16], // ParentWorkflowInstanceId
                WorkflowName = SharedWorkflowName,
                Description = $"Quy trình Onboarding Nhân sự cho nhân viên: {primaryUser.FullName} ({primaryUser.Username})",
                WorkflowStatus = "Running",
                IsActive = true,
                CreatedAt = now
            };
            dbContext.BusinessWorkflowMappings.Add(processMapping);
        }
        else
        {
            processMapping.UserId = primaryUser.Id;
            processMapping.StepCode = ProcessCode;
            processMapping.WorkflowDefinitionId = SharedWorkflowDefinitionId;
            processMapping.WorkflowName = SharedWorkflowName;
            processMapping.Description = $"Quy trình Onboarding Nhân sự cho nhân viên: {primaryUser.FullName} ({primaryUser.Username})";
            if (string.IsNullOrWhiteSpace(processMapping.InstanceId))
            {
                processMapping.InstanceId = Guid.NewGuid().ToString("N")[..16];
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    [HttpGet("steps")]
    public async Task<IActionResult> GetStepsAsync([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);

        var query = dbContext.OnboardingSteps.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        var steps = await query
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var mappingQuery = dbContext.BusinessWorkflowMappings.AsQueryable();
        if (userId.HasValue)
        {
            mappingQuery = mappingQuery.Where(m => m.UserId == userId.Value);
        }

        var processMapping = await mappingQuery
            .FirstOrDefaultAsync(m => m.StepCode == ProcessCode || m.WorkflowDefinitionId == SharedWorkflowDefinitionId, cancellationToken)
            .ConfigureAwait(false);

        var mappingDto = processMapping == null ? null : new
        {
            processMapping.Id,
            processMapping.UserId,
            processMapping.StepId,
            processMapping.StepCode,
            processMapping.WorkflowDefinitionId,
            processMapping.InstanceId,
            processMapping.WorkflowName,
            processMapping.Description,
            processMapping.WorkflowStatus,
            processMapping.IsActive,
            processMapping.CreatedAt
        };

        var result = steps.Select(s => new
        {
            s.Id,
            s.UserId,
            s.StepCode,
            s.Title,
            s.Department,
            s.Description,
            s.Order,
            s.Status,
            s.AssignedTo,
            s.CreatedAt,
            s.UpdatedAt,
            WorkflowMapping = mappingDto
        });

        return Ok(result);
    }

    [HttpGet("mappings")]
    public async Task<IActionResult> GetMappingsAsync(CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);

        var mappings = await dbContext.BusinessWorkflowMappings
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalSteps = await dbContext.OnboardingSteps.CountAsync(cancellationToken).ConfigureAwait(false);
        var completedSteps = await dbContext.OnboardingSteps.CountAsync(x => x.Status == "Completed", cancellationToken).ConfigureAwait(false);

        var result = mappings.Select(m => new
        {
            m.Id,
            m.UserId,
            UserName = m.User?.FullName ?? "Nhân viên chưa gán",
            UserEmail = m.User?.Email,
            m.StepId,
            m.StepCode,
            m.WorkflowDefinitionId,
            m.InstanceId,
            m.WorkflowName,
            m.Description,
            m.WorkflowStatus,
            m.IsActive,
            m.CreatedAt,
            StepTitle = "Toàn bộ 7 bước Onboarding",
            Department = m.User?.Department ?? "HR & IT",
            Status = completedSteps == totalSteps && totalSteps > 0 ? "Completed" : "InProgress",
            CompletedSteps = completedSteps,
            TotalSteps = totalSteps
        });

        return Ok(result);
    }

    [HttpPost("steps/{id:guid}/execute")]
    public async Task<IActionResult> ExecuteStepAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);

        var step = await dbContext.OnboardingSteps
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (step == null)
        {
            return NotFound(new { message = $"Không tìm thấy bước với ID: {id}" });
        }

        // Cập nhật trạng thái bước hiện tại sang Completed
        step.Status = "Completed";
        step.UpdatedAt = DateTimeOffset.UtcNow;

        // Tự động chuyển bước kế tiếp (nếu có) sang InProgress
        var nextStep = await dbContext.OnboardingSteps
            .Where(x => x.Order > step.Order && x.Status == "Pending")
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (nextStep != null)
        {
            nextStep.Status = "InProgress";
            nextStep.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Đồng bộ trạng thái Parent Workflow trong BusinessWorkflowMappings
        var processMapping = await dbContext.BusinessWorkflowMappings
            .FirstOrDefaultAsync(m => (step.UserId != null && m.UserId == step.UserId) || m.StepCode == ProcessCode || m.WorkflowDefinitionId == SharedWorkflowDefinitionId, cancellationToken)
            .ConfigureAwait(false);

        var allSteps = await dbContext.OnboardingSteps
            .Where(s => step.UserId == null || s.UserId == step.UserId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var allDone = allSteps.All(s => s.Id == step.Id || s.Status == "Completed");

        if (processMapping != null)
        {
            processMapping.WorkflowStatus = allDone ? "Completed" : "Running";
            if (string.IsNullOrWhiteSpace(processMapping.InstanceId))
            {
                if (workflowService != null && !string.IsNullOrWhiteSpace(processMapping.WorkflowDefinitionId))
                {
                    try
                    {
                        var runResult = await workflowService.RunAsync(
                            processMapping.WorkflowDefinitionId,
                            new Dictionary<string, object> { ["ProcessCode"] = ProcessCode },
                            correlationId: processMapping.Id.ToString(),
                            cancellationToken: cancellationToken).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(runResult?.WorkflowInstanceId))
                        {
                            processMapping.InstanceId = runResult.WorkflowInstanceId;
                        }
                    }
                    catch
                    {
                        // Fallback in-memory demo
                    }
                }

                processMapping.InstanceId ??= Guid.NewGuid().ToString("N")[..16];
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            message = $"Đã thực thi thành công bước '{step.Title}'!",
            stepId = step.Id,
            status = step.Status,
            nextStepId = nextStep?.Id,
            workflowDefinitionId = processMapping?.WorkflowDefinitionId,
            instanceId = processMapping?.InstanceId,
            workflowStatus = processMapping?.WorkflowStatus
        });
    }

    [HttpGet("steps/{id:guid}/workflow-details")]
    public async Task<IActionResult> GetStepWorkflowDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);

        var step = await dbContext.OnboardingSteps
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (step == null)
        {
            return NotFound(new { message = $"Không tìm thấy bước với ID: {id}" });
        }

        var processMapping = await dbContext.BusinessWorkflowMappings
            .FirstOrDefaultAsync(m => (step.UserId != null && m.UserId == step.UserId) || m.StepCode == ProcessCode || m.WorkflowDefinitionId == SharedWorkflowDefinitionId, cancellationToken)
            .ConfigureAwait(false);

        if (processMapping == null)
        {
            return NotFound(new { message = "Chưa được cấu hình workflow mapping cho quy trình Onboarding." });
        }

        object? workflowData = null;
        var isInstance = !string.IsNullOrWhiteSpace(processMapping.InstanceId);

        if (isInstance && workflowInstanceService != null)
        {
            // Workflow đang/đã chạy -> lấy theo ParentWorkflowInstanceId
            var instanceMeta = await workflowInstanceService.GetAsync(processMapping.InstanceId!, cancellationToken).ConfigureAwait(false);
            if (instanceMeta != null)
            {
                workflowData = instanceMeta;
            }
        }

        if (workflowData == null && workflowDefinitionService != null && !string.IsNullOrWhiteSpace(processMapping.WorkflowDefinitionId))
        {
            // Fallback tra cứu theo WorkflowDefinitionId
            var defMeta = await workflowDefinitionService.GetAsync(processMapping.WorkflowDefinitionId, cancellationToken).ConfigureAwait(false);
            if (defMeta != null)
            {
                workflowData = defMeta;
            }
        }

        return Ok(new
        {
            stepId = step.Id,
            stepCode = step.StepCode,
            stepTitle = step.Title,
            workflowDefinitionId = processMapping.WorkflowDefinitionId,
            instanceId = processMapping.InstanceId,
            workflowStatus = processMapping.WorkflowStatus,
            queryMode = isInstance ? "ParentWorkflowInstance" : "Definition",
            workflow = workflowData
        });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetDemoAsync(CancellationToken cancellationToken)
    {
        var steps = await dbContext.OnboardingSteps.ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var s in steps)
        {
            s.Status = s.Order == 1 ? "InProgress" : "Pending";
            s.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var processMapping = await dbContext.BusinessWorkflowMappings
            .FirstOrDefaultAsync(m => m.StepCode == ProcessCode || m.WorkflowDefinitionId == SharedWorkflowDefinitionId, cancellationToken)
            .ConfigureAwait(false);

        if (processMapping != null)
        {
            processMapping.WorkflowStatus = "Running";
            processMapping.InstanceId = Guid.NewGuid().ToString("N")[..16]; // Sinh mới ParentWorkflowInstanceId cho lần chạy mới
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new { message = "Đã đặt lại quy trình Onboarding về trạng thái ban đầu." });
    }
}
