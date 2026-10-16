using System.ComponentModel.DataAnnotations;

namespace Platform.Modules.Files;

/// <summary>Business error codes for the Files module (982xx).</summary>
public static class FileErrorCode
{
    [Display(Description = "Path contains invalid '.' or '..' segments")]
    public const string InvalidPath = "98201";

    [Display(Description = "Folder tree browse is not supported for the active blob provider")]
    public const string TreeNotSupported = "98202";

    [Display(Description = "A required field is missing or empty")]
    public const string RequiredField = "98203";

    [Display(Description = "Invalid move destination (into self or subfolder)")]
    public const string InvalidMove = "98204";

    [Display(Description = "Duplicate paths in batch request")]
    public const string DuplicatePath = "98205";

    [Display(Description = "Uploaded file must not be empty")]
    public const string EmptyFile = "98206";

    [Display(Description = "Request body is required")]
    public const string RequestBodyRequired = "98207";

    [Display(Description = "Query or body field cannot be whitespace when provided")]
    public const string WhitespaceField = "98208";

    /// <summary>Default user-facing message for <paramref name="code"/>.</summary>
    public static string Message(string code) => code switch
    {
        InvalidPath => "Path must not contain '.' or '..' segments.",
        TreeNotSupported => "Folder tree browse is not supported for the active blob provider. Register a matching IFileTreeProvider (e.g. UseSharePointTree, UseSynologyTree).",
        RequiredField => "A required field is missing or empty.",
        InvalidMove => "Cannot move an item into itself or one of its subfolders.",
        DuplicatePath => "Batch request must not contain duplicate paths.",
        EmptyFile => "Uploaded file must not be empty.",
        RequestBodyRequired => "Request body is required.",
        WhitespaceField => "Provided field cannot be empty or whitespace.",
        _ => "The file request is invalid."
    };
}
