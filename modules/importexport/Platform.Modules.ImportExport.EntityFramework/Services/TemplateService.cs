using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Application.Common.Validation;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.EntityFramework.Services;

/// <summary>
/// API/application façade over <see cref="ITemplateStore"/>.
/// </summary>
public sealed class TemplateService(
    ITemplateStore store,
    ILogger<TemplateService> logger) : ITemplateService
{
    public async Task<TemplateDetail> CreateAsync(
        CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Content is not { Length: > 0 })
            throw new InvalidTemplateException(EngineMessages.TemplateContentRequired);

        using var scope = BeginScope("Create", request.Code);
        logger.LogInformation(EngineLogMessages.TemplateOperationStarted, "Create", request.Code);

        ValidateLayout(request.Mappings, request.Sheets);
        TemplateFileTypeHelper.ValidateSheetNameForTemplate(
            request.FileName,
            request.SheetName,
            request.Sheets);

        var definition = ToDefinition(request);
        await store.SaveAsync(definition, cancellationToken);

        var saved = await store.GetByCodeAsync(definition.Code, cancellationToken)
            ?? throw new TemplateNotFoundException(definition.Code);

        logger.LogInformation(EngineLogMessages.TemplateOperationFinished, "Create", saved.Code);
        return ToDetail(saved);
    }

    public async Task<TemplateDetail> UpdateMetadataAsync(
        string code,
        UpdateTemplateMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidTemplateException(EngineMessages.TemplateNameRequired);

        using var scope = BeginScope("UpdateMetadata", code);
        logger.LogInformation(EngineLogMessages.TemplateOperationStarted, "UpdateMetadata", code);

        var existing = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());

        var updated = new TemplateDefinition
        {
            TenantId = existing.TenantId,
            Code = existing.Code,
            Name = request.Name.Trim(),
            FileName = existing.FileName,
            Location = existing.Location,
            Content = existing.Content,
            Mappings = existing.Mappings,
            Sheets = existing.Sheets,
            HeaderRow = existing.HeaderRow,
            StartRow = existing.StartRow,
            RowCount = existing.RowCount,
            SheetName = existing.SheetName
        };

        await store.SaveAsync(updated, cancellationToken);
        var saved = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());
        logger.LogInformation(EngineLogMessages.TemplateOperationFinished, "UpdateMetadata", saved.Code);
        return ToDetail(saved);
    }

    public async Task<TemplateDetail> UpdateConfigurationAsync(
        string code,
        UpdateTemplateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var scope = BeginScope("UpdateConfiguration", code);
        logger.LogInformation(EngineLogMessages.TemplateOperationStarted, "UpdateConfiguration", code);

        var existing = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());

        var mappings = request.Mappings ?? existing.Mappings;
        var sheets = request.Sheets ?? existing.Sheets;
        if (request.Mappings is not null || request.Sheets is not null)
            ValidateLayout(mappings, sheets);

        var fileName = request.FileName?.Trim() ?? existing.FileName;
        var sheetName = request.SheetName ?? existing.SheetName;

        TemplateFileTypeHelper.ValidateSheetNameForTemplate(fileName, sheetName, sheets);

        var updated = new TemplateDefinition
        {
            TenantId = existing.TenantId,
            Code = existing.Code,
            Name = existing.Name,
            FileName = fileName,
            Location = existing.Location,
            Content = request.Content ?? existing.Content,
            Mappings = mappings,
            Sheets = sheets,
            HeaderRow = request.HeaderRow ?? existing.HeaderRow,
            StartRow = request.StartRow ?? existing.StartRow,
            RowCount = request.RowCount ?? existing.RowCount,
            SheetName = sheetName
        };

        await store.SaveAsync(updated, cancellationToken);
        var saved = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());
        logger.LogInformation(EngineLogMessages.TemplateOperationFinished, "UpdateConfiguration", saved.Code);
        return ToDetail(saved);
    }

    public async Task<TemplateDetail> UpdateAsync(
        string code,
        UpdateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await UpdateConfigurationAsync(code, new UpdateTemplateConfigurationRequest
        {
            FileName = request.FileName,
            Content = request.Content,
            Mappings = request.Mappings,
            Sheets = request.Sheets,
            HeaderRow = request.HeaderRow,
            StartRow = request.StartRow,
            RowCount = request.RowCount,
            SheetName = request.SheetName
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<TemplateSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = await store.ListAsync(cancellationToken);
        return list.Select(ToSummary).ToList();
    }

    public async Task<TemplateSummary> GetForExecutionAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var def = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());
        return ToSummary(def);
    }

    public async Task<TemplateDetail> GetAsync(string code, CancellationToken cancellationToken = default)
    {
        var def = await store.GetByCodeAsync(code, cancellationToken)
            ?? throw new TemplateNotFoundException(code.Trim());
        return ToDetail(def);
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        using var scope = BeginScope("Delete", code);
        logger.LogInformation(EngineLogMessages.TemplateOperationStarted, "Delete", code);
        await store.DeleteAsync(code, cancellationToken);
        logger.LogInformation(EngineLogMessages.TemplateOperationFinished, "Delete", code);
    }

    private IDisposable? BeginScope(string operation, string? code) =>
        logger.BeginScope(new Dictionary<string, object?>
        {
            [LogPropertyNames.Operation] = operation,
            [LogPropertyNames.TemplateCode] = code?.Trim(),
            [LogPropertyNames.FileName] = null,
            [LogPropertyNames.Phase] = "Template"
        });

    private static void ValidateLayout(
        IReadOnlyList<ColumnMapping> mappings,
        IReadOnlyList<SheetTemplate> sheets)
    {
        if (sheets.Count > 0)
        {
            foreach (var sheet in sheets)
            {
                if (sheets.Count > 1 && string.IsNullOrWhiteSpace(sheet.SheetName))
                    throw new InvalidTemplateException(EngineMessages.SheetNameRequiredForMultiSheet);
                MappingValidator.Validate(sheet.Mappings, requireAtLeastOne: true);
            }
            return;
        }

        MappingValidator.Validate(mappings, requireAtLeastOne: true);
    }

    private static TemplateDefinition ToDefinition(CreateTemplateRequest request) => new()
    {
        Code = request.Code.Trim(),
        Name = request.Name.Trim(),
        FileName = request.FileName.Trim(),
        Content = request.Content,
        Mappings = request.Mappings,
        Sheets = request.Sheets,
        HeaderRow = request.HeaderRow,
        StartRow = request.StartRow,
        RowCount = request.RowCount,
        SheetName = string.IsNullOrWhiteSpace(request.SheetName) ? null : request.SheetName.Trim()
    };

    private static TemplateSummary ToSummary(TemplateDefinition d) => new()
    {
        TenantId = d.TenantId,
        Code = d.Code,
        Name = d.Name,
        FileName = d.FileName,
        Location = d.Location,
        HeaderRow = d.HeaderRow,
        StartRow = d.StartRow,
        RowCount = d.RowCount,
        SheetName = d.SheetName,
        Mappings = d.Mappings,
        Sheets = d.Sheets,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static TemplateDetail ToDetail(TemplateDefinition d) => new()
    {
        TenantId = d.TenantId,
        Code = d.Code,
        Name = d.Name,
        FileName = d.FileName,
        Location = d.Location,
        HeaderRow = d.HeaderRow,
        StartRow = d.StartRow,
        RowCount = d.RowCount,
        SheetName = d.SheetName,
        Mappings = d.Mappings,
        Sheets = d.Sheets,
        Content = d.Content ?? [],
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}
