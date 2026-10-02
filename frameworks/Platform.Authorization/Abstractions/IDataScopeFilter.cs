namespace Platform.Authorization.Abstractions;

/// <summary>
/// Áp dụng các bộ lọc phạm vi dữ liệu ABAC cho các truy vấn.
/// Thu hẹp kết quả dựa trên quyền hạn + chính sách của người dùng.
/// Được sử dụng trong các endpoint danh sách/tìm kiếm để đảm bảo người dùng chỉ xem dữ liệu mà họ được ủy quyền.
/// </summary>
public interface IDataScopeFilter
{
    /// <summary>
    /// Áp dụng phạm vi ủy quyền cho IQueryable.
    /// Lọc kết quả để chỉ bao gồm các thực thể mà người dùng có thể truy cập dựa trên các chính sách được gán.
    /// Ví dụ: Danh sách nhân viên cho Trưởng phòng chỉ nên bao gồm nhân viên trong cùng bộ phận.
    ///
    /// Trả về queryable được lọc; loại T phải khớp với thực thể đang được truy vấn.
    /// </summary>
    Task<IQueryable<T>> ApplyFilterAsync<T>(
        IQueryable<T> query,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default) where T : class;
}
