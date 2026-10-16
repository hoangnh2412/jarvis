namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>Structured logging property names (keep placeholders in sync via nameof).</summary>
public static class LogPropertyNames
{
    public const string FileName = nameof(FileName);
    public const string SheetName = nameof(SheetName);
    public const string Sheet = nameof(Sheet);
    public const string StartRow = nameof(StartRow);
    public const string HeaderRow = nameof(HeaderRow);
    public const string MappingCount = nameof(MappingCount);
    public const string RowCount = nameof(RowCount);
    public const string Bytes = nameof(Bytes);
    public const string ProcessedRows = nameof(ProcessedRows);
    public const string ValidRows = nameof(ValidRows);
    public const string ErrorCount = nameof(ErrorCount);
    public const string ErrorCode = nameof(ErrorCode);
    public const string Code = nameof(Code);
    public const string Row = nameof(Row);
    public const string Column = nameof(Column);
    public const string Field = nameof(Field);
    public const string Message = nameof(Message);
    public const string Operation = nameof(Operation);
    public const string TemplateCode = nameof(TemplateCode);
    public const string Phase = nameof(Phase);
    public const string DurationMs = nameof(DurationMs);
    public const string SavedCount = nameof(SavedCount);
    public const string EntityKey = nameof(EntityKey);
    public const string BatchSize = nameof(BatchSize);
    public const string TimeoutSeconds = nameof(TimeoutSeconds);
    public const string RowsOmitted = nameof(RowsOmitted);
}

/// <summary>Shared log message templates for DLL + API host.</summary>
public static class EngineLogMessages
{
    public static readonly string ImportStarted =
        $"Import started. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.StartRow}={{{LogPropertyNames.StartRow}}}, " +
        $"{LogPropertyNames.HeaderRow}={{{LogPropertyNames.HeaderRow}}}, " +
        $"{LogPropertyNames.SheetName}={{{LogPropertyNames.SheetName}}}, " +
        $"{LogPropertyNames.MappingCount}={{{LogPropertyNames.MappingCount}}}";

    public static readonly string ImportRowValidationFailed =
        $"Import row validation failed. {LogPropertyNames.Sheet}={{{LogPropertyNames.Sheet}}}, " +
        $"{LogPropertyNames.Row}={{{LogPropertyNames.Row}}}, " +
        $"{LogPropertyNames.Column}={{{LogPropertyNames.Column}}}, " +
        $"{LogPropertyNames.Field}={{{LogPropertyNames.Field}}}, " +
        $"{LogPropertyNames.Code}={{{LogPropertyNames.Code}}}, " +
        $"{LogPropertyNames.Message}={{{LogPropertyNames.Message}}}";

    public static readonly string ImportCanceled =
        $"Import canceled. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string ImportFailed =
        $"Import failed. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.ErrorCode}={{{LogPropertyNames.ErrorCode}}}";

    public static readonly string ImportFailedUnexpectedly =
        $"Import failed unexpectedly. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string ImportFinished =
        $"Import finished. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.Sheet}={{{LogPropertyNames.Sheet}}}, " +
        $"{LogPropertyNames.ProcessedRows}={{{LogPropertyNames.ProcessedRows}}}, " +
        $"{LogPropertyNames.ValidRows}={{{LogPropertyNames.ValidRows}}}, " +
        $"{LogPropertyNames.ErrorCount}={{{LogPropertyNames.ErrorCount}}}";

    public static readonly string ImportDuration =
        $"Import duration. {LogPropertyNames.DurationMs}={{{LogPropertyNames.DurationMs}}}, " +
        $"{LogPropertyNames.SavedCount}={{{LogPropertyNames.SavedCount}}}, " +
        $"{LogPropertyNames.RowsOmitted}={{{LogPropertyNames.RowsOmitted}}}";

    public static readonly string ImportSheetStarted =
        $"Import sheet started. {LogPropertyNames.Sheet}={{{LogPropertyNames.Sheet}}}, " +
        $"{LogPropertyNames.StartRow}={{{LogPropertyNames.StartRow}}}, " +
        $"{LogPropertyNames.HeaderRow}={{{LogPropertyNames.HeaderRow}}}, " +
        $"{LogPropertyNames.MappingCount}={{{LogPropertyNames.MappingCount}}}";

    public static readonly string ImportSheetFinished =
        $"Import sheet finished. {LogPropertyNames.Sheet}={{{LogPropertyNames.Sheet}}}, " +
        $"{LogPropertyNames.ProcessedRows}={{{LogPropertyNames.ProcessedRows}}}, " +
        $"{LogPropertyNames.ValidRows}={{{LogPropertyNames.ValidRows}}}, " +
        $"{LogPropertyNames.ErrorCount}={{{LogPropertyNames.ErrorCount}}}, " +
        $"{LogPropertyNames.SavedCount}={{{LogPropertyNames.SavedCount}}}";

    public static readonly string ImportRequestStarted =
        $"Import request started. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.Phase}={{{LogPropertyNames.Phase}}}, " +
        $"{LogPropertyNames.EntityKey}={{{LogPropertyNames.EntityKey}}}";

