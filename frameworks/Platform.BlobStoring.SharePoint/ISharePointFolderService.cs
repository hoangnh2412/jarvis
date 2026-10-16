// Namespace chứa contract và DTO riêng của provider SharePoint, tách khỏi abstraction lưu blob dùng chung.
namespace Platform.BlobStoring.SharePoint;

/// <summary>
/// Contract riêng của SharePoint để duyệt cây thư mục và file (lazy, từng cấp) bên dưới Document Library.
/// </summary>
/// <remarks>
/// Interface này bổ sung khả năng duyệt cây, không thay thế <see cref="IBlobStoringService"/>
/// vốn cung cấp upload, download, delete và liệt kê object key dạng phẳng.
/// Đăng ký qua DI giúp consumer phụ thuộc vào abstraction thay vì implementation Microsoft Graph cụ thể.
/// </remarks>
public interface ISharePointFolderService
{
    /// <summary>
    /// Lazy tree: liệt kê con trực tiếp dưới <paramref name="rootPath"/>; folder con có <c>Children</c> rỗng.
    /// </summary>
    /// <param name="rootPath">
    /// Đường dẫn tương đối tính từ Document Library root; <see langword="null"/> hoặc chuỗi rỗng
    /// nghĩa là bắt đầu ngay tại root.
    /// </param>
    /// <param name="cancellationToken">
    /// Token truyền xuyên suốt các lời gọi bất đồng bộ để caller có thể hủy việc duyệt cây,
    /// chẳng hạn khi HTTP request bị ngắt hoặc ứng dụng shutdown.
    /// </param>
    /// <returns>
    /// Task đại diện cho thao tác bất đồng bộ; kết quả là node gốc với children trực tiếp (không đệ quy sâu).
    /// </returns>
    // rootPath nullable và có default null để caller có thể bỏ qua khi muốn duyệt toàn bộ library.
    Task<SharePointDriveNode> GetTreeAsync(
        string? rootPath = null,
        // default tạo CancellationToken.None, nhưng vẫn cho phép caller truyền token để hủy cooperative.
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO biểu diễn một node thư mục hoặc file trong cây SharePoint Drive.
/// </summary>
/// <remarks>
/// DTO chỉ mang dữ liệu qua ranh giới service; các property dùng <c>init</c> để chỉ được gán
/// trong object initializer/constructor và không bị thay đổi tùy ý sau khi khởi tạo.
/// </remarks>
public sealed class SharePointDriveNode
{
    /// <summary>
    /// Tên hiển thị của file hoặc thư mục tại node hiện tại.
    /// </summary>
    // required buộc caller gán giá trị khi khởi tạo object; init giữ property ổn định sau giai đoạn khởi tạo.
    public required string Name { get; init; }

    /// <summary>
    /// Đường dẫn tương đối tính từ Document Library root, ví dụ <c>invoices/2024/a.pdf</c>.
    /// </summary>
    // required ngăn tạo DTO thiếu định danh đường dẫn; kiểu string non-null được nullable analysis kiểm tra.
    public required string Path { get; init; }

    /// <summary>
    /// Cho biết node là thư mục; <see langword="false"/> biểu thị node file.
    /// </summary>
    // bool mặc định false phù hợp với node file nếu initializer không gán rõ giá trị.
    public bool IsFolder { get; init; }

    /// <summary>
    /// Danh sách node con; file hoặc thư mục rỗng có danh sách không phần tử.
    /// </summary>
    // IReadOnlyList chỉ cho consumer đọc; [] tạo collection rỗng non-null để tránh kiểm tra null.
    public IReadOnlyList<SharePointDriveNode> Children { get; init; } = [];
}
