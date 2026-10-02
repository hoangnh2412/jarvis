namespace Platform.Authorization.Abstractions;

using Platform.Authorization.Domain;

/// <summary>
/// Cung cấp các chính sách cho đánh giá ABAC.
/// Thường được lưu trữ trong bộ nhớ cache để nâng cao hiệu suất.
/// </summary>
public interface IPolicyProvider
{
    /// <summary>
    /// Lấy tất cả các chính sách được gán cho một vai trò cụ thể.
    /// Trả về bộ sưu tập các chính sách; mỗi chính sách bao gồm các quy tắc của nó.
    /// Ném <see cref="UnresolvedPolicyException"/> nếu vai trò tham chiếu mã còn thiếu từ kho lưu trữ,
    /// do đó đường dẫn AND không bao giờ có thể mất đi điều kiện một cách âm thầm.
    /// </summary>
    Task<IEnumerable<Policy>> GetPoliciesByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả các chính sách được gán cho người dùng thông qua các vai trò của họ.
    /// Được loại bỏ trùng lặp (hợp nhất tất cả các chính sách của vai trò).
    /// </summary>
    Task<IEnumerable<Policy>> GetPoliciesByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chính sách cụ thể theo code.
    /// Ném nếu không tìm thấy hoặc trả về null dựa trên ngữ cảnh gọi.
    /// </summary>
    Task<Policy?> GetPolicyByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chính sách cụ thể theo ID.
    /// </summary>
    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả các chính sách hệ thống (dữ liệu seed).
    /// </summary>
    Task<IEnumerable<Policy>> GetSystemPoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Làm mất hiệu lực bộ nhớ cache (được gọi khi chính sách/quy tắc thay đổi).
    /// Không hoạt động nếu caching chưa được triển khai.
    /// </summary>
    Task InvalidateCacheAsync(CancellationToken cancellationToken = default);
}
