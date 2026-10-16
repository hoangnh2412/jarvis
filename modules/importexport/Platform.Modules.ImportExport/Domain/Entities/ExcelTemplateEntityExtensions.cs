namespace Platform.Modules.ImportExport.Domain.Entities;

public static class ExcelTemplateEntityExtensions
{
    public static void SoftDelete(this IExcelTemplateEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.IsActive = false;
    }
}