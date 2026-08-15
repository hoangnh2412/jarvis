using System.Security.Cryptography;
using System.Text;

namespace Jarvis.Common.Encryption;

/// <summary>
/// Helper AES-GCM mã hóa/giải mã chuỗi. Ciphertext format: <c>enc.v1.{base64(nonce|tag|ciphertext)}</c>.
/// Không đăng ký DI — caller truyền Base64 key (vd. từ <c>Encryption:DataEncryptionKey</c>).
/// </summary>
public static class AesGcmStringEncryptionHelper
{
    /// <summary>
    /// Nhãn envelope cho ciphertext lưu dạng string:
    /// <c>enc.v1.{base64(nonce | tag | ciphertext)}</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mục đích: (1) nhận diện giá trị đã mã hóa vs plaintext;
    /// (2) version format để sau này đổi layout/thuật toán (<c>enc.v2.</c>) mà vẫn đọc được dữ liệu cũ.
    /// Pattern tương tự Vault Transit (<c>vault:v1:...</c>) và ý tưởng versioning của Fernet.
    /// </para>
    /// <para>
    /// Tham khảo:
    /// <list type="bullet">
    /// <item>
    /// HashiCorp Vault Transit — ciphertext prefix + key/format version:
    /// https://developer.hashicorp.com/vault/docs/secrets/transit
    /// </item>
    /// <item>
    /// Fernet Spec — version byte trong token layout:
    /// https://github.com/fernet/spec/blob/master/Spec.md
    /// </item>
    /// <item>
    /// NIST SP 800-38D — AES-GCM (nonce/IV, tag length):
    /// https://csrc.nist.gov/pubs/sp/800/38/d/final
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Nonce (IV)</b> — xem <see cref="NonceSize"/>.
    /// <b>Tag</b> — xem <see cref="TagSize"/>.
    /// Đổi kích thước nonce/tag = đổi wire format → bump prefix (vd. <c>enc.v2.</c>) và migration decrypt.
    /// </para>
    /// </remarks>
    public const string Prefix = "enc.v1.";

    /// <summary>
    /// Kích thước nonce/IV (12 byte = 96-bit) — khuyến nghị mặc định của AES-GCM (NIST SP 800-38D).
    /// Mỗi lần Encrypt phải dùng nonce mới (random); không tái sử dụng nonce với cùng key.
    /// Chỉ đổi khi tích hợp hệ thống bắt buộc độ dài khác, hoặc chuyển AEAD khác — khi đó bump <see cref="Prefix"/>.
    /// </summary>
    private const int NonceSize = 12;

    /// <summary>
    /// Kích thước authentication tag (16 byte = 128-bit) — mức mạnh mặc định của AES-GCM.
    /// Chỉ rút ngắn khi bị ràng buộc tương thích (hiếm khi nên &lt; 128-bit; NIST cho phép ngắn hơn nhưng giảm an toàn).
    /// Đổi tag size cũng là breaking format → bump <see cref="Prefix"/> + migration.
    /// </summary>
    private const int TagSize = 16;

    private const int KeySize = 32;

    /// <summary>Mã hóa plaintext; trả về chuỗi có <see cref="Prefix"/>.</summary>
    /// <param name="plaintext">Giá trị gốc.</param>
    /// <param name="dataEncryptionKeyBase64">Khóa AES-256 dạng Base64 (32 byte sau decode).</param>
    public static string Encrypt(string plaintext, string dataEncryptionKeyBase64)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var key = ResolveKey(dataEncryptionKeyBase64);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var payload = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, payload, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, payload, NonceSize + TagSize, ciphertext.Length);

        return Prefix + Convert.ToBase64String(payload);
    }

    /// <summary>
    /// Giải mã ciphertext. Nếu không có <see cref="Prefix"/> thì trả nguyên chuỗi (coi như plaintext).
    /// </summary>
    /// <param name="ciphertext">Chuỗi đã mã hóa hoặc plaintext.</param>
    /// <param name="dataEncryptionKeyBase64">Khóa AES-256 dạng Base64 (32 byte sau decode).</param>
    public static string Decrypt(string ciphertext, string dataEncryptionKeyBase64)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);

        if (!ciphertext.StartsWith(Prefix, StringComparison.Ordinal))
            return ciphertext;

        var key = ResolveKey(dataEncryptionKeyBase64);
        var payload = Convert.FromBase64String(ciphertext[Prefix.Length..]);
        if (payload.Length < NonceSize + TagSize)
            throw new InvalidOperationException("Encrypted payload is invalid.");

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var encrypted = payload.AsSpan(NonceSize + TagSize);
        var plaintextBytes = new byte[encrypted.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, encrypted, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    /// <summary>True nếu giá trị có prefix ciphertext của helper này.</summary>
    public static bool IsEncryptedPayload(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);

    private static byte[] ResolveKey(string? dataEncryptionKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(dataEncryptionKeyBase64))
        {
            throw new InvalidOperationException(
                "Encryption:DataEncryptionKey is required when encrypted values are used.");
        }

        try
        {
            var key = Convert.FromBase64String(dataEncryptionKeyBase64);
            if (key.Length != KeySize)
            {
                throw new InvalidOperationException(
                    "Encryption:DataEncryptionKey must be a Base64-encoded 256-bit (32-byte) key.");
            }

            return key;
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Encryption:DataEncryptionKey must be valid Base64.",
                ex);
        }
    }
}
