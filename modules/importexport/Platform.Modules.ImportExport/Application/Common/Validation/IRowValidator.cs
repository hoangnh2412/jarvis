using Platform.Modules.ImportExport.Domain.Models;
using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>Context shared across rows during one import (in-file unique tracking).</summary>
public sealed class RowValidationContext
{
    public required int ExcelRowNumber { get; init; }
    public string? SheetName { get; init; }
    public required IReadOnlyList<ColumnMapping> Mappings { get; init; }

    /// <summary>FieldName → normalized values already accepted in this import.</summary>
    public required Dictionary<string, HashSet<string>> SeenUniqueValues { get; init; }
}

/// <summary>Per-row cell rules + uniqueness. Overridable via DI.</summary>
public interface IRowValidator
{
    Task<IReadOnlyList<ImportRowError>> ValidateAsync(
        IReadOnlyDictionary<string, object?> row,
        RowValidationContext context,
        CancellationToken cancellationToken = default);
}
