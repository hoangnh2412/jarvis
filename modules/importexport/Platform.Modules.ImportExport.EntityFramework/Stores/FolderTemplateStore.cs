using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Services;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.EntityFramework.Stores;

/// <summary>
/// File-system template store under <c>{root}/{tenantId}/{code}.json</c>.
/// Tenant from <see cref="ICurrentTenant{TTenant}.GetIdAsync"/> (ambient scope or host resolvers), same as Setting module.
/// </summary>
public sealed class FolderTemplateStore<TTenant>(
    IOptions<ExcelEngineOptions> options,
    TemplateFileResolver fileResolver,
    ICurrentTenant<TTenant> currentTenant,
    ILogger<FolderTemplateStore<TTenant>> logger) : ITemplateStore
    where TTenant : class, ICurrentTenantIdentity
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _root = ResolveRoot(options.Value.TemplateFilesRoot);

    public async Task<TemplateDefinition?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        var path = MetaPath(tenantId, code);
        if (!File.Exists(path))
            return null;

        await using var stream = File.OpenRead(path);
        var def = await JsonSerializer.DeserializeAsync<TemplateDefinition>(stream, JsonOptions, cancellationToken);
        if (def is null)
            return null;

        var content = await fileResolver.ReadAsync(def.Location, cancellationToken);
        return new TemplateDefinition
        {
            TenantId = def.TenantId == Guid.Empty ? tenantId : def.TenantId,
            Code = def.Code,
            Name = def.Name,
            FileName = def.FileName,
            Location = def.Location,
            Content = content.Length == 0 ? def.Content : content,
            Mappings = def.Mappings,
            Sheets = def.Sheets,
            HeaderRow = def.HeaderRow,
            StartRow = def.StartRow,
            RowCount = def.RowCount,
            SheetName = def.SheetName
        };
    }

    public async Task SaveAsync(TemplateDefinition template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        Directory.CreateDirectory(TenantRoot(tenantId));

        var location = template.Location;
        if (template.Content is { Length: > 0 })
        {
            location = await fileResolver.WriteAsync(
                tenantId,
                template.Code,
                template.FileName,
                template.Content,
                cancellationToken);
        }

        var toSave = new TemplateDefinition
        {
            TenantId = tenantId,
            Code = template.Code,
            Name = template.Name,
            FileName = template.FileName,
            Location = location,
            Content = null,
            Mappings = template.Mappings,
            Sheets = template.Sheets,
            HeaderRow = template.HeaderRow,
            StartRow = template.StartRow,
            RowCount = template.RowCount,
            SheetName = template.SheetName
        };

        var path = MetaPath(tenantId, template.Code);
        var isCreate = !File.Exists(path);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, toSave, JsonOptions, cancellationToken);

        if (isCreate)
        {
            logger.LogInformation(
                EngineLogMessages.TemplateCreated,
                template.Code,
                template.Mappings.Count,
                template.FileName);
        }
        else
        {
            logger.LogInformation(EngineLogMessages.TemplateUpdated, template.Code);
        }
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        var path = MetaPath(tenantId, code);
        if (File.Exists(path))
        {
            File.Delete(path);
            logger.LogInformation(EngineLogMessages.TemplateDeleted, code.Trim());
        }
    }

    public async Task<IReadOnlyList<TemplateDefinition>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        var tenantDir = TenantRoot(tenantId);
        if (!Directory.Exists(tenantDir))
            return [];

        var list = new List<TemplateDefinition>();
        foreach (var file in Directory.EnumerateFiles(tenantDir, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = File.OpenRead(file);
            var def = await JsonSerializer.DeserializeAsync<TemplateDefinition>(
                stream,
                JsonOptions,
                cancellationToken);
            if (def is not null)
                list.Add(def);
        }

        return list;
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new InvalidTemplateException(EngineMessages.TenantRequired);
        return tenantId.Value;
    }

    private string TenantRoot(Guid tenantId) =>
        Path.Combine(_root, tenantId.ToString("D"));

    private string MetaPath(Guid tenantId, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidTemplateException(EngineMessages.TemplateCodeRequired);

        var safe = string.Join("_", code.Trim().Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(TenantRoot(tenantId), safe + ".json");
    }

    private static string ResolveRoot(string root) =>
        Path.IsPathRooted(root) ? root : Path.GetFullPath(root);
}
