namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>
/// Business error codes for the ImportExport module.
/// </summary>
public static class ImportExportErrorCode
{
    public const string InvalidTemplate = nameof(InvalidTemplate);

    public const string UnsupportedFile = nameof(UnsupportedFile);

    public const string FileTooLarge = nameof(FileTooLarge);

    public const string RowLimitExceeded = nameof(RowLimitExceeded);

    public const string ExcelParsingFailed = nameof(ExcelParsingFailed);

    public const string ExportFailed = nameof(ExportFailed);

    public const string OperationTimeout = nameof(OperationTimeout);

    public const string TemplateNotFound = nameof(TemplateNotFound);

    public const string TemplateAlreadyExists = nameof(TemplateAlreadyExists);

    public const string ImportExportEntityNotFound = nameof(ImportExportEntityNotFound);
}