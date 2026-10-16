

namespace Platform.Modules.ImportExport.Application.Export.Models;

public sealed class ExportResult
    {
        public required byte[] Content { get; init; }
        public required string FileName { get; init; }
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }