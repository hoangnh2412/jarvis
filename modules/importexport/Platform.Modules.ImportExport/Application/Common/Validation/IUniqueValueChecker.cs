namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>
/// Optional external uniqueness (e.g. email already in consumer DB).
/// Default no-op returns false. Register a consumer implementation to enable.
/// </summary>
public interface IUniqueValueChecker
{
    Task<bool> ExistsAsync(string fieldName, object? value, CancellationToken cancellationToken = default);
}