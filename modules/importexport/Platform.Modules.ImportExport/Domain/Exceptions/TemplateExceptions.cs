using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class TemplateNotFoundException : ExcelEngineException
{
    public TemplateNotFoundException(string code)
        : base(ImportExportErrorCode.TemplateNotFound, EngineMessages.TemplateNotFound(code))
    {
    }
}

public sealed class TemplateAlreadyExistsException : ExcelEngineException
{
    public TemplateAlreadyExistsException(string code)
        : base(ImportExportErrorCode.TemplateAlreadyExists, EngineMessages.TemplateAlreadyExists(code))
    {
    }
}
