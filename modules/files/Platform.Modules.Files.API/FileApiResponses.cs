using Microsoft.AspNetCore.Mvc;
using Platform.Modules.Files.Validation;

namespace Platform.Modules.Files.API;

internal static class FileApiResponses
{
    public static BadRequestObjectResult ValidationError(FileValidationResult result) =>
        new(new { code = result.Code, message = result.Message });
}
