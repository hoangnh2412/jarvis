using Platform.BlobStoring.Helpers;
using Platform.Modules.Files.Models;

namespace Platform.Modules.Files.Validation;

/// <summary>Path and request validation for file operations (no HTTP/MVC dependency).</summary>
public static class FilePathValidator
{
    public static FileValidationResult ValidateCreateBucket(string bucket, string? path)
    {
        if (path is not null && string.IsNullOrWhiteSpace(path))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                "Query parameter 'path' cannot be empty or whitespace when provided.");
        }

        if (!string.IsNullOrEmpty(bucket) && BlobPathHelper.ContainsTraversalSegment(bucket))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        if (path is not null && BlobPathHelper.ContainsTraversalSegment(path))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        return FileValidationResult.Success();
    }

    public static FileValidationResult ValidateUpload(string bucket, string? fileName, long? fileLength)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return FileValidationResult.Fail(FileErrorCode.RequiredField, "Query parameter 'fileName' is required.");

        if (fileLength is null or 0)
            return FileValidationResult.Fail(FileErrorCode.EmptyFile);

        if (!string.IsNullOrEmpty(bucket) && BlobPathHelper.ContainsTraversalSegment(bucket))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        return ValidatePathSegment(fileName, "Query parameter 'fileName'");
    }

    public static FileValidationResult ValidateDownload(string bucket, string? fileName) =>
        ValidateFileQuery(bucket, fileName);

    public static FileValidationResult ValidateView(string bucket, string? fileName) =>
        ValidateFileQuery(bucket, fileName);

    public static FileValidationResult ValidateDelete(string bucket, string? fileName, string? prefix)
    {
        if (!string.IsNullOrEmpty(bucket) && BlobPathHelper.ContainsTraversalSegment(bucket))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        var fileNameResult = ValidateOptionalWhitespace(fileName, "fileName");
        if (!fileNameResult.IsValid)
            return fileNameResult;

        if (!string.IsNullOrWhiteSpace(fileName))
            return ValidatePathSegment(fileName, "Query parameter 'fileName'");

        var prefixResult = ValidateOptionalPrefix(prefix);
        if (!prefixResult.IsValid)
            return prefixResult;

        return FileValidationResult.Success();
    }

    public static FileValidationResult ValidateDeleteBatch(string bucket, string[]? fileNames)
    {
        if (!string.IsNullOrEmpty(bucket) && BlobPathHelper.ContainsTraversalSegment(bucket))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        if (fileNames is null || fileNames.Length == 0)
        {
            return FileValidationResult.Fail(
                FileErrorCode.RequiredField,
                "Request body must contain at least one file path.");
        }

        if (fileNames.Any(string.IsNullOrWhiteSpace))
        {
            return FileValidationResult.Fail(
                FileErrorCode.RequiredField,
                "Request body must not contain empty or whitespace file paths.");
        }

        foreach (var fileName in fileNames)
        {
            var pathResult = ValidatePathSegment(fileName, "Request body file path");
            if (!pathResult.IsValid)
                return pathResult;
        }

        return FileValidationResult.Success();
    }

    public static FileValidationResult ValidateRename(RenameFileInput? input)
    {
        if (input is null)
            return FileValidationResult.Fail(FileErrorCode.RequestBodyRequired);

        if (input.Path is not null && input.Path.Length > 0 && string.IsNullOrWhiteSpace(input.Path))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                "Body field 'path' cannot be whitespace when provided.");
        }

        if (string.IsNullOrWhiteSpace(input.NewName))
            return FileValidationResult.Fail(FileErrorCode.RequiredField, "Body field 'newName' is required.");

        if (string.IsNullOrEmpty(input.Path))
        {
            if (input.NewName.Contains('/') || input.NewName.Contains('\\'))
            {
                return FileValidationResult.Fail(
                    FileErrorCode.InvalidPath,
                    "Body field 'newName' must not contain path separators when renaming a bucket.");
            }

            if (BlobPathHelper.ContainsTraversalSegment(input.NewName))
                return FileValidationResult.Fail(FileErrorCode.InvalidPath);

            return FileValidationResult.Success();
        }

        if (BlobPathHelper.ContainsTraversalSegment(input.Path)
            || BlobPathHelper.ContainsTraversalSegment(input.NewName))
        {
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);
        }

        return FileValidationResult.Success();
    }

    public static FileValidationResult ValidateMove(MoveFileInput? input) =>
        input is null
            ? FileValidationResult.Fail(FileErrorCode.RequestBodyRequired)
            : ValidateMovePaths(input.SourcePath, input.DestinationFolder);

    public static FileValidationResult ValidateMoveBatch(MoveBatchFileInput? input)
    {
        if (input is null)
            return FileValidationResult.Fail(FileErrorCode.RequestBodyRequired);

        if (input.SourcePaths is null || input.SourcePaths.Length == 0)
        {
            return FileValidationResult.Fail(
                FileErrorCode.RequiredField,
                "Body field 'sourcePaths' must contain at least one path.");
        }

        if (input.SourcePaths.Any(string.IsNullOrWhiteSpace))
        {
            return FileValidationResult.Fail(
                FileErrorCode.RequiredField,
                "Body field 'sourcePaths' must not contain empty or whitespace entries.");
        }

        var distinctCount = input.SourcePaths
            .Select(ObjectStoragePathHelper.NormalizeKey)
            .Distinct(StringComparer.Ordinal)
            .Count();

        if (distinctCount != input.SourcePaths.Length)
            return FileValidationResult.Fail(FileErrorCode.DuplicatePath);

        if (input.DestinationFolder is not null
            && input.DestinationFolder.Length > 0
            && string.IsNullOrWhiteSpace(input.DestinationFolder))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                "Body field 'input.DestinationFolder' cannot be whitespace when provided.");
        }

        var folder = string.IsNullOrWhiteSpace(input.DestinationFolder)
            ? string.Empty
            : input.DestinationFolder.Trim();

        if (BlobPathHelper.ContainsTraversalSegment(input.DestinationFolder))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        foreach (var sourcePath in input.SourcePaths)
        {
            var error = ValidateMovePaths(sourcePath, input.DestinationFolder);
            if (!error.IsValid)
                return error;
        }

        return FileValidationResult.Success();
    }

    private static FileValidationResult ValidateFileQuery(string bucket, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return FileValidationResult.Fail(FileErrorCode.RequiredField, "Query parameter 'fileName' is required.");

        if (!string.IsNullOrEmpty(bucket) && BlobPathHelper.ContainsTraversalSegment(bucket))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);

        return ValidatePathSegment(fileName, "Query parameter 'fileName'");
    }

    private static FileValidationResult ValidatePathSegment(string path, string fieldLabel)
    {
        if (BlobPathHelper.ContainsTraversalSegment(path))
        {
            return FileValidationResult.Fail(
                FileErrorCode.InvalidPath,
                $"{fieldLabel} must not contain '.' or '..' segments.");
        }

        return FileValidationResult.Success();
    }

    private static FileValidationResult ValidateOptionalWhitespace(string? value, string fieldName)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                $"Query parameter '{fieldName}' cannot be empty or whitespace when provided.");
        }

        return FileValidationResult.Success();
    }

    private static FileValidationResult ValidateOptionalPrefix(string? prefix)
    {
        if (prefix is not null && string.IsNullOrWhiteSpace(prefix))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                "Query parameter 'prefix' cannot be empty or whitespace when provided.");
        }

        if (BlobPathHelper.ContainsTraversalSegment(prefix))
            return FileValidationResult.Fail(FileErrorCode.InvalidPath, "Query parameter 'prefix' must not contain '.' or '..' segments.");

        return FileValidationResult.Success();
    }

    private static FileValidationResult ValidateMovePaths(string sourcePath, string? destinationFolder)
    {
        

        if (string.IsNullOrWhiteSpace(sourcePath))
            return FileValidationResult.Fail(FileErrorCode.RequiredField, "Body field 'sourcePath' is required.");

        if (destinationFolder is not null
            && destinationFolder.Length > 0
            && string.IsNullOrWhiteSpace(destinationFolder))
        {
            return FileValidationResult.Fail(
                FileErrorCode.WhitespaceField,
                "Body field 'destinationFolder' cannot be whitespace when provided.");
        }

var folder = string.IsNullOrWhiteSpace(destinationFolder)
    ? string.Empty
    : destinationFolder.Trim();

        if (BlobPathHelper.ContainsTraversalSegment(sourcePath)
            || BlobPathHelper.ContainsTraversalSegment(folder))
        {
            return FileValidationResult.Fail(FileErrorCode.InvalidPath);
        }

        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, folder))
            return FileValidationResult.Fail(FileErrorCode.InvalidMove);

        if (ObjectStoragePathHelper.NormalizeKey(sourcePath)
            .Equals(ObjectStoragePathHelper.NormalizeKey(folder), StringComparison.Ordinal))
        {
            return FileValidationResult.Fail(FileErrorCode.InvalidMove, "Cannot move an item into itself.");
        }

        return FileValidationResult.Success();
    }
}
