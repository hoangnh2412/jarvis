namespace Platform.Authorization.Abstractions;

/// <summary>
/// Một vai trò tham chiếu các mã chính sách không tồn tại trong kho lưu trữ chính sách (cấu hình sai).
/// </summary>
public sealed class UnresolvedPolicyException : InvalidOperationException
{
    public UnresolvedPolicyException(Guid roleId, IReadOnlyCollection<string> missingCodes)
        : base($"Vai trò {roleId} tham chiếu các mã chính sách không rõ: {string.Join(", ", missingCodes)}")
    {
        RoleId = roleId;
        MissingCodes = missingCodes;
    }

    public Guid RoleId { get; }

    public IReadOnlyCollection<string> MissingCodes { get; }
}
