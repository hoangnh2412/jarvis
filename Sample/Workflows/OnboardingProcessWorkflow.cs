using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace Sample.Workflows;

/// <summary>
/// Workflow tổng thể quy trình Onboarding nhân sự tích hợp Elsa Workflow,
/// bao gồm đầy đủ các bước tuần tự từ HR và IT Admin đến Nhân sự mới.
/// </summary>
public sealed class OnboardingProcessWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-onboarding-process";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "Quy trình Onboarding Nhân sự";
        builder.Description = "Quy trình tích hợp 7 bước tiếp nhận nhân sự mới";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("=== [BƯỚC 1] HR bàn giao thiết bị làm việc (Laptop, phụ kiện, thẻ ra vào) ==="),
                new WriteLine("=== [BƯỚC 2] IT admin tạo tài khoản Active Directory và hòm thư doanh nghiệp ==="),
                new WriteLine("=== [BƯỚC 3] IT admin phân quyền dự án Jira ==="),
                new WriteLine("=== [BƯỚC 4] IT admin cấp quyền wiki tài liệu Confluence ==="),
                new WriteLine("=== [BƯỚC 5] IT admin cấp quyền kho mã nguồn Bitbucket ==="),
                new WriteLine("=== [BƯỚC 6] HR hoàn tất ký hợp đồng thử việc ==="),
                new WriteLine("=== [BƯỚC 7] Nhân sự mới xác nhận hoàn tất quy trình Onboarding ===")
            }
        };
    }
}

public sealed class HrHandoverDeviceWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-hr-handover-device";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "HR bàn giao thiết bị làm việc";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("HR: Kiểm tra danh sách thiết bị bàn giao (Laptop, màn hình, thẻ ra vào)"),
                new WriteLine("HR: Bàn giao thiết bị và lập biên bản bàn giao thiết bị làm việc"),
                new WriteLine("HR: Nhân viên ký nhận biên bản thiết bị hoàn tất")
            }
        };
    }
}

public sealed class ItCreateAdWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-it-create-ad";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "IT admin tạo và bàn giao tài khoản AD";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("IT Admin: Tiếp nhận thông tin nhân sự từ HR"),
                new WriteLine("IT Admin: Tạo tài khoản Active Directory và hòm thư doanh nghiệp"),
                new WriteLine("IT Admin: Gửi thông tin tài khoản đăng nhập cho nhân viên")
            }
        };
    }
}

public sealed class ItCreateJiraWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-it-create-jira";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "IT admin tạo và bàn giao tài khoản Jira";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("IT Admin: Kiểm tra dự án và phân quyền Jira cho nhân sự"),
                new WriteLine("IT Admin: Phân quyền vào dự án Jira tương ứng"),
                new WriteLine("IT Admin: Gửi thông báo cấp quyền Jira thành công")
            }
        };
    }
}

public sealed class ItCreateConfluenceWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-it-create-confluence";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "IT admin tạo và bàn giao tài khoản Confluence";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("IT Admin: Cấp quyền truy cập Confluence Space dự án"),
                new WriteLine("IT Admin: Gửi tài liệu hướng dẫn làm quen dự án qua Confluence")
            }
        };
    }
}

public sealed class ItCreateBitbucketWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-it-create-bitbucket";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "IT admin tạo và bàn giao tài khoản Bitbucket";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("IT Admin: Phân nhóm quyền vào kho mã nguồn Bitbucket repository"),
                new WriteLine("IT Admin: Cấu hình SSH Key và gửi tài liệu hướng dẫn Git cho nhân viên")
            }
        };
    }
}

public sealed class HrSignProbationContractWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-hr-sign-contract";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "HR ký hợp đồng thử việc";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("HR: Tạo dự thảo hợp đồng thử việc theo mẫu"),
                new WriteLine("HR: Gửi hợp đồng thử việc cho nhân viên ký số / ký giấy"),
                new WriteLine("HR: Lưu trữ hợp đồng vào hệ thống nhân sự")
            }
        };
    }
}

public sealed class EmployeeConfirmOnboardWorkflow : WorkflowBase
{
    public const string DefinitionId = "wf-employee-confirm";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.DefinitionId = DefinitionId;
        builder.Name = "Nhân sự mới xác nhận đã onboard";

        builder.Root = new Sequence
        {
            Activities =
            {
                new WriteLine("Employee: Kiểm tra toàn bộ tài khoản, thiết bị và hợp đồng"),
                new WriteLine("Employee: Xác nhận hoàn tất quy trình Onboarding nhân sự")
            }
        };
    }
}
