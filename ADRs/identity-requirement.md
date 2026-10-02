# Thiết kế phân quyền RBAC + ABAC cho hệ thống ERP trên ASP.NET Identity

> Phiên bản: 1.0  
> Phạm vi: Authentication, Authorization, RBAC, ABAC, Role Governance, Data Scope  
> Nền tảng tham chiếu: ASP.NET Core + ASP.NET Identity + C#

---

## 1. Mục tiêu

Hệ thống cần đáp ứng đồng thời các yêu cầu:

- RBAC (Role-Based Access Control): xác định **user được phép thực hiện action nào**.
- ABAC (Attribute-Based Access Control): xác định **user được phép thực hiện action trên resource/data nào**.
- Multi-tenant.
- Multi-organization (Org) trong Tenant.
- Department/Team scope.
- Ownership scope: dữ liệu do chính user tạo.
- Managed resource scope: user chỉ quản lý resource mà mình phụ trách.
- Numeric threshold: ví dụ hạn mức duyệt phiếu chi.
- Dynamic business roles: khách hàng được tạo/chỉnh sửa Role nghiệp vụ.
- System roles cố định: `Admin`, `TenantAdmin`, `OrgAdmin` không được chỉnh sửa quyền.
- Role Governance: user trong Org chỉ được tạo/chỉnh sửa Role có phạm vi quyền thấp hơn quyền của chính họ.
- Tận dụng tối đa ASP.NET Identity, hạn chế tạo thêm các bảng không cần thiết.
- Policy được hệ thống định nghĩa trước; khách hàng cấu hình việc gán Policy/Permission và các attribute cần thiết.
- PolicyRule có thể biểu diễn các biểu thức động như:

```text
Resource.OrgId == User.OrgId
Resource.DepartmentId == User.DepartmentId
Resource.CreatedBy == User.Id
Resource.Amount <= User.ApproveLimit
```

---

# 2. Nguyên tắc thiết kế cốt lõi

## 2.1. RBAC và ABAC có trách nhiệm khác nhau

### RBAC

RBAC trả lời:

> User được phép làm gì?

Ví dụ:

```text
Customer.View
Customer.Edit
Project.View
Project.Edit
Expense.View
Expense.Approve
Accounting.View
Accounting.Edit
Company.Config.Edit
SalesResult.View
SalesResult.Edit
```

### ABAC

ABAC trả lời:

> User được phép làm việc đó trên dữ liệu nào?

Ví dụ:

```text
Resource.TenantId == User.TenantId
Resource.OrgId == User.OrgId
Resource.DepartmentId == User.DepartmentId
Resource.CreatedBy == User.Id
Project.Id IN User.ManagedProjectIds
Expense.Amount <= User.ApproveLimit
```

### Công thức authorization

```text
Authorization
    = RBAC Permission Check
    AND ABAC Policy Check
```

Tùy từng permission, có thể không cần ABAC.

Ví dụ:

```text
HealthCheck.View
```

chỉ cần RBAC.

Trong khi:

```text
Expense.Approve
```

có thể cần:

```text
RBAC: Expense.Approve
ABAC: Expense.Amount <= User.ApproveLimit
ABAC: Expense.OrgId == User.OrgId
```

---

# 3. Mô hình dữ liệu tổng thể

```text
┌─────────────────┐
│    AspNetUsers  │
└────────┬────────┘
         │
         │  UserRoles
         ▼
┌─────────────────┐
│   AspNetRoles   │
└────────┬────────┘
         │
         │ RoleClaims
         ├──────────────────────────┐
         │                          │
         ▼                          ▼
┌─────────────────┐          ┌───────────────┐
│   Permission    │          │    Policy     │
│ (ClaimType/Val) │          └───────┬───────┘
└─────────────────┘                  │
                                     │
                                     ▼
                              ┌───────────────┐
                              │  PolicyRules  │
                              └───────────────┘

┌─────────────────┐
│  AspNetUsers    │
└────────┬────────┘
         │
         │ UserClaims
         ▼
┌───────────────────────────────┐
│ User Attributes / Data Scope │
│ TenantId / OrgId / DeptId    │
│ ApproveLimit / ...            │
└───────────────────────────────┘
```

