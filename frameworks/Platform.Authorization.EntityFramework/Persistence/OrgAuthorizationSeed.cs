using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence;

namespace Platform.Authorization.EntityFramework.Persistence;

/// <summary>
/// Policy phạm vi truy cập theo tổ chức (seed DB). UI chỉ đọc name/rules từ API, không hard-code trên FE.
/// </summary>
public static class OrgAuthorizationSeed
{
    public static void Configure(AuthorizationSeedCatalog catalog) =>
        catalog
            .AddPolicy(SameOrganizationalUnitPolicy())
            .AddPolicy(SamePositionPolicy())
            .AddPolicy(SameLegalEntityPolicy())
            .AddPolicy(WithinOuSubtreePolicy())
            .AddPolicy(ApproveLimitPolicy());

    private static Policy SameOrganizationalUnitPolicy() =>
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
            Code = "SAME_ORGANIZATIONAL_UNIT",
            Name = "Cùng đơn vị tổ chức",
            IsSystemPolicy = true,
            Rules =
            [
                new PolicyRule
                {
                    Id = Guid.Parse("21000000-0000-0000-0000-000000000007"),
                    Order = 1,
                    LeftOperand = "Resource.OrganizationalUnitId",
                    Operator = "==",
                    RightOperand = "User.OrganizationalUnitId",
                },
            ],
        };

    private static Policy SamePositionPolicy() =>
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-00000000000a"),
            Code = "SAME_POSITION",
            Name = "Cùng vị trí",
            IsSystemPolicy = true,
            Rules =
            [
                new PolicyRule
                {
                    Id = Guid.Parse("21000000-0000-0000-0000-00000000000a"),
                    Order = 1,
                    LeftOperand = "Resource.PositionId",
                    Operator = "==",
                    RightOperand = "User.PositionId",
                },
            ],
        };

    private static Policy SameLegalEntityPolicy() =>
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000008"),
            Code = "SAME_LEGAL_ENTITY",
            Name = "Cùng pháp nhân",
            IsSystemPolicy = true,
            Rules =
            [
                new PolicyRule
                {
                    Id = Guid.Parse("21000000-0000-0000-0000-000000000008"),
                    Order = 1,
                    LeftOperand = "Resource.LegalEntityId",
                    Operator = "==",
                    RightOperand = "User.LegalEntityId",
                },
            ],
        };

    private static Policy WithinOuSubtreePolicy() =>
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000009"),
            Code = "WITHIN_OU_SUBTREE",
            Name = "Trong nhánh đơn vị",
            IsSystemPolicy = true,
            Rules =
            [
                new PolicyRule
                {
                    Id = Guid.Parse("21000000-0000-0000-0000-000000000009"),
                    Order = 1,
                    LeftOperand = "Resource.OrganizationalUnitPath",
                    Operator = "StartsWith",
                    RightOperand = "User.OrganizationalUnitPath",
                },
            ],
        };

    private static Policy ApproveLimitPolicy() =>
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
            Code = "APPROVE_LIMIT",
            Name = "Hạn mức duyệt",
            IsSystemPolicy = true,
            Rules =
            [
                new PolicyRule
                {
                    Id = Guid.Parse("21000000-0000-0000-0000-000000000006"),
                    Order = 1,
                    LeftOperand = "Resource.Amount",
                    Operator = "<=",
                    RightOperand = "User.ApproveLimit",
                },
            ],
        };
}
