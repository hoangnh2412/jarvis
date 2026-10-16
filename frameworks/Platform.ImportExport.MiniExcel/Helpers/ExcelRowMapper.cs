namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Helper thuần (không MiniExcel / IO) — map một dòng Excel dictionary → FieldName dictionary.
/// </summary>
/// <remarks>
/// Gọi từ <c>MiniExcelImportProcessor.ImportOneSheetAsync</c> sau <c>QueryAsync</c>.
/// Validation rule (required cell, regex, …) thuộc <c>IRowValidator</c>, không nằm ở đây.
/// </remarks>
internal static class ExcelRowMapper
{
    /// <summary>true nếu mọi cell null/whitespace — bỏ qua, không tăng processed count.</summary>
    public static bool IsEmptyRow(IDictionary<string, object> source)
    {
        foreach (var value in source.Values)
        {
            if (!IsEmptyValue(value))
                return false;
        }

        return true;
    }

    /// <summary>null hoặc string trắng = empty; số/bool/DateTime không trống.</summary>
    public static bool IsEmptyValue(object? value) =>
        value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            _ => false
        };

    /// <summary>
    /// Build ColumnName (mapping) → key header thực trong row MiniExcel (giữ key gốc để đọc cell).
    /// Chỉ các cột có trong mappings; so khớp trim + không phân biệt hoa thường.
    /// </summary>
    public static Dictionary<string, string> BuildColumnKeyMap(
        IDictionary<string, object> source,
        IReadOnlyCollection<ColumnMapping> mappings)
    {
        var map = new Dictionary<string, string>(mappings.Count, StringComparer.OrdinalIgnoreCase);
        var sourceKeys = new Dictionary<string, string>(source.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var key in source.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            // Lookup theo tên đã trim; value = key gốc MiniExcel (có thể còn space).
            sourceKeys[key.Trim()] = key;
        }

        foreach (var mapping in mappings)
        {
            if (sourceKeys.TryGetValue(mapping.ColumnName.Trim(), out var actualKey))
                map[mapping.ColumnName] = actualKey;
        }

        return map;
    }

    /// <summary>Copy mọi cột (khi không có ColumnMapping) — key = header Excel.</summary>
    public static Dictionary<string, object?> MapAllColumns(IDictionary<string, object> source)
    {
        var mapped = new Dictionary<string, object?>(source.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in source)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            mapped[key] = value;
        }

        return mapped;
    }

    /// <summary>
    /// Fail-fast chỉ khi cột <see cref="ColumnMapping.Required"/> thiếu trên header Excel.
    /// Cột optional thiếu được phép (cell = empty). Cột thừa trên file bị bỏ qua.
    /// </summary>
    public static void EnsureMappedColumnsExist(
        IReadOnlyCollection<ColumnMapping> mappings,
        IReadOnlyDictionary<string, string> columnKeyMap)
    {
        foreach (var mapping in mappings)
        {
            if (!mapping.Required)
                continue;

            if (!columnKeyMap.ContainsKey(mapping.ColumnName))
                throw new InvalidTemplateException(EngineMessages.ColumnNotFound(mapping.ColumnName));
        }
    }

    /// <summary>
    /// Map một data row: <c>FieldName → value</c>. Không validate Required/rules (xem <c>IRowValidator</c>).
    /// </summary>
    public static Dictionary<string, object?> MapRow(
        IDictionary<string, object> source,
        IReadOnlyCollection<ColumnMapping> mappings,
        IReadOnlyDictionary<string, string> columnKeyMap)
    {
        var mapped = new Dictionary<string, object?>(mappings.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in mappings)
        {
            columnKeyMap.TryGetValue(mapping.ColumnName, out var actualKey);
            object? value = null;
            if (actualKey is not null)
                source.TryGetValue(actualKey, out value);

            mapped[mapping.FieldName] = value;
        }

        return mapped;
    }
}
