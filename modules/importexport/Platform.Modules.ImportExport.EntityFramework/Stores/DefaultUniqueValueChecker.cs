using Platform.Modules.ImportExport.Application.Common.Validation;

namespace Platform.Modules.ImportExport.EntityFramework.Stores;

/// <summary>Default unique checker — always reports "not exists" (no DB lookup).</summary>
public sealed class DefaultUniqueValueChecker : IUniqueValueChecker
{
    public Task<bool> ExistsAsync(
        string fieldName,
        object? value,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
