using Sample.Models;

namespace Sample.ImportExport.Constants;

public static class ImportExportApiMessages
{
    public const string FileRequired = $"{nameof(StudentImportRequest.File)} is required.";

    public static string ImportFailedWithRowErrors(int failedRowCount) =>
        $"Import failed: {failedRowCount} row(s) have validation errors.";
}