Các bảng chính:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
Policies
PolicyRules
```

---

# 4. AspNetUsers

ASP.NET Identity tiếp tục là nguồn dữ liệu chính cho user.

Các field nghiệp vụ ngoài Identity không nên nhét tùy tiện vào `UserClaims` nếu đó là dữ liệu quan hệ cốt lõi của domain.

Ví dụ user cần các attribute authorization:

```text
TenantId
OrgId
DepartmentId
ApproveLimit
```

Có thể đưa vào `UserClaims` nếu muốn giữ authorization context ở mức claim.

Ví dụ:

| UserId | ClaimType | ClaimValue |
|---|---|---|
| U001 | TenantId | T001 |
| U001 | OrgId | O001 |
| U001 | DepartmentId | D010 |
| U001 | ApproveLimit | 10000000 |

---

# 5. AspNetRoles

Role được chia thành 2 loại.

## 5.1. System Role

Các role do hệ thống định nghĩa, quyền cố định:

```text
Admin
TenantAdmin
OrgAdmin
```

Đặc tính:

```text
IsSystemRole = true
```

Không cho khách hàng:

- đổi tên nếu hệ thống không cho phép;
- thêm Permission;
- xóa Permission;
- thêm Policy;
- xóa Policy;
- xóa Role.

## 5.2. Business Role / Custom Role

Ví dụ:

```text
Director
CFO
CTO
Chief Accountant
Technical Manager
Sales Manager
Employee
Sales Employee
```

Được cấu hình động bằng UI.

---

# 6. Role Governance

Nên bổ sung metadata cho Role.

Ví dụ model mở rộng:

> Triển khai thực tế: class `Role` (trước đây `ApplicationRole`) trong `Platform.Authentication.Identity/Domain/Role.cs` — xem [ADR RBAC+ABAC §11](./2026-08-14_platform_rbac-abac-authorization.md#11-project-structure--cấu-trúc-triển-khai).

```csharp
public class Role : IdentityRole<Guid>
{
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Cấp độ quyền của Role. Level càng cao thì quyền càng lớn.
    /// </summary>
    public int RoleLevel { get; set; }
}
```

Ví dụ:

| Role | IsSystemRole | RoleLevel |
|---|---:|---:|
| Admin | true | 1000 |
| TenantAdmin | true | 900 |
| OrgAdmin | true | 800 |
| Director | false | 700 |
| CFO | false | 650 |
| CTO | false | 650 |
| DepartmentManager | false | 500 |
| Employee | false | 100 |

Quy tắc:

```text
Actor.RoleLevel > TargetRole.RoleLevel
```

thì Actor mới có thể tạo/chỉnh sửa TargetRole.

Nên dùng điều kiện **strictly lower** để tránh user tự sửa Role có cùng quyền với mình.

---

# 7. AspNetUserRoles

Bảng liên kết user với role.

Ví dụ:

| UserId | RoleId |
|---|---|
| U001 | R_CFO |
| U002 | R_SALES_MANAGER |
| U003 | R_EMPLOYEE |

User có thể có nhiều Role nếu hệ thống cho phép.

Ví dụ:

```text
Nguyen A
 ├── SalesManager
 └── ProjectManager
```

Authorization phải hợp nhất Permission/Policy từ toàn bộ Role của user.

---

# 8. AspNetRoleClaims

`AspNetRoleClaims` là nơi giữ **quyền RBAC** và **tham chiếu tới Policy**.

Quy ước ClaimType:

```text
Permission
Policy
```

## 8.1. Permission Claim

Ví dụ:

| RoleId | ClaimType | ClaimValue |
|---|---|---|
| R_CFO | Permission | Expense.View |
| R_CFO | Permission | Expense.Approve |
| R_CFO | Permission | Accounting.View |
| R_CFO | Permission | Accounting.Edit |

## 8.2. Policy Claim

Role không lưu toàn bộ Rule trong ClaimValue.

RoleClaim chỉ tham chiếu PolicyId/PolicyCode.

Ví dụ:

| RoleId | ClaimType | ClaimValue |
|---|---|---|
| R_CFO | Policy | P_SAME_ORG |
| R_CFO | Policy | P_APPROVE_LIMIT |

Lý do:

- `RoleClaim` vẫn được tận dụng.
- Policy Rule có cấu trúc riêng.
- Có thể query Policy độc lập.
- Dễ audit Policy đang được sử dụng bởi Role nào.
- Không nhồi JSON rule vào `ClaimValue`.

---

# 9. AspNetUserClaims

Dùng cho **Subject Attributes** và authorization context của user.

Ví dụ:

| UserId | ClaimType | ClaimValue |
|---|---|---|
| U001 | TenantId | T001 |
| U001 | OrgId | O001 |
| U001 | DepartmentId | D010 |
| U001 | ApproveLimit | 10000000 |

## Không nên dùng UserClaim để lưu Permission

Không nên:

```text
CanEditCustomer=true
CanApproveExpense=true
```

Nên dùng:

```text
Permission = Customer.Edit
Permission = Expense.Approve
```

trên RoleClaim.

UserClaim chủ yếu đại diện cho attribute được Policy sử dụng.

---

# 10. Policies

Policy là một authorization rule definition được hệ thống định nghĩa trước.

Ví dụ:

| Id | Code | Resource | Action | Description |
|---|---|---|---|---|
| P001 | SameTenant | Any | View/Edit | Resource cùng Tenant |
| P002 | SameOrg | Any | View/Edit | Resource cùng Org |
| P003 | SameDepartment | Any | View/Edit | Resource cùng Department |
| P004 | OwnerOnly | Any | View/Edit | Resource do User tạo |
| P005 | ApproveLimit | Expense | Approve | Amount <= User.ApproveLimit |
| P006 | ManagedProject | Project | View/Edit | Project thuộc phạm vi quản lý |
| P007 | ManagedDepartment | DepartmentData | View/Edit | Dữ liệu thuộc Department quản lý |

Model:

```csharp
public class Policy
{
    public Guid Id { get; set; }

    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? ResourceType { get; set; }

    public string? Action { get; set; }

    public bool IsSystemPolicy { get; set; }

    public ICollection<PolicyRule> Rules { get; set; }
        = new List<PolicyRule>();
}
```

---

# 11. PolicyRules

PolicyRule là biểu thức nguyên tử.

Ví dụ:

```text
LeftOperand  = Resource.OrgId
Operator     = ==
RightOperand = User.OrgId
```

Model:

```csharp
public class PolicyRule
{
    public Guid Id { get; set; }

    public Guid PolicyId { get; set; }

    public string LeftOperand { get; set; } = default!;

    public string Operator { get; set; } = default!;

    public string RightOperand { get; set; } = default!;

