using System.Globalization;
using System.Text.RegularExpressions;
using Jarvis.DDD.Domain.Shared.ExceptionHandling;
using Jarvis.Modules.Setting.Definitions;

namespace Jarvis.Modules.Setting.Validation;

/// <summary>
/// Kiểm tra giá trị plaintext theo quy tắc <see cref="SettingValueTypes"/>
/// (đồng bộ với validator phía frontend Setting).
/// </summary>
public static partial class SettingValueValidator
{
    private static readonly Regex DateRegex = CreateDateRegex();
    private static readonly Regex DateTimeRegex = CreateDateTimeRegex();

    public static void Validate(SettingDefinition? definition, string value)
    {
        var type = definition?.Type ?? SettingValueTypes.Text;
        Validate(type, definition?.Options, value);
    }

    public static void Validate(string type, string? options, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.Equals(type, SettingValueTypes.Text, StringComparison.OrdinalIgnoreCase))
        {
            ValidateMaxLength(options, value);
            return;
        }

        if (string.Equals(type, SettingValueTypes.Textarea, StringComparison.OrdinalIgnoreCase))
        {
            ValidateMaxLength(options, value);
            ValidateTextareaRows(options, value);
            return;
        }

        if (string.Equals(type, SettingValueTypes.Email, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                return;

            ValidateMaxLength(options, trimmed);

            var pattern = SettingTypeOptions.GetEmailRegex(options);
            try
            {
                if (!Regex.IsMatch(trimmed, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                    throw Invalid("Email address is invalid.");
            }
            catch (ArgumentException)
            {
                throw Invalid("Email validation regex in Options is invalid.");
            }

            return;
        }

        if (string.Equals(type, SettingValueTypes.Number, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                throw Invalid("Number value is required.");

            // Value lưu DB luôn invariant (vd. 1234567.89). UI có thể hiển thị dấu chấm ngăn hàng nghìn.
            if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                throw Invalid("Value must be a valid number.");

            var decimals = SettingTypeOptions.GetNumberDecimals(options);
            if (decimals is int maxDecimals)
            {
                var scale = GetInvariantDecimalScale(trimmed);
                if (scale > maxDecimals)
                    throw Invalid($"Number must have at most {maxDecimals} decimal place(s).");
            }

            return;
        }

        if (string.Equals(type, SettingValueTypes.Checkbox, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, SettingValueTypes.Switch, StringComparison.OrdinalIgnoreCase))
        {
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized is not ("true" or "false"))
                throw Invalid($"{type} value must be true or false.");

            return;
        }

        if (string.Equals(type, SettingValueTypes.Combobox, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, SettingValueTypes.Radio, StringComparison.OrdinalIgnoreCase))
        {
            var allowed = ParseOptionValues(options);
            if (allowed.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(value))
                throw Invalid("A value must be selected.");

            if (!allowed.Contains(value))
                throw Invalid("Value is not in the allowed options.");

            return;
        }

        if (string.Equals(type, SettingValueTypes.MultiSelect, StringComparison.OrdinalIgnoreCase))
        {
            var allowed = ParseOptionValues(options);
            if (allowed.Count == 0)
                return;

            var selected = SplitMultiSelect(value);
            if (selected.Count == 0)
                return;

            if (selected.Any(entry => !allowed.Contains(entry)))
                throw Invalid("One or more values are not in the allowed options.");

            return;
        }

        if (string.Equals(type, SettingValueTypes.Date, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                return;

            if (!DateRegex.IsMatch(trimmed)
                || !DateTime.TryParseExact(
                    trimmed,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                throw Invalid("Date value is invalid. Expected yyyy-MM-dd.");
            }

            return;
        }

        if (string.Equals(type, SettingValueTypes.DateTime, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                return;

            if (!DateTimeRegex.IsMatch(trimmed)
                || !DateTime.TryParse(
                    trimmed,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                throw Invalid("DateTime value is invalid. Expected yyyy-MM-ddTHH:mm or yyyy-MM-ddTHH:mm:ss.");
            }

            return;
        }

        if (string.Equals(type, SettingValueTypes.Image, StringComparison.OrdinalIgnoreCase))
        {
            ValidateImage(options, value);
        }
    }

    private static void ValidateMaxLength(string? options, string value)
    {
        var maxLength = SettingTypeOptions.GetMaxLength(options);
        if (maxLength is int max && value.Length > max)
            throw Invalid($"Value must be at most {max} character(s).");
    }

    /// <summary>
    /// Khi Options có <c>rows</c>, giới hạn số dòng (tách bởi <c>\n</c> / <c>\r\n</c>).
    /// Chuỗi rỗng = 1 dòng (ô trống); thiếu key <c>rows</c> = không giới hạn.
    /// </summary>
    private static void ValidateTextareaRows(string? options, string value)
    {
        var maxRows = SettingTypeOptions.GetMaxTextareaRows(options);
        if (maxRows is not int max)
            return;

        var lines = CountTextLines(value);
        if (lines > max)
            throw Invalid($"Value must have at most {max} line(s).");
    }

    private static int CountTextLines(string value)
    {
        if (value.Length == 0)
            return 1;

        var lines = 1;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '\n')
            {
                lines++;
            }
            else if (c == '\r')
            {
                lines++;
                if (i + 1 < value.Length && value[i + 1] == '\n')
                    i++;
            }
        }

        return lines;
    }

    private static void ValidateImage(string? options, string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
            return;

        var allowedMimes = SettingTypeOptions.GetImageMimeTypes(options);
        var maxBytes = SettingTypeOptions.GetMaxImageBytes(options);

        // data:image/{subtype};base64,...
        const string prefix = "data:";
        const string base64Marker = ";base64,";
        if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw Invalid("Image must be a data URL.");

        var markerIndex = trimmed.IndexOf(base64Marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
            throw Invalid("Image must be a data URL with base64 payload.");

        var mime = trimmed[prefix.Length..markerIndex].Trim().ToLowerInvariant();
        if (!allowedMimes.Contains(mime))
            throw Invalid($"Image MIME type '{mime}' is not allowed.");

        var base64 = trimmed[(markerIndex + base64Marker.Length)..];
        if (base64.Length == 0 || !Regex.IsMatch(base64, @"^[A-Za-z0-9+/]+={0,2}$", RegexOptions.CultureInvariant))
            throw Invalid("Image data URL payload is invalid.");

        var decodedBytes = GetBase64DecodedLength(base64);
        if (decodedBytes > maxBytes)
        {
            var mb = maxBytes / (1024d * 1024d);
            throw Invalid($"Image exceeds the maximum size of {mb:0.##}MB.");
        }
    }

    private static int GetInvariantDecimalScale(string raw)
    {
        var trimmed = raw.Trim();
        var sepIndex = trimmed.IndexOf('.');
        if (sepIndex < 0)
            return 0;

        return trimmed.Length - sepIndex - 1;
    }

    private static int GetBase64DecodedLength(string base64)
    {
        var padding = 0;
        if (base64.EndsWith("==", StringComparison.Ordinal))
            padding = 2;
        else if (base64.EndsWith('='))
            padding = 1;

        return (base64.Length * 3 / 4) - padding;
    }

    private static HashSet<string> ParseOptionValues(string? options)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(options))
            return set;

        foreach (var entry in options.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = entry.IndexOf(':');
            var raw = separator < 0 ? entry : entry[..separator];
            set.Add(DecodeOptionPart(raw));
        }

        return set;
    }

    private static List<string> SplitMultiSelect(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static part => part.Length > 0)
            .ToList();
    }

    private static string DecodeOptionPart(string value)
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

    private static BadRequestException Invalid(string message) =>
        new(SettingErrorCode.InvalidValue, message);

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex CreateDateRegex();

    [GeneratedRegex(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2})?$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex CreateDateTimeRegex();
}
