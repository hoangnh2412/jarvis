namespace Jarvis.Common.Encryption;

/// <summary>
/// Options section <c>Encryption</c> — cấu hình mã hóa dùng chung (không gắn một thuật toán cụ thể).
/// </summary>
/// <remarks>
/// <para>
/// Host đăng ký qua <c>services.AddEncryptionOptions()</c> (idempotent);
/// từng helper/algorithm (vd. <see cref="AesGcmStringEncryptionHelper"/>)
/// tự diễn giải <see cref="DataEncryptionKey"/> theo yêu cầu của mình.
/// </para>
/// Ví dụ:
/// <code>
/// "Encryption": {
///   "DataEncryptionKey": "&lt;Base64 khóa — độ dài phụ thuộc thuật toán đang dùng&gt;"
/// }
/// </code>
/// </remarks>
public class EncryptionOptions
{
    /// <summary>Tên section trong cấu hình host.</summary>
    public const string SectionName = "Encryption";

    /// <summary>
    /// Khóa mã hóa dữ liệu at-rest (thường Base64).
    /// <see cref="AesGcmStringEncryptionHelper"/> yêu cầu Base64 của đúng 32 byte (AES-256).
    /// Không commit khóa thật vào source; dùng secret store / biến môi trường.
    /// </summary>
    public string? DataEncryptionKey { get; set; }
}