    public int Order { get; set; }
}
```

Ví dụ:

| PolicyId | Order | LeftOperand | Operator | RightOperand |
|---|---:|---|---|---|
| P001 | 1 | Resource.TenantId | == | User.TenantId |
| P002 | 1 | Resource.OrgId | == | User.OrgId |
| P003 | 1 | Resource.DepartmentId | == | User.DepartmentId |
| P004 | 1 | Resource.CreatedBy | == | User.Id |
| P005 | 1 | Resource.Amount | <= | User.ApproveLimit |

---

# 12. Các policy chuẩn cho ERP

## 12.1. SameTenant

```text
Resource.TenantId == User.TenantId
```

Dùng cho:

- TenantAdmin
- IT tập đoàn
- các Role cần truy cập toàn bộ dữ liệu trong Tenant

---

## 12.2. SameOrg

```text
Resource.OrgId == User.OrgId
```

Dùng cho:

- OrgAdmin
- Director
- CFO
- Kế toán trưởng
- các Role chỉ được thao tác trong một Org

---

## 12.3. SameDepartment

```text
Resource.DepartmentId == User.DepartmentId
```

Dùng cho:

- Department Manager
- nhân viên được phép truy cập dữ liệu trong phòng

---

## 12.4. OwnerOnly

```text
Resource.CreatedBy == User.Id
```

Dùng cho:

- nhân viên chỉ thao tác trên dữ liệu của chính họ;
- Sales Employee chỉ xem kết quả kinh doanh của mình.

---

## 12.5. ApproveLimit

```text
Resource.Amount <= User.ApproveLimit
```

Dùng cho:

- CFO
- CTO
- Department Manager
- các role có hạn mức khác nhau.

---

## 12.6. ManagedProject

Đây là policy phù hợp với yêu cầu:

> Trưởng phòng kỹ thuật chỉ được xem/sửa các project do phòng mình quản lý.

Một biến thể đơn giản:

```text
Resource.DepartmentId == User.DepartmentId
```

Nếu business thực tế yêu cầu user quản lý một tập project cụ thể, cần resolve một collection:

```text
Resource.ProjectId IN User.ManagedProjectIds
```

Trong trường hợp này, `ManagedProjectIds` thường nên được lấy từ domain data/relationship thay vì nhét một danh sách lớn vào JWT.

---

## 12.7. ManagedDepartment

```text
Resource.DepartmentId IN User.ManagedDepartmentIds
```

Có thể dùng cho các trường hợp quản lý nhiều department.

---

# 13. Thiết kế theo từng yêu cầu nghiệp vụ

## 13.1. Admin

Yêu cầu:

> Được xem toàn bộ hệ thống.

Nên coi đây là System Role.

```text
Role = Admin
IsSystemRole = true
RoleLevel = 1000
```

Permission:

```text
*
```

Không cần Policy scope nếu hệ thống coi Admin là global.

Authorization:

```text
HasPermission("*") => Allow
```

---

## 13.2. TenantAdmin

Yêu cầu:

> Xem toàn bộ dữ liệu trong Tenant.

```text
Role = TenantAdmin
IsSystemRole = true
RoleLevel = 900
```

Permission:

```text
*
```

Policy:

```text
SameTenant
```

Tức:

```text
Resource.TenantId == User.TenantId
```

---

## 13.3. OrgAdmin

Yêu cầu:

> Xem toàn bộ dữ liệu trong Org.

```text
Role = OrgAdmin
IsSystemRole = true
RoleLevel = 800
```

Permission:

```text
*
```

Policy:

```text
SameOrg
```

---

## 13.4. Director

Yêu cầu:

> Xem toàn bộ dữ liệu trong Org nhưng không chỉnh sửa.

RoleClaims:

```text
Permission = Customer.View
Permission = Project.View
Permission = Accounting.View
Permission = SalesResult.View
Expense.View
...

Policy = P_SAME_ORG
```

Không cấp:

```text
Customer.Edit
Project.Edit
Accounting.Edit
```

Đây là ví dụ điển hình cho việc:

```text
RBAC = View
ABAC = SameOrg
```

---

## 13.5. IT tập đoàn

Yêu cầu:

> Có thể thay đổi cấu hình các công ty thành viên.

RBAC:

```text
Company.Config.View
Company.Config.Edit
```

ABAC tùy cấu trúc tenant:

```text
SameTenant
```

hoặc policy riêng nếu IT chỉ được tác động lên một tập Org/company nhất định.

---

## 13.6. Kế toán trưởng

Yêu cầu:

> Xem/sửa toàn bộ dữ liệu kế toán trong công ty, không xem công ty khác.

RBAC:

```text
Accounting.View
Accounting.Edit
```

ABAC:

```text
SameOrg
```

---

## 13.7. Trưởng phòng kỹ thuật

Yêu cầu:

> Chỉ xem/sửa các Project mà phòng của trưởng phòng quản lý.

RBAC:

```text
Project.View
Project.Edit
```

ABAC tùy domain:

### Nếu mỗi phòng quản lý toàn bộ project thuộc Department

```text
Resource.DepartmentId == User.DepartmentId
```

### Nếu mỗi trưởng phòng quản lý một tập project cụ thể

```text
Resource.ProjectId IN User.ManagedProjectIds
```

Khuyến nghị domain model có quan hệ rõ ràng:

```text
Department
Project
ProjectManager / ProjectManagerDepartment
```

thay vì lưu danh sách ProjectId lớn vào claim.

---

## 13.8. Employee

Yêu cầu:

> Xem/sửa dữ liệu do chính họ tạo ra hoặc dữ liệu trong phòng dưới sự quản lý của trưởng phòng.

Permission:

```text
<business>.View
<business>.Edit
```

ABAC cần hỗ trợ OR.

Điều kiện:

```text
OwnerOnly
OR
SameDepartment
```

Logic:

```text
Resource.CreatedBy == User.Id
OR
Resource.DepartmentId == User.DepartmentId
```

Không nên ép Policy `OwnerOnly` và `SameDepartment` thành hai Role độc lập nếu business rule thực sự là OR.

---

## 13.9. CFO / CTO

Yêu cầu:

> Duyệt phiếu chi dưới 10 triệu.

RBAC:

```text
Expense.View
Expense.Approve
```

UserClaim:

```text
ApproveLimit = 10000000
```

Policy:

```text
Resource.Amount <= User.ApproveLimit
```

Ngoài ra nên kết hợp:

```text
Resource.OrgId == User.OrgId
```

nếu CFO/CTO chỉ có quyền duyệt trong Org của mình.

---

## 13.10. Department Manager

Yêu cầu:

> Duyệt phiếu chi dưới 2 triệu.

RBAC:

```text
Expense.View
Expense.Approve
```

UserClaim:

```text
ApproveLimit = 2000000
```

Policy:

```text
ApproveLimit
```

Nếu phiếu chi phải thuộc Org/Department của người duyệt, kết hợp thêm:

```text
SameOrg
SameDepartment
```

---

## 13.11. Sales Manager

Yêu cầu:

> Quản lý kết quả kinh doanh của nhân viên trong phòng.

RBAC:

```text
SalesResult.View
SalesResult.Edit
```

ABAC:

```text
Resource.DepartmentId == User.DepartmentId
```

hoặc `ManagedDepartment` nếu manager quản lý nhiều department.

---

## 13.12. Sales Employee

Yêu cầu:

> Chỉ được xem kết quả kinh doanh của chính mình.

RBAC:

```text
SalesResult.View
```

ABAC:

```text
Resource.EmployeeId == User.Id
```

Có thể tái sử dụng `OwnerOnly` nếu domain coi `EmployeeId` chính là owner.

---

# 14. Role mẫu

## Admin

```text
Type: System
Level: 1000

