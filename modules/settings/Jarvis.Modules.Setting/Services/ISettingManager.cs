using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Models;

namespace Jarvis.Modules.Setting.Services;

/// <summary>
/// Facade ứng dụng cho SettingManagement.
/// UI và service nghiệp vụ chỉ gọi API này — không query <c>DbSet&lt;Setting&gt;</c> trực tiếp.
/// </summary>
/// <remarks>
/// Trách nhiệm của Manager:
/// <list type="bullet">
/// <item>Đọc metadata từ Library (Group/Definition).</item>
/// <item>Đọc/ghi Value theo working tenant (<c>ICurrentTenant&lt;TTenant&gt;.GetIdAsync</c>) qua Unit of Work.</item>
/// <item>Cache theo Tenant + Key; invalidate khi ghi.</item>
/// <item>Mã hóa secret khi lưu; luôn trả plaintext cho caller.</item>
/// <item>Chặn sửa/xóa khi <c>IsReadOnly</c>.</item>
/// </list>
/// Host đăng ký <c>AddCurrentTenant&lt;TTenant&gt;</c> (không <c>IWorkContext</c>). Auth/user thuộc host — Manager không inject <c>ICurrentUser</c>.
/// </remarks>
public interface ISettingManager
{
    /// <summary>
    /// Lấy danh sách nhóm cấu hình từ Library (không đụng DB).
    /// Dùng để vẽ menu / tab trên UI quản trị.
    /// </summary>
    IReadOnlyList<SettingGroupDefinition> GetGroups();

    /// <summary>
    /// Lấy danh sách definition Key từ Library (không đụng DB).
    /// </summary>
    /// <param name="group">Lọc theo nhóm; null = tất cả.</param>
    IReadOnlyList<SettingDefinition> GetDefinitions(string? group = null);

    /// <summary>
    /// Đọc một cấu hình theo Key: cache → DB → decrypt (nếu secret).
    /// </summary>
    /// <param name="key">Mã cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Model plaintext, hoặc null nếu tenant chưa có bản ghi.</returns>
    Task<SettingModel?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đọc toàn bộ bản ghi đã lưu của một Group (không cache theo group).
    /// Chỉ trả các Key đã có row trong DB — khác với <see cref="GetFormAsync"/> có kèm default.
    /// </summary>
    /// <param name="group">Mã nhóm.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task<IReadOnlyList<SettingModel>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dựng payload form UI: mọi definition trong Group ghép với Value đã lưu (hoặc DefaultValue).
    /// Đây là API chính để frontend render form trong một lần gọi.
    /// </summary>
    /// <param name="group">Mã nhóm.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <exception cref="Jarvis.DDD.Domain.Shared.ExceptionHandling.NotFoundException">Group không có definition nào.</exception>
    Task<IReadOnlyList<SettingFormItemModel>> GetFormAsync(string group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo mới một row từ definition đã đăng ký (runtime create — không seed sẵn).
    /// </summary>
    /// <param name="key">Mã cấu hình phải có trong Library.</param>
    /// <param name="value">Giá trị; null thì lấy DefaultValue của definition.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <exception cref="Jarvis.DDD.Domain.Shared.ExceptionHandling.NotFoundException">Definition chưa đăng ký.</exception>
    /// <exception cref="Jarvis.DDD.Domain.Shared.ExceptionHandling.ConflictException">Key đã tồn tại trong tenant.</exception>
    Task<SettingModel> CreateAsync(string key, string? value = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Có thì lấy; chưa có thì tạo từ definition.
    /// An toàn với race: nếu nhiều request cùng tạo, request thua unique index sẽ đọc lại row thắng và trả về (không ném lỗi cho caller).
    /// Phù hợp service nghiệp vụ đọc cấu hình runtime (ví dụ SMTP).
    /// </summary>
    /// <param name="key">Mã cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task<SettingModel> GetOrCreateAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật Value theo Key. Key phải đã đăng ký trong Library.
    /// Password/encrypted: value rỗng/whitespace → <c>98107</c>, không mã hóa và không ghi.
    /// </summary>
    /// <param name="key">Mã cấu hình.</param>
    /// <param name="value">Giá trị mới (plaintext).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <exception cref="Jarvis.DDD.Domain.Shared.ExceptionHandling.NotFoundException">
    /// Definition không có trong Library, hoặc không có bản ghi DB.
    /// </exception>
    /// <exception cref="Jarvis.DDD.Domain.Shared.ExceptionHandling.BadRequestException">IsReadOnly hoặc secret rỗng.</exception>
    Task<SettingModel> UpdateAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu nhiều Key của một Group trong một lần (partial upsert — chỉ các Key được gửi).
    /// Key phải thuộc Group và đã đăng ký trong Library.
    /// Password/encrypted gửi rỗng → <c>98107</c> (không mã hóa, không lưu). Client nên bỏ key khỏi payload nếu không đổi secret.
    /// Chặn khi definition hoặc entity <c>IsReadOnly</c>.
    /// </summary>
    /// <param name="group">Mã nhóm.</param>
    /// <param name="values">Dictionary Key → Value plaintext từ form.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task<IReadOnlyList<SettingModel>> SaveGroupAsync(
        string group,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa bản ghi theo Key; chặn nếu entity hoặc definition <c>IsReadOnly</c>; invalidate cache sau khi xóa.
    /// </summary>
    /// <param name="key">Mã cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
