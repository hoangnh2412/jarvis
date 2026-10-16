using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>
/// Validate sau khi template đã resolve (<see cref="ImportWork"/>).
/// Override qua DI nếu project cần rule thêm.
/// </summary>
public interface IImportRequestValidator
{
    /// <summary>Kiểm tra file, size, sheet plans, mappings trên <paramref name="work"/>.</summary>
    void Validate(ImportWork work);
}
