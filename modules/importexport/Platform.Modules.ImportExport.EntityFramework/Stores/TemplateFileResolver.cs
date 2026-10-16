using Microsoft.Extensions.Options;

namespace Platform.Modules.ImportExport.EntityFramework.Stores;

/// <summary>Resolves template file bytes from Location (absolute or under TemplateFilesRoot).</summary>
public sealed class TemplateFileResolver(IOptions<ExcelEngineOptions> options)
{
    private readonly ExcelEngineOptions _options = options.Value;

    public string ResolvePath(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return string.Empty;

        if (Path.IsPathRooted(location))
            return location;

        var root = _options.TemplateFilesRoot;
        if (!Path.IsPathRooted(root))
            root = Path.GetFullPath(root);

        return Path.GetFullPath(Path.Combine(root, location));
    }

    public async Task<byte[]> ReadAsync(string? location, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(location);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return [];

        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    public async Task<string> WriteAsync(
        Guid tenantId,
        string code,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidTemplateException(EngineMessages.TenantRequired);

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var safeCode = string.Join("_", code.Split(Path.GetInvalidFileNameChars()));
        var safeName = Path.GetFileName(fileName);
        var relative = Path.Combine(tenantId.ToString("D"), safeCode, safeName);

        var fullPath = ResolvePath(relative);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return relative.Replace('\\', '/');
    }
}
