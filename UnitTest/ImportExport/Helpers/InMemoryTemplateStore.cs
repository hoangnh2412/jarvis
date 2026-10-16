namespace UnitTest.ImportExport.Helpers;

/// <summary>In-memory <see cref="ITemplateStore"/> for <see cref="Infrastructure.Services.TemplateService"/> unit tests.</summary>
public sealed class InMemoryTemplateStore : ITemplateStore
{
    private readonly Dictionary<string, TemplateDefinition> _items =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<TemplateDefinition?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(code))
            return Task.FromResult<TemplateDefinition?>(null);

        _items.TryGetValue(code.Trim(), out var def);
        return Task.FromResult(def);
    }

    public Task SaveAsync(TemplateDefinition template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        cancellationToken.ThrowIfCancellationRequested();

        var code = template.Code.Trim();
        var now = DateTimeOffset.UtcNow;
        var existing = _items.TryGetValue(code, out var prev) ? prev : null;

        _items[code] = new TemplateDefinition
        {
            Code = code,
            Name = template.Name.Trim(),
            FileName = template.FileName.Trim(),
            Location = template.Location,
            Content = template.Content,
            Mappings = template.Mappings,
            Sheets = template.Sheets,
            HeaderRow = template.HeaderRow,
            StartRow = template.StartRow,
            RowCount = template.RowCount,
            SheetName = template.SheetName,
            CreatedAt = existing?.CreatedAt ?? now,
            UpdatedAt = now
        };

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var trimmed = code.Trim();
        if (!_items.Remove(trimmed))
            throw new TemplateNotFoundException(trimmed);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TemplateDefinition>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TemplateDefinition> list = _items.Values
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult(list);
    }
}
