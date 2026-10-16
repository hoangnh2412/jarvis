using Platform.Modules.ImportExport.Application.Templates.Models;

namespace Platform.Modules.ImportExport.Application.Templates;

public interface ITemplateService
{
    Task<TemplateDetail> CreateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);

    Task<TemplateDetail> UpdateMetadataAsync(
        string code,
        UpdateTemplateMetadataRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateDetail> UpdateConfigurationAsync(
        string code,
        UpdateTemplateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task<TemplateDetail> UpdateAsync(string code, UpdateTemplateRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TemplateSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<TemplateSummary> GetForExecutionAsync(string code, CancellationToken cancellationToken = default);

    Task<TemplateDetail> GetAsync(string code, CancellationToken cancellationToken = default);

    Task DeleteAsync(string code, CancellationToken cancellationToken = default);
}