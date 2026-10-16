using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Domain.Models;
using Platform.Multitenancy;
using Sample.Entities;
using Sample.ImportExport.Constants;
using Sample.Persistence;

namespace Sample.ImportExport.Services;

/// <summary>
/// Demo persister: maps import rows with fields <c>Name</c> / <c>Age</c> into <see cref="Student"/>.
/// </summary>
public sealed class StudentImportModulePersister(
    ITenantUnitOfWork unitOfWork,
    ICurrentTenant<CurrentTenantInfo> currentTenant) : IImportModulePersister
{
    public string EntityKey => ImportExportEntityKeys.Student;

    public async Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);

        var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>(cancellationToken).ConfigureAwait(false);
        var entities = new List<Student>(rows.Count);
        var errors = new List<ImportRowError>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var name = GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidOperationException("Name is required.");

                entities.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    Name = name.Trim(),
                    Age = GetInt32(row, "Age"),
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(
                    RowNumber: i + 1,
                    Message: ex.Message,
                    ErrorCode: ImportErrorCodes.PersistFailed,
                    SheetName: template.SheetName));
            }
        }

        foreach (var entity in entities)
            await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);

        if (entities.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ImportPersistResult
        {
            SavedCount = entities.Count,
            Errors = errors
        };
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId is required to persist students.");

        return tenantId.Value;
    }

    private static string? GetString(IReadOnlyDictionary<string, object?> row, string key)
    {
        if (!TryGet(row, key, out var value) || value is null)
            return null;
        return Convert.ToString(value);
    }

    private static int GetInt32(IReadOnlyDictionary<string, object?> row, string key)
    {
        if (!TryGet(row, key, out var value) || value is null || value is "")
            return 0;
        return Convert.ToInt32(value);
    }

    private static bool TryGet(IReadOnlyDictionary<string, object?> row, string key, out object? value)
    {
        if (row.TryGetValue(key, out value))
            return true;

        foreach (var pair in row)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}
