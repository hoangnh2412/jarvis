namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>MiniExcel SaveAsByTemplate conventions for export templates.</summary>
public static class ExportTemplateConstants
{
    /// <summary>
    /// Collection name in template placeholders, e.g. <c>{{rows.CustomerCode}}</c>.
    /// Must match <see cref="Models.ColumnMapping.FieldName"/> after the dot.
    /// </summary>
    public const string RowsCollectionName = "rows";
}