using System.Globalization;
using System.Text.RegularExpressions;
using Platform.ImportExport.MiniExcel.Helpers;

namespace Platform.ImportExport.MiniExcel.Validation;

/// <summary>Default cell rules: Required → MaxLength → Regex → Min/Max → Unique (file + store).</summary>
public sealed class DefaultRowValidator(IUniqueValueChecker uniqueValueChecker) : IRowValidator
{
    public async Task<IReadOnlyList<ImportRowError>> ValidateAsync(
        IReadOnlyDictionary<string, object?> row,
        RowValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ImportRowError>();

        foreach (var mapping in context.Mappings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            row.TryGetValue(mapping.FieldName, out var value);
            var text = NormalizeText(value);

            if (mapping.Required && ExcelRowMapper.IsEmptyValue(value))
            {
                errors.Add(Error(
                    context,
                    mapping,
                    EngineMessages.FieldRequired(mapping.ColumnName, mapping.FieldName),
                    ImportErrorCodes.FieldRequired));
                continue;
            }

            if (ExcelRowMapper.IsEmptyValue(value))
                continue;

            if (mapping.MinLength is { } minLength && text.Length < minLength)
            {
                errors.Add(Error(
                    context,
                    mapping,
                    EngineMessages.FieldMinLengthNotMet(mapping.ColumnName, mapping.FieldName, minLength),
                    ImportErrorCodes.MinLengthNotMet));
                continue;
            }

            if (mapping.MaxLength is { } maxLength && text.Length > maxLength)
            {
                errors.Add(Error(
                    context,
                    mapping,
                    EngineMessages.FieldMaxLengthExceeded(mapping.ColumnName, mapping.FieldName, maxLength),
                    ImportErrorCodes.MaxLengthExceeded));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(mapping.Regex))
            {
                try
                {
                    if (!Regex.IsMatch(text, mapping.Regex, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                    {
                        errors.Add(Error(
                            context,
                            mapping,
                            EngineMessages.FieldInvalidFormat(mapping.ColumnName, mapping.FieldName),
                            ImportErrorCodes.InvalidFormat));
                        continue;
                    }
                }
                catch (RegexParseException)
                {
                    errors.Add(Error(
                        context,
                        mapping,
                        EngineMessages.FieldInvalidFormat(mapping.ColumnName, mapping.FieldName),
                        ImportErrorCodes.InvalidFormat));
                    continue;
                }
            }

            if (mapping.Min is not null || mapping.Max is not null)
            {
                if (!TryParseDecimal(value, out var number))
                {
                    errors.Add(Error(
                        context,
                        mapping,
                        EngineMessages.FieldInvalidFormat(mapping.ColumnName, mapping.FieldName),
                        ImportErrorCodes.InvalidFormat));
                    continue;
                }

                if ((mapping.Min is { } min && number < min) ||
                    (mapping.Max is { } max && number > max))
                {
                    errors.Add(Error(
                        context,
                        mapping,
                        EngineMessages.FieldOutOfRange(mapping.ColumnName, mapping.FieldName, mapping.Min, mapping.Max),
                        ImportErrorCodes.OutOfRange));
                    continue;
                }
            }

            if (!mapping.Unique)
                continue;

            if (!context.SeenUniqueValues.TryGetValue(mapping.FieldName, out var seen))
            {
                seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                context.SeenUniqueValues[mapping.FieldName] = seen;
            }

            if (!seen.Add(text))
            {
                errors.Add(Error(
                    context,
                    mapping,
                    EngineMessages.FieldDuplicateValue(mapping.ColumnName, mapping.FieldName),
                    ImportErrorCodes.DuplicateValue));
                continue;
            }

            if (await uniqueValueChecker.ExistsAsync(mapping.FieldName, value, cancellationToken))
            {
                errors.Add(Error(
                    context,
                    mapping,
                    EngineMessages.FieldDuplicateInStore(mapping.ColumnName, mapping.FieldName),
                    ImportErrorCodes.DuplicateInStore));
            }
        }

        return errors;
    }

    private static ImportRowError Error(
        RowValidationContext context,
        ColumnMapping mapping,
        string message,
        string errorCode) =>
        new(
            context.ExcelRowNumber,
            message,
            mapping.ColumnName,
            mapping.FieldName,
            errorCode,
            context.SheetName);

    private static string NormalizeText(object? value) =>
        value switch
        {
            null => string.Empty,
            string s => s.Trim(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
            _ => value.ToString()?.Trim() ?? string.Empty
        };

    private static bool TryParseDecimal(object? value, out decimal number)
    {
        switch (value)
        {
            case decimal d:
                number = d;
                return true;
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case double dbl:
                number = (decimal)dbl;
                return true;
            case float fl:
                number = (decimal)fl;
                return true;
            default:
                return decimal.TryParse(
                    NormalizeText(value),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out number);
        }
    }
}
