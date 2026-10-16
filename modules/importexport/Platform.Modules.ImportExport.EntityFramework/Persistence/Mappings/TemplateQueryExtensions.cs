using Platform.Modules.ImportExport.EntityFramework.Persistence.Models;

namespace Platform.Modules.ImportExport.EntityFramework.Persistence.Mappings;

internal static class TemplateQueryExtensions
{
    public static IQueryable<TemplateSummaryRow> ProjectSummary<TEntity>(
        this IQueryable<TEntity> query)
        where TEntity : class, IExcelTemplateEntity =>
        query.Select(x => new TemplateSummaryRow
        {
            TenantId = x.TenantId,
            Code = x.Code,
            Name = x.Name,
            FileName = x.FileName,
            Location = x.Location,
            MappingsJson = x.MappingsJson,
            HeaderRow = x.HeaderRow,
            StartRow = x.StartRow,
            RowCount = x.RowCount,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
}
