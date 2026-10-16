using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.Domain.Exceptions;

public sealed class InvalidTemplateException : ExcelEngineException
    {
        public InvalidTemplateException(string message)
            : base(
                ImportExportErrorCode.InvalidTemplate,
                message)
        {
        }
    }
