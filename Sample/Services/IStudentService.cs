using Platform.DDD.Domain.Repositories;
using Platform.Modules.ImportExport.API.Models;
using Sample.Models;

namespace Sample.Services;

public interface IStudentService
{
    Task<StudentListResponse> GetListAsync(PagedListRequest request, CancellationToken cancellationToken);

    Task<StudentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken);

    Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<ImportApiResponse> ValidateImportAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);

    Task<ImportApiResponse> ImportAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);

    Task<FileDownloadResponse> ExportAsync(string? fileName, CancellationToken cancellationToken);
}
