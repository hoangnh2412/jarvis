namespace Platform.Modules.ImportExport.Domain.Enums;

/// <summary>
/// Import pipeline phase — validate-only skips <see cref="Application.Import.IImportDataPersister"/>.
/// </summary>
public enum ImportExecutionPhase
{
    ValidateOnly = 0,
    Import = 1
}
