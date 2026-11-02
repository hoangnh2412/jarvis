namespace Platform.Modules.IdentityAdmin.Abstractions;

public sealed record AuthorizationOperandDto(string Path, string Source, string Kind, string ValueType);

public sealed record AuthorizationPolicyRuleDto(
    Guid Id,
    int Order,
    string LeftOperand,
    string Operator,
    string RightOperand);

public sealed record AuthorizationPolicyDto(
    Guid Id,
    string Code,
    string Name,
    string? ResourceType,
    string? Action,
    bool IsSystemPolicy,
    bool IsInUse,
    IReadOnlyList<AuthorizationPolicyRuleDto> Rules);

public sealed class AuthorizationPolicyWriteDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ResourceType { get; set; }

    public string? Action { get; set; }

    public List<AuthorizationPolicyRuleWriteDto> Rules { get; set; } = [];
}

public sealed class AuthorizationPolicyRuleWriteDto
{
    public int Order { get; set; }

    public string LeftOperand { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public string RightOperand { get; set; } = string.Empty;
}

public interface IAuthorizationPolicyAdminService
{
    IReadOnlyList<AuthorizationOperandDto> ListOperands();

    IReadOnlyList<string> ListOperators();

    Task<IReadOnlyList<AuthorizationPolicyDto>> ListPoliciesAsync(CancellationToken cancellationToken = default);

    Task<AuthorizationPolicyDto> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);

    Task<AuthorizationPolicyDto> CreatePolicyAsync(
        AuthorizationPolicyWriteDto input,
        CancellationToken cancellationToken = default);

    Task<AuthorizationPolicyDto> UpdatePolicyAsync(
        Guid policyId,
        AuthorizationPolicyWriteDto input,
        CancellationToken cancellationToken = default);

    Task DeletePolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
}