Permission:
    *

Policy:
    none
```

---

## TenantAdmin

```text
Type: System
Level: 900

Permission:
    *

Policy:
    SameTenant
```

---

## OrgAdmin

```text
Type: System
Level: 800

Permission:
    *

Policy:
    SameOrg
```

---

## Director

```text
Type: Custom
Level: 700

Permission:
    Customer.View
    Project.View
    Accounting.View
    Expense.View
    SalesResult.View

Policy:
    SameOrg
```

---

## CFO

```text
Type: Custom
Level: 650

Permission:
    Accounting.View
    Accounting.Edit
    Expense.View
    Expense.Approve

Policy:
    SameOrg
    ApproveLimit
```

User attribute:

```text
ApproveLimit = 10000000
```

---

## TechnicalManager

```text
Type: Custom
Level: 500

Permission:
    Project.View
    Project.Edit

Policy:
    SameDepartment
```

Hoặc:

```text
ManagedProject
```

nếu project scope không hoàn toàn tương ứng với Department.

---

## Employee

```text
Type: Custom
Level: 100

Permission:
    <business>.View
    <business>.Edit

Policy:
    OwnerOnly
    OR SameDepartment
```

---

# 15. Role Claim dữ liệu mẫu

Ví dụ `CFO`:

| ClaimType | ClaimValue |
|---|---|
| Permission | Accounting.View |
| Permission | Accounting.Edit |
| Permission | Expense.View |
| Permission | Expense.Approve |
| Policy | P_SAME_ORG |
| Policy | P_APPROVE_LIMIT |

Ví dụ `TechnicalManager`:

| ClaimType | ClaimValue |
|---|---|
| Permission | Project.View |
| Permission | Project.Edit |
| Policy | P_MANAGED_PROJECT |

Ví dụ `SalesEmployee`:

| ClaimType | ClaimValue |
|---|---|
| Permission | SalesResult.View |
| Policy | P_OWNER_ONLY |

---

# 16. Authentication Flow

Authentication không nên đánh giá toàn bộ Policy cho mọi request.

Flow khuyến nghị:

```text
Username + Password
        │
        ▼
ASP.NET Identity
        │
        ├── User
        ├── UserRoles
        ├── Roles
        ├── UserClaims
        └── RoleClaims
        │
        ▼
Authorization Context
        │
        ▼
Cache / Access Token
```

JWT có thể chứa:

```json
{
  "sub": "U001",
  "tenant_id": "T001",
  "org_id": "O001",
  "department_id": "D010",
  "permissions": [
    "Expense.View",
    "Expense.Approve",
    "Accounting.View"
  ],
  "policies": [
    "P_SAME_ORG",
    "P_APPROVE_LIMIT"
  ]
}
```

Không nên nhét mọi domain relationship vào JWT.

Ví dụ không nên để:

```json
"managed_project_ids": ["P1", "P2", "P3", "...rất nhiều..."]
```

nếu danh sách lớn hoặc thay đổi thường xuyên.

---

# 17. Authorization Flow

Authorization gồm các lớp:

```text
Request
  │
  ▼
Authentication
  │
  ▼
User Context
  │
  ▼
RBAC Permission Check
  │
  ├── Deny → 403
  │
  ▼
ABAC Policy Check
  │
  ├── Deny → 403
  │
  ▼
