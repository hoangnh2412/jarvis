using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Domain.Constants;

public static class EngineMessages
{
    public const string DefaultExportSheetName = "Sheet1";
    public const string DefaultExportFileName = "export.xlsx";
    public const string XlsxExtension = ".xlsx";
    public const string CsvExtension = ".csv";
    public const string FirstSheetPlaceholder = "(first)";

    public const string RequestRequired = "Import/export request is required.";
    public const string FileStreamRequired = "File stream is required.";
    public const string FileNameRequired = "File name is required.";
    public const string TemplateRequired = "Template configuration is required.";
    public const string TemplateContentRequired = "Template file content is required.";
    public const string TemplateNameRequired = "Template name is required.";
    public const string TemplateCodeRequired = "Template code is required.";
    public const string TenantRequired = "Tenant context is required to access templates.";
    public const string DataRequired = "Export data is required.";
    public const string MappingsRequired = "At least one column mapping is required.";

    public static readonly string ColumnNameRequired =
        $"{nameof(ColumnMapping.ColumnName)} is required. Each mapping must include property '{MappingJsonConstants.Properties.ColumnName}'.";

    public static readonly string FieldNameRequired =
        $"{nameof(ColumnMapping.FieldName)} is required. Each mapping must include property '{MappingJsonConstants.Properties.FieldName}'.";

    public static readonly string ColumnNameUnique =
        $"{nameof(ColumnMapping.ColumnName)} values must be unique.";

    public static readonly string FieldNameUnique =
        $"{nameof(ColumnMapping.FieldName)} values must be unique.";

    public static string UnknownMappingProperty(string propertyName) =>
        $"Unknown mapping property '{propertyName}'. Expected properties: " +
        $"{MappingJsonConstants.Properties.ColumnName}, {MappingJsonConstants.Properties.FieldName}, " +
        $"{MappingJsonConstants.Properties.Required}, {MappingJsonConstants.Properties.Unique}, " +
        $"{MappingJsonConstants.Properties.Regex}, {MappingJsonConstants.Properties.MaxLength}, " +
        $"{MappingJsonConstants.Properties.Min}, {MappingJsonConstants.Properties.Max}.";

    public static string FieldMaxLengthExceeded(string columnName, string fieldName, int maxLength) =>
        $"Column '{columnName}' (field '{fieldName}') exceeds max length {maxLength}.";

    public static string FieldInvalidFormat(string columnName, string fieldName) =>
        $"Column '{columnName}' (field '{fieldName}') has an invalid format.";

    public static string FieldOutOfRange(string columnName, string fieldName, decimal? min, decimal? max) =>
        $"Column '{columnName}' (field '{fieldName}') is out of range" +
        (min is null ? "" : $" (min {min})") +
        (max is null ? "" : $" (max {max})") + ".";

    public static string FieldDuplicateValue(string columnName, string fieldName) =>
        $"Column '{columnName}' (field '{fieldName}') has a duplicate value in the file.";

    public static string FieldDuplicateInStore(string columnName, string fieldName) =>
        $"Column '{columnName}' (field '{fieldName}') already exists in the data store.";

    public const string InvalidMappingFormat = "Invalid mapping format. Expected a JSON object or array of mappings.";

    public static string InvalidMappingsJson(string detail, string preview) =>
        $"Invalid Mappings JSON: {detail}. Received (preview): {preview}";

    public static string MappingsMustBeJson(string preview) =>
        $"Mappings must be a JSON array or object. Received (preview): {preview}";

    public const string InvalidRowStructure = "Invalid row structure.";
    public const string FailedToParseExcel = "Failed to parse the Excel/CSV file.";
    public const string FailedToExportExcel = "Failed to export Excel file.";
    public const string AsyncModeNotSupported = "ExecutionMode.Async is reserved for the GD2 job executor and is not enabled in this release.";
    public const string RequestCanceled = "The request was canceled.";
    public const string ApplicationTerminatedUnexpectedly = "Application terminated unexpectedly";

    public static string ColumnNotFound(string columnName) =>
        $"Column '{columnName}' not found.";

    public static string FieldRequired(string columnName, string fieldName) =>
        $"Column '{columnName}' (field '{fieldName}') is required.";

    public static string StartRowInvalid() =>
        $"{nameof(TemplateConfiguration.StartRow)} must be greater than or equal to 1.";

    public static string HeaderRowInvalid() =>
        $"{nameof(TemplateConfiguration.HeaderRow)} must be greater than or equal to 1.";

    public static string HeaderRowMustPrecedeStartRow() =>
        $"{nameof(TemplateConfiguration.HeaderRow)} must be less than {nameof(TemplateConfiguration.StartRow)}.";

    public static string RowCountInvalid() =>
        $"{nameof(TemplateConfiguration.RowCount)} must be greater than 0 when specified.";

    public static string ColumnLimitExceeded(int actual, int max) =>
        $"Column count '{actual:N0}' exceeds configured limit '{max:N0}'.";

    public static string SheetNameRequiredForMultiSheet =>
        "Each entry in sheets[] must include sheetName when configuring multiple sheets.";

    public static string SheetNameRequiredForExcel =>
        "sheetName is required when the template file is Excel (.xlsx).";

    public static string MappingMinLengthExceedsMaxLength(string fieldName) =>
        $"Field '{fieldName}': minLength must be less than or equal to maxLength.";

    public static string FieldMinLengthNotMet(string columnName, string fieldName, int minLength) =>
        $"Column '{columnName}' (field '{fieldName}') must be at least {minLength} character(s).";

    public static string SheetNotFound(string requested, IEnumerable<string> available) =>
        $"Sheet '{requested}' was not found. Available sheets: {string.Join(", ", available)}.";

    public static string NoDataRowsFound(int startRow) =>
        $"No data rows found from {nameof(TemplateConfiguration.StartRow)} {startRow}.";

    public static string TemplateNotFound(string code) =>
        $"Template '{code}' was not found.";

    public static string TemplateAlreadyExists(string code) =>
        $"Template '{code}' already exists.";

    public static string ImportExportEntityNotFound(string key) =>
        $"Import/export entity '{key}' was not found.";

    public static string UnsupportedFile(string fileName) =>
        $"Unsupported file format '{fileName}'.";

    public static string FileTooLarge(long actualSize, long maxSize) =>
        $"File size '{actualSize:N0}' bytes exceeds configured limit '{maxSize:N0}' bytes.";

    public static string RowLimitExceeded(int actualRows, int maxRows) =>
        $"Row count '{actualRows:N0}' exceeds configured limit '{maxRows:N0}'.";

    public static string ConnectionStringRequired(string name) =>
        $"Connection string '{name}' is required for template storage.";

    public static string ConnectionStringNotFound(string name) =>
        $"Connection string '{name}' not found.";

    public static string ExportTemplatePlaceholdersRequired() =>
        "Export template must use MiniExcel placeholders, e.g. {{rows.FieldName}} on the data row (under the header). " +
        "This keeps template styles and enables streaming export.";

    public static string ExportTemplatePlaceholderMissing(string fieldName) =>
        $"Export template is missing placeholder '{{{{rows.{fieldName}}}}}'.";

    public static string OperationTimeout(string operation, int timeoutSeconds) =>
        $"{operation} exceeded configured timeout of {timeoutSeconds} second(s). " +
        "Increase ExcelEngine:ImportTimeoutSeconds / ExportTimeoutSeconds, or use async jobs (GĐ2) for very large files.";
}
