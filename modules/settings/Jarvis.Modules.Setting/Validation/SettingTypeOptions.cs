using System.Globalization;
using System.Text;

namespace Jarvis.Modules.Setting.Validation;

/// <summary>
/// Parse / build chuỗi <c>Options</c>. Ngữ nghĩa phụ thuộc <c>Type</c>:
/// <list type="bullet">
/// <item>
/// <b>Choice</b> (Combobox / Radio / MultiSelect): danh sách chọn
/// <c>value:label|value:label|...</c> — không dùng helper này để parse choice.
/// </item>
/// <item>
/// <b>Constraint</b> (Text / Textarea / Number / Email / Image…): điều kiện giá trị
/// <c>key:value|key:value|...</c> (value Uri-escape khi có <c>|</c> / <c>:</c>).
/// </item>
/// </list>
/// </summary>
public static class SettingTypeOptions
{
    public const string KeyRegex = "regex";
    public const string KeyMaxBytes = "maxBytes";
    public const string KeyMimeTypes = "mimeTypes";
    public const string KeyDecimals = "decimals";
    public const string KeyRows = "rows";
    public const string KeyMaxLength = "maxLength";

    /// <summary>
    /// Token ngắn lưu trong Options thay cho pattern email đầy đủ.
    /// <see cref="GetEmailRegex"/> resolve về <see cref="DefaultEmailRegex"/>.
    /// </summary>
    public const string RegexTokenDefault = "default";

    /// <summary>Regex email mặc định (trong code; không snapshot nguyên pattern xuống DB).</summary>
    public const string DefaultEmailRegex =
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$";

    public const int DefaultMaxImageBytes = 2 * 1024 * 1024;

    public static readonly string[] DefaultImageMimeTypes =
    [
        "image/png",
        "image/jpeg",
        "image/gif",
        "image/webp",
    ];

    public const int DefaultTextareaRows = 3;

    public static IReadOnlyDictionary<string, string> Parse(string? options)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(options))
            return map;

        foreach (var entry in options.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = entry.IndexOf(':');
            if (separator <= 0)
                continue;

            var key = Decode(entry[..separator]);
            var value = Decode(entry[(separator + 1)..]);
            if (key.Length == 0)
                continue;

            map[key] = value;
        }

        return map;
    }

    public static string Build(params (string Key, string Value)[] pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);
        var sb = new StringBuilder();
        foreach (var (key, value) in pairs)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (sb.Length > 0)
                sb.Append('|');

            sb.Append(Uri.EscapeDataString(key.Trim()));
            sb.Append(':');
            sb.Append(Uri.EscapeDataString(value ?? string.Empty));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Options constraint cho Email: luôn có <c>regex</c> trong Options.
    /// Không truyền pattern → lưu token <see cref="RegexTokenDefault"/> (ngắn trên DB);
    /// truyền pattern → lưu pattern đó (Uri-escape).
    /// </summary>
    public static string ForEmail(string? regex = null) =>
        Build((KeyRegex, string.IsNullOrWhiteSpace(regex) ? RegexTokenDefault : regex));

    public static string ForImage(int maxBytes = DefaultMaxImageBytes, IEnumerable<string>? mimeTypes = null)
    {
        var mimes = mimeTypes?.Where(static m => !string.IsNullOrWhiteSpace(m)).Select(static m => m.Trim()).ToArray()
            ?? DefaultImageMimeTypes;
        return Build(
            (KeyMaxBytes, maxBytes.ToString(CultureInfo.InvariantCulture)),
            (KeyMimeTypes, string.Join(',', mimes)));
    }

    public static string ForNumber(int decimals) =>
        Build((KeyDecimals, decimals.ToString(CultureInfo.InvariantCulture)));

    /// <summary>Options constraint cho Text: <c>maxLength</c> (số ký tự tối đa).</summary>
    public static string ForText(int maxLength) =>
        Build((KeyMaxLength, maxLength.ToString(CultureInfo.InvariantCulture)));

    /// <summary>
    /// Options constraint cho Textarea: <c>rows</c> = chiều cao UI và số dòng tối đa khi nhập.
    /// </summary>
    public static string ForTextarea(int rows) =>
        Build((KeyRows, rows.ToString(CultureInfo.InvariantCulture)));

    /// <summary>
    /// Lấy pattern email từ Options: thiếu / <c>default</c> → <see cref="DefaultEmailRegex"/>;
    /// khác → dùng đúng chuỗi điều kiện đã lưu.
    /// </summary>
    public static string GetEmailRegex(string? options)
    {
        var map = Parse(options);
        if (!map.TryGetValue(KeyRegex, out var regex) || string.IsNullOrWhiteSpace(regex))
            return DefaultEmailRegex;

        if (string.Equals(regex, RegexTokenDefault, StringComparison.OrdinalIgnoreCase))
            return DefaultEmailRegex;

        return regex;
    }

    public static int GetMaxImageBytes(string? options)
    {
        var map = Parse(options);
        if (map.TryGetValue(KeyMaxBytes, out var raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes)
            && bytes > 0)
        {
            return bytes;
        }

        return DefaultMaxImageBytes;
    }

    public static HashSet<string> GetImageMimeTypes(string? options)
    {
        var map = Parse(options);
        if (map.TryGetValue(KeyMimeTypes, out var raw) && !string.IsNullOrWhiteSpace(raw))
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var mime = part.Contains('/') ? part : $"image/{part}";
                set.Add(mime);
            }

            if (set.Count > 0)
                return set;
        }

        return new HashSet<string>(DefaultImageMimeTypes, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Số chữ số sau dấu thập phân. <c>null</c> = không giới hạn (chỉ format ngăn cách hàng nghìn trên UI).
    /// </summary>
    public static int? GetNumberDecimals(string? options)
    {
        var map = Parse(options);
        if (map.TryGetValue(KeyDecimals, out var raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var decimals)
            && decimals >= 0)
        {
            return decimals;
        }

        return null;
    }

    /// <summary>Số dòng hiển thị UI; thiếu <c>rows</c> → <see cref="DefaultTextareaRows"/>.</summary>
    public static int GetTextareaRows(string? options) =>
        GetMaxTextareaRows(options) ?? DefaultTextareaRows;

    /// <summary>
    /// Giới hạn số dòng khi Options có <c>rows</c>; <c>null</c> = không giới hạn (chỉ dùng default cho chiều cao UI).
    /// </summary>
    public static int? GetMaxTextareaRows(string? options)
    {
        var map = Parse(options);
        if (map.TryGetValue(KeyRows, out var raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rows)
            && rows > 0)
        {
            return rows;
        }

        return null;
    }

    /// <summary>Độ dài tối đa; <c>null</c> = không giới hạn.</summary>
    public static int? GetMaxLength(string? options)
    {
        var map = Parse(options);
        if (map.TryGetValue(KeyMaxLength, out var raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxLength)
            && maxLength > 0)
        {
            return maxLength;
        }

        return null;
    }

    private static string Decode(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value.Replace("+", "%2B", StringComparison.Ordinal));
        }
        catch (UriFormatException)
        {
            return value;
        }
    }
}