    public static readonly string ImportRequestFinished =
        $"Import request finished. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.ProcessedRows}={{{LogPropertyNames.ProcessedRows}}}, " +
        $"{LogPropertyNames.ValidRows}={{{LogPropertyNames.ValidRows}}}, " +
        $"{LogPropertyNames.ErrorCount}={{{LogPropertyNames.ErrorCount}}}, " +
        $"{LogPropertyNames.SavedCount}={{{LogPropertyNames.SavedCount}}}, " +
        $"{LogPropertyNames.DurationMs}={{{LogPropertyNames.DurationMs}}}";

    public static readonly string ExportStarted =
        $"Export started. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.SheetName}={{{LogPropertyNames.SheetName}}}, " +
        $"{LogPropertyNames.MappingCount}={{{LogPropertyNames.MappingCount}}}";

    public static readonly string ExportFinished =
        $"Export finished. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.SheetName}={{{LogPropertyNames.SheetName}}}, " +
        $"{LogPropertyNames.RowCount}={{{LogPropertyNames.RowCount}}}, " +
        $"{LogPropertyNames.Bytes}={{{LogPropertyNames.Bytes}}}";

    public static readonly string ExportDuration =
        $"Export duration. {LogPropertyNames.DurationMs}={{{LogPropertyNames.DurationMs}}}";

    public static readonly string ExportCanceled =
        $"Export canceled. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string ExportFailed =
        $"Export failed. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.ErrorCode}={{{LogPropertyNames.ErrorCode}}}";

    public static readonly string ExportFailedUnexpectedly =
        $"Export failed unexpectedly. {LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string ExportRequestStarted =
        $"Export request started. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string ExportRequestFinished =
        $"Export request finished. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.Bytes}={{{LogPropertyNames.Bytes}}}, " +
        $"{LogPropertyNames.DurationMs}={{{LogPropertyNames.DurationMs}}}";

    public static readonly string ExportTemplateWithoutPlaceholders =
        $"Export template has no {{{{rows.*}}}} placeholders; using plain SaveAs (template styles not applied). " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string OperationTimeout =
        $"Operation timed out. {LogPropertyNames.Operation}={{{LogPropertyNames.Operation}}}, " +
        $"{LogPropertyNames.TimeoutSeconds}={{{LogPropertyNames.TimeoutSeconds}}}";

    public static readonly string PersistBatchCompleted =
        $"Persist batch completed. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.EntityKey}={{{LogPropertyNames.EntityKey}}}, " +
        $"{LogPropertyNames.BatchSize}={{{LogPropertyNames.BatchSize}}}, " +
        $"{LogPropertyNames.SavedCount}={{{LogPropertyNames.SavedCount}}}, " +
        $"{LogPropertyNames.ErrorCount}={{{LogPropertyNames.ErrorCount}}}";

    public static readonly string PersistRoutingFailed =
        $"Persist routing failed. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.EntityKey}={{{LogPropertyNames.EntityKey}}}, " +
        $"{LogPropertyNames.Message}={{{LogPropertyNames.Message}}}";

    public static readonly string PersistNoOp =
        $"Default import persister is no-op. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.BatchSize}={{{LogPropertyNames.BatchSize}}}";

    public static readonly string ApiImportReceived =
        $"API import received. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.Phase}={{{LogPropertyNames.Phase}}}, " +
        $"{LogPropertyNames.EntityKey}={{{LogPropertyNames.EntityKey}}}";

    public static readonly string ApiExportReceived =
        $"API export received. {LogPropertyNames.TemplateCode}={{{LogPropertyNames.TemplateCode}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}, " +
        $"{LogPropertyNames.RowCount}={{{LogPropertyNames.RowCount}}}";

    public static readonly string ApiTemplateMutation =
        $"API template mutation. {LogPropertyNames.Operation}={{{LogPropertyNames.Operation}}}, " +
        $"{LogPropertyNames.Code}={{{LogPropertyNames.Code}}}";

    public static readonly string TemplateOperationStarted =
        $"Template operation started. {LogPropertyNames.Operation}={{{LogPropertyNames.Operation}}}, " +
        $"{LogPropertyNames.Code}={{{LogPropertyNames.Code}}}";

    public static readonly string TemplateOperationFinished =
        $"Template operation finished. {LogPropertyNames.Operation}={{{LogPropertyNames.Operation}}}, " +
        $"{LogPropertyNames.Code}={{{LogPropertyNames.Code}}}";

    public static readonly string SchemaListRequested =
        "ImportExport entity schema list requested.";

    public static readonly string SchemaFieldsRequested =
        $"ImportExport entity fields requested. {LogPropertyNames.EntityKey}={{{LogPropertyNames.EntityKey}}}";

    public static readonly string TemplateCreated =
        $"Template created. {LogPropertyNames.Code}={{{LogPropertyNames.Code}}}, " +
        $"{LogPropertyNames.MappingCount}={{{LogPropertyNames.MappingCount}}}, " +
        $"{LogPropertyNames.FileName}={{{LogPropertyNames.FileName}}}";

    public static readonly string TemplateUpdated =
        $"Template updated. {LogPropertyNames.Code}={{{LogPropertyNames.Code}}}";

    public static readonly string TemplateDeleted =
        $"Template soft-deleted. {LogPropertyNames.Code}={{{LogPropertyNames.Code}}}";

    public static readonly string EngineErrorDetail =
        $"{{{LogPropertyNames.ErrorCode}}}: {{{LogPropertyNames.Message}}}";
}