Allow
```

---

# 18. RBAC Permission Check

Ví dụ extension:

```csharp
public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(
        this ClaimsPrincipal user,
        string permission)
    {
        if (user.HasClaim("Permission", "*"))
            return true;

        return user.Claims.Any(x =>
            x.Type == "Permission" &&
            x.Value == permission);
    }
}
```

Ví dụ:

```csharp
if (!User.HasPermission("Expense.Approve"))
{
    return Forbid();
}
```

---

# 19. Policy Resolution

Policy được lấy từ RoleClaim.

```csharp
public IReadOnlyCollection<string> GetPolicyCodes(
    ClaimsPrincipal user)
{
    return user.Claims
        .Where(x => x.Type == "Policy")
        .Select(x => x.Value)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
```

Sau đó resolve:

```text
PolicyCode
   ↓
Policy
   ↓
PolicyRules
```

Có thể cache toàn bộ compiled policy theo `PolicyCode`.

---

# 20. Operand Resolution

Policy engine cần resolve hai loại operand chính:

```text
User.*
Resource.*
```

Ví dụ:

```text
User.OrgId
User.DepartmentId
User.ApproveLimit
User.Id

Resource.OrgId
Resource.DepartmentId
Resource.Amount
Resource.CreatedBy
```

Ví dụ code:

```csharp
public interface IOperandResolver
{
    object? Resolve(
        string operand,
        AuthorizationContext context);
}
```

Context:

```csharp
public sealed class AuthorizationContext
{
    public required ClaimsPrincipal User { get; init; }

    public required object Resource { get; init; }

    public required string ResourceType { get; init; }

    public required string Action { get; init; }
}
```

Resolver:

```csharp
public sealed class OperandResolver : IOperandResolver
{
    public object? Resolve(
        string operand,
        AuthorizationContext context)
    {
        if (operand.StartsWith("User.", StringComparison.OrdinalIgnoreCase))
        {
            var claimType = operand[5..];

            return context.User.Claims
                .FirstOrDefault(x => x.Type == claimType)
                ?.Value;
        }

        if (operand.StartsWith("Resource.", StringComparison.OrdinalIgnoreCase))
        {
            var propertyName = operand[9..];

            return context.Resource
                .GetType()
                .GetProperty(propertyName)
                ?.GetValue(context.Resource);
        }

        throw new InvalidOperationException(
            $"Unsupported operand: {operand}");
    }
}
```

> Trong production nên tránh reflection tùy tiện cho mọi request; có thể dùng property metadata/compiled accessor hoặc domain-specific resolver để kiểm soát field được phép truy cập.

---

# 21. Rule Evaluator

```csharp
public interface IRuleEvaluator
{
    bool Evaluate(
        PolicyRule rule,
        AuthorizationContext context);
}
```

Ví dụ:

```csharp
public sealed class RuleEvaluator : IRuleEvaluator
{
    private readonly IOperandResolver _operandResolver;

    public RuleEvaluator(IOperandResolver operandResolver)
    {
        _operandResolver = operandResolver;
    }

    public bool Evaluate(
        PolicyRule rule,
        AuthorizationContext context)
    {
        var left = _operandResolver.Resolve(
            rule.LeftOperand,
            context);

        var right = _operandResolver.Resolve(
            rule.RightOperand,
            context);

        return rule.Operator switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            ">"  => Compare(left, right) > 0,
            ">=" => Compare(left, right) >= 0,
            "<"  => Compare(left, right) < 0,
            "<=" => Compare(left, right) <= 0,
            _ => throw new InvalidOperationException(
                $"Unsupported operator: {rule.Operator}")
        };
    }

    private static int Compare(object? left, object? right)
    {
        if (left is null || right is null)
            throw new InvalidOperationException(
                "Cannot compare null operands.");

        if (decimal.TryParse(left.ToString(), out var leftNumber) &&
            decimal.TryParse(right.ToString(), out var rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        return string.Compare(
            left.ToString(),
            right.ToString(),
            StringComparison.Ordinal);
    }
}
```

---

# 22. Policy Evaluation

Policy cần có semantics rõ ràng.

## AND

Ví dụ:

```text
SameOrg
AND
ApproveLimit
```

Điều này nghĩa là:

```text
Resource.OrgId == User.OrgId
AND
Resource.Amount <= User.ApproveLimit
```

## OR

Ví dụ Employee:

```text
OwnerOnly
OR
SameDepartment
```

Tương ứng:

```text
Resource.CreatedBy == User.Id
OR
Resource.DepartmentId == User.DepartmentId
```

Không nên dùng một danh sách `Policy` mà không có semantics về AND/OR.

Có thể biểu diễn ở cấp `AuthorizationRequirement` hoặc nhóm rule.

---

# 23. Gợi ý mở rộng Policy Group

Nếu một authorization cần kết hợp nhiều Policy:

```text
AuthorizationRuleSet

AND
 ├── SameOrg
 └── ApproveLimit
```

Hoặc:

```text
AuthorizationRuleSet

OR
 ├── OwnerOnly
 └── SameDepartment
```

Nếu muốn biểu diễn phức tạp hơn:

```text
(OwnerOnly OR SameDepartment)
AND
SameOrg
```

nên sử dụng một cấu trúc expression tree thay vì nhồi toàn bộ vào chuỗi.

Tuy nhiên không nên xây DSL tự do cho khách hàng ngay từ đầu nếu các rule chuẩn đã đủ dùng.

---

# 24. Ví dụ hoàn chỉnh: Expense Approve

## User

```text
User.Id = U001
User.OrgId = O001
User.DepartmentId = D010
User.ApproveLimit = 10,000,000
```

## Role

```text
CFO
```

RoleClaims:

```text
Permission = Expense.Approve
Policy = P_SAME_ORG
Policy = P_APPROVE_LIMIT
```

## Expense

```text
Expense.OrgId = O001
Expense.Amount = 8,000,000
```

## Evaluation

RBAC:

```text
Expense.Approve
```

=> PASS

ABAC 1:

```text
Expense.OrgId == User.OrgId
O001 == O001
```

=> PASS

ABAC 2:

```text
Expense.Amount <= User.ApproveLimit
8,000,000 <= 10,000,000
```

=> PASS

Final:

```text
ALLOW
```

---

# 25. Ví dụ vượt hạn mức

```text
Expense.Amount = 15,000,000
User.ApproveLimit = 10,000,000
```

RBAC:

```text
Expense.Approve = PASS
```

ABAC:

```text
15,000,000 <= 10,000,000
```

=> FAIL

Final:

```text
DENY
```

---

# 26. Ví dụ Director

RoleClaims:

```text
Permission = Accounting.View
Permission = Project.View
Permission = Customer.View
Policy = P_SAME_ORG
```

Resource:

```text
OrgId = O001
```

User:

```text
OrgId = O001
```

View:

```text
RBAC = PASS
ABAC = PASS
=> ALLOW
```

Edit:

```text
RBAC = FAIL
=> DENY
```

Không cần chạy ABAC nếu RBAC đã fail.

---

# 27. Ví dụ Technical Manager

User:

```text
DepartmentId = TECH-A
```

Role:

```text
TechnicalManager
```

Permission:

```text
Project.View
Project.Edit
```

Policy:

```text
ManagedProject
```

Project P001:

```text
DepartmentId = TECH-A
```

=> PASS

Project P002:

```text
DepartmentId = TECH-B
```

=> FAIL

---

# 28. Ví dụ Employee

Employee có:

```text
Permission = Customer.View
Permission = Customer.Edit
```

Policy:

```text
OwnerOnly OR SameDepartment
```

Resource 1:

```text
CreatedBy = User.Id
DepartmentId = D999
```

OwnerOnly:

```text
PASS
```

=> ALLOW

Resource 2:

```text
CreatedBy = OtherUser
DepartmentId = User.DepartmentId
```

SameDepartment:

```text
PASS
```

=> ALLOW

Resource 3:

```text
CreatedBy = OtherUser
DepartmentId = D999
```

OwnerOnly:

```text
FAIL
```

SameDepartment:

```text
FAIL
```

=> DENY

---

# 29. Role Governance: tạo Role động

Khi user OrgAdmin tạo Role mới:

```text
Actor = OrgAdmin
Actor.RoleLevel = 800
```

Role mới:

```text
DepartmentManager
RoleLevel = 500
```

=> hợp lệ.

Nhưng nếu UI/API yêu cầu:

```text
RoleLevel = 900
```

thì:

```text
900 >= 800
```

=> DENY.

---

# 30. Giới hạn Permission khi tạo Role

Không chỉ kiểm tra RoleLevel.

Actor chỉ được cấp Permission mà Actor có quyền cấp.

Ví dụ:

```csharp
var actorPermissions = authorizationContext.GetPermissions();

foreach (var permission in requestedPermissions)
{
    if (!actorPermissions.Contains(permission))
    {
        throw new ForbiddenException(
            $"Cannot grant permission: {permission}");
    }
}
```

Nếu OrgAdmin có wildcard trong hệ thống thì cần có logic đặc biệt cho System Role.

---

# 31. Giới hạn Policy khi tạo Role

Tương tự Permission:

```csharp
foreach (var policy in requestedPolicies)
{
    if (!actorPolicies.Contains(policy))
    {
        throw new ForbiddenException(
            $"Cannot assign policy: {policy}");
    }
}
```

Không nên cho một user tự gán Policy mà Actor không có.

---

# 32. Privilege Escalation cần ngăn chặn

Không được chỉ kiểm tra:

```text
RoleLevel
```

Mà cần kiểm tra đồng thời:

```text
1. RoleLevel
2. Permission grant scope
3. Policy grant scope
4. Tenant scope
5. Org scope
```

Ví dụ User thuộc Org O001 không được tạo Role có:

```text
SameTenant
```

nếu bản thân User không có quyền vượt ra ngoài Org.

---

# 33. System Role Governance

Các role:

```text
Admin
TenantAdmin
OrgAdmin
```

được seed bởi hệ thống.

Nên có identifier ổn định:

```text
ADMIN
TENANT_ADMIN
ORG_ADMIN
```

Không nên sử dụng tên hiển thị làm logic authorization.

Ví dụ:

```csharp
public static class SystemRoles
{
    public const string Admin = "ADMIN";
    public const string TenantAdmin = "TENANT_ADMIN";
    public const string OrgAdmin = "ORG_ADMIN";
}
```

---

# 34. Wildcard Permission

`Permission = *` đơn giản nhưng cần cẩn thận.

Có thể tách:

```text
System.All
```

thay vì wildcard string thuần túy.

Ví dụ:

```text
Permission = System.All
```

Ý nghĩa:

```text
Bypass normal permission lookup
```

nhưng vẫn phải kiểm tra ABAC nếu role không phải global Admin.

Do đó:

```text
Admin
Permission = System.All
ABAC = Global
```

trong khi:

```text
TenantAdmin
Permission = System.All
ABAC = SameTenant
```

---

# 35. Data Scope và Query

ABAC không chỉ dùng để trả `true/false` ở cuối request.

Với API list/search, nên áp dụng scope trực tiếp vào query để tránh:

```text
Load toàn bộ dữ liệu
→ Evaluate từng record
→ Filter
```

Ví dụ:

```csharp
query = query.Where(x =>
    x.OrgId == userOrgId);
```

Với Employee:

```csharp
query = query.Where(x =>
    x.CreatedBy == userId ||
    x.DepartmentId == userDepartmentId);
```

Đây là **Data Filtering / Data Scope**.

Nó khác với resource authorization sau khi record đã được load.

---

# 36. Authorization nên có hai lớp runtime

## Layer 1 - Permission Authorization

Kiểm tra:

```text
Action có được phép không?
```

Ví dụ:

```text
Expense.Approve
```

## Layer 2 - Data Scope Authorization

Kiểm tra:

```text
Resource có nằm trong scope không?
```

Ví dụ:

```text
Expense.OrgId == User.OrgId
Expense.Amount <= User.ApproveLimit
```

---

# 37. Cache Authorization Context

Có thể cache:

```text
UserId
TenantId
OrgId
DepartmentId
Permissions
PolicyIds
Relevant Attributes
```

Ví dụ:

```csharp
public sealed class AuthorizationSnapshot
{
    public Guid UserId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OrgId { get; init; }
    public Guid? DepartmentId { get; init; }

    public IReadOnlySet<string> Permissions { get; init; }
        = new HashSet<string>();

    public IReadOnlySet<string> Policies { get; init; }
        = new HashSet<string>();

    public decimal? ApproveLimit { get; init; }
}
```

Cache có thể nằm ở:

```text
Redis
In-memory cache
Distributed cache
```

nếu hệ thống cần.

---

# 38. Cache Invalidation

Khi thay đổi:

- UserRole
- RoleClaim
- UserClaim
- Role
- Policy
- PolicyRule

cần invalid authorization cache tương ứng.

Ví dụ:

```text
RoleClaim changed
    ↓
Invalidate all user snapshots of that Role
```

Nếu hệ thống multi-tenant lớn, nên quản lý cache key có scope rõ ràng.

---

# 39. Policy Cache

Policy definition có thể cache riêng:

```text
Policy:P_SAME_ORG
Policy:P_APPROVE_LIMIT
Policy:P_OWNER_ONLY
```

Ví dụ:

```csharp
public interface IPolicyProvider
{
    Task<PolicyDefinition?> GetAsync(
        string policyCode,
        CancellationToken cancellationToken);
}
```

Sau khi load từ DB lần đầu:

```text
DB → Policy Cache → Evaluator
```

---

# 40. Query thống kê và audit

## Role nào có Permission `Expense.Approve`?

```sql
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc
    ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Permission'
  AND rc.ClaimValue = 'Expense.Approve';
```

## Role nào dùng Policy `P_SAME_ORG`?

```sql
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc
    ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Policy'
  AND rc.ClaimValue = 'P_SAME_ORG';
```

## Policy nào đang tham chiếu `Resource.OrgId`?

```sql
SELECT *
FROM PolicyRules
WHERE LeftOperand = 'Resource.OrgId'
   OR RightOperand = 'Resource.OrgId';
```

## Role nào có thể duyệt theo `ApproveLimit`?

```sql
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc
    ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Permission'
  AND rc.ClaimValue = 'Expense.Approve';
```

Sau đó kiểm tra Policy:

```sql
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc
    ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Policy'
  AND rc.ClaimValue = 'P_APPROVE_LIMIT';
```

---

# 41. Authorization Trace / Audit

Đối với ERP nên có log authorization khi cần audit.

Ví dụ:

```text
User: U001
Action: Expense.Approve
Resource: EXP-000123

RBAC:
    Permission = Expense.Approve
    Result = PASS

ABAC:
    Policy = P_SAME_ORG
    Rule = Resource.OrgId == User.OrgId
    Result = PASS

    Policy = P_APPROVE_LIMIT
    Rule = Resource.Amount <= User.ApproveLimit
    Result = PASS

Decision = ALLOW
```

Khi bị deny:

```text
Decision = DENY
Reason = ApproveLimitExceeded
```

Không nên log quá nhiều dữ liệu nhạy cảm trong authorization log.

---

# 42. Các nguyên tắc quan trọng

## 42.1. Không đưa toàn bộ Business Rule vào Role

Role chỉ nói:

```text
Permission
Policy assignment
```

Không nên chứa hàng trăm câu điều kiện business cụ thể.

---

## 42.2. Không đưa domain relationship lớn vào JWT

Ví dụ:

```text
ManagedProjectIds = 5000 IDs
```

không phù hợp cho JWT.

Nên resolve từ domain data/cache.

---

## 42.3. Không cho khách hàng viết DSL tùy ý ở giai đoạn đầu

Nên cung cấp danh sách operand/operator đã được hệ thống whitelist.

Ví dụ operand được phép:

```text
Resource.TenantId
Resource.OrgId
Resource.DepartmentId
Resource.CreatedBy
Resource.Amount

User.Id
User.TenantId
User.OrgId
User.DepartmentId
User.ApproveLimit
```

Operator:

```text
==
!=
>
>=
<
<=
IN
```

---

## 42.4. Policy phải được validate khi cấu hình

Ví dụ không cho phép:

```text
Resource.Password == User.Password
```

hoặc:

```text
UnknownField == User.Id
```

Policy compiler/validator phải reject ngay khi lưu.

---

# 43. Khuyến nghị schema

## Identity

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
```

## Authorization extension

```text
Policies
PolicyRules
```

Không cần tạo ngay:

```text
Permissions
RolePermissions
UserPermissions
PolicyExpressions
PolicyConditions
PolicyOperators
```

trừ khi sau này có nhu cầu quản trị metadata Permission/Policy ở mức sâu hơn.

---

# 44. ERD logic

```text
┌─────────────────────┐
│     AspNetUsers     │
│---------------------│
│ Id                  │
│ UserName            │
└──────────┬──────────┘
           │
           │ 1:N
           ▼
┌─────────────────────┐
│  AspNetUserClaims   │
│---------------------│
│ UserId              │
│ ClaimType           │
│ ClaimValue          │
└─────────────────────┘

           │
           │ N:M
           ▼
┌─────────────────────┐
│   AspNetUserRoles   │
│---------------------│
│ UserId              │
│ RoleId              │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│     AspNetRoles     │
│---------------------│
│ Id                  │
│ Name                │
│ IsSystemRole        │
│ RoleLevel           │
└──────────┬──────────┘
           │
           │ 1:N
           ▼
┌─────────────────────┐
│  AspNetRoleClaims   │
│---------------------│
│ RoleId              │
│ ClaimType           │
│ ClaimValue          │
│                     │
│ Permission           │
│ Policy → PolicyId   │
└─────────────────────┘

Policies
   │
   │ 1:N
   ▼
PolicyRules
```

---

# 45. Authorization Decision Model

```text
                         ┌──────────────────┐
                         │   HTTP Request   │
                         └────────┬─────────┘
                                  │
                                  ▼
                         ┌──────────────────┐
                         │ Authentication   │
                         └────────┬─────────┘
                                  │
                                  ▼
                         ┌──────────────────┐
                         │ User Context     │
                         └────────┬─────────┘
                                  │
                    ┌─────────────┴─────────────┐
                    ▼                           ▼
          ┌──────────────────┐       ┌──────────────────┐
          │ RBAC Permission  │       │ ABAC Context     │
          │ Check            │       │ Resolution       │
          └────────┬─────────┘       └────────┬─────────┘
                   │                          │
                   └────────────┬─────────────┘
                                ▼
                       ┌──────────────────┐
                       │ Policy Evaluate  │
                       └────────┬─────────┘
                                │
                    ┌───────────┴───────────┐
                    ▼                       ▼
                 ALLOW                    DENY
```

---

# 46. Mapping toàn bộ requirement → RBAC + ABAC

| Requirement | RBAC | ABAC |
|---|---|---|
| Admin xem toàn hệ thống | `System.All` | Global |
| TenantAdmin xem toàn Tenant | `System.All` | `SameTenant` |
| OrgAdmin xem toàn Org | `System.All` | `SameOrg` |
| Director xem toàn Org, không sửa | `*.View` | `SameOrg` |
| IT tập đoàn sửa config công ty thành viên | `Company.Config.Edit` | `SameTenant` hoặc scope riêng |
| Kế toán trưởng xem/sửa kế toán | `Accounting.View/Edit` | `SameOrg` |
| Trưởng phòng kỹ thuật xem/sửa project | `Project.View/Edit` | `ManagedProject` |
| Employee xem/sửa dữ liệu của mình | `<domain>.View/Edit` | `OwnerOnly` |
| Employee xem/sửa dữ liệu phòng | `<domain>.View/Edit` | `SameDepartment` |
| CFO/CTO duyệt < 10M | `Expense.Approve` | `ApproveLimit` + `SameOrg` |
| Trưởng phòng duyệt < 2M | `Expense.Approve` | `ApproveLimit` + `SameDepartment` |
| Sales Manager quản lý doanh số phòng | `SalesResult.View/Edit` | `ManagedDepartment` |
| Sales Employee xem doanh số mình | `SalesResult.View` | `OwnerOnly` |

---

# 47. Kết luận kiến trúc

Mô hình cuối cùng được chuẩn hóa thành:

```text
ASP.NET Identity
│
├── Users
├── Roles
├── UserRoles
├── UserClaims
└── RoleClaims
      │
      ├── Permission
      └── PolicyId

Authorization Extension
│
├── Policies
└── PolicyRules
```

Nguyên tắc:

```text
RBAC
→ RoleClaims / Permission
→ User được làm gì?

ABAC
→ UserClaims + Policies + PolicyRules
→ User được làm trên resource nào?

Role Governance
→ RoleLevel + IsSystemRole
→ User được cấp quyền gì cho người khác?
```

---

# 48. Tư tưởng thiết kế cuối cùng

Không xây hệ thống phân quyền theo kiểu:

```text
Role = tất cả logic quyền
```

mà theo 3 lớp:

```text
1. Permission
   = What can the user do?

2. Policy
   = Under what conditions?

3. Scope / Attribute
   = On which data?
```

Ví dụ:

```text
CFO
│
├── Permission
│     └── Expense.Approve
│
├── Policy
│     ├── SameOrg
│     └── ApproveLimit
│
└── Attributes
      ├── OrgId = O001
      └── ApproveLimit = 10,000,000
```

Kết quả authorization:

```text
CFO
    │
    ├── Can do Expense.Approve?      → RBAC
    │
    ├── Expense belongs to Org?     → ABAC / SameOrg
    │
    └── Expense <= approval limit?  → ABAC / ApproveLimit
```

Chỉ khi cả ba điều kiện đều phù hợp thì request mới được `ALLOW`.

---

# 49. Phạm vi nên giữ đơn giản ở phiên bản đầu

Nên triển khai trước các thành phần:

```text
Permission
SameTenant
SameOrg
SameDepartment
OwnerOnly
ApproveLimit
ManagedProject
ManagedDepartment
```

Và các operator:

```text
==
!=
>
>=
<
<=
IN
```

Đủ để xây phần lớn các use case RBAC + ABAC đã nêu mà chưa cần một Policy Engine quá phức tạp.

Nếu sau này xuất hiện nhu cầu:

```text
(A OR B) AND (C OR D)
NOT A
nested expression
function calls
relationship traversal
external data source
environment context
```

thì mới cân nhắc nâng Policy Engine thành Expression Tree/DSL riêng hoặc tích hợp một engine chuyên dụng.
