using Platform.Modules.Files.API.Models;
using Platform.Modules.Files.Models;

namespace Platform.Modules.Files.API.Mapping;

internal static class FileHttpRequestMapping
{
    public static RenameFileInput ToInput(RenameFileRequest request) => new()
    {
        Path = request.Path,
        NewName = request.NewName
    };

    public static MoveFileInput ToInput(MoveFileRequest request) => new()
    {
        SourcePath = request.SourcePath,
        DestinationFolder = request.DestinationFolder
    };

    public static MoveBatchFileInput ToInput(MoveBatchFileRequest request) => new()
    {
        DestinationFolder = request.DestinationFolder,
        SourcePaths = request.SourcePaths
    };
}
