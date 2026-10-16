using System.Reflection;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.Enums;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.ImportExport.Application.Schemas;
using Platform.Modules.ImportExport.Application.Templates;
using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Application.Export;
using Platform.Modules.ImportExport.Application.Common.Execution;
using Platform.Modules.ImportExport.Application.Common.Validation;
using Platform.Modules.ImportExport.Domain.Models;
using Platform.Modules.ImportExport.Application.Templates.Models;
using Platform.Modules.ImportExport.Application.Import.Models;
using Platform.Modules.ImportExport.Application.Export.Models;
using Platform.Modules.ImportExport.Application.Schemas.Models;
using Platform.Modules.ImportExport.Domain.Enums;
using Sample.Entities;
using Platform.Modules.ImportExport.API.Models;
using Sample.ImportExport.Constants;
using Sample.Models;
using Platform.Multitenancy;
using Sample.Persistence;
using Sample.Settings;

namespace Sample.Services;

public sealed class StudentService(
    ITenantUnitOfWork unitOfWork,
    ICurrentTenant<CurrentTenantInfo> currentTenant,
    ImportExportSettingReader importExportSettingReader,
    IImportService importService,
    IExportService exportService) : IStudentService
{
    private static readonly HashSet<string> AllowedListFields = typeof(Student)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => p.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<StudentListResponse> GetListAsync(
        PagedListRequest request,
        CancellationToken cancellationToken)
    {
        var options = new PagedQueryOptions<Student>
        {
            AllowedFields = AllowedListFields
        };

        var repo = await unitOfWork.GetRepositoryAsync<IQueryRepository<Student>>(cancellationToken);
        var (items, totalCount) = await repo.PaginationAsync(request, options, cancellationToken);

        return new StudentListResponse
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        };
    }

    public async Task<StudentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await GetStudentOrThrowAsync(id, cancellationToken);
        return MapToDto(student);
    }

    public async Task<StudentDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await GetTenantIdOrThrowAsync(cancellationToken);
        var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>(cancellationToken);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Age = request.Age,
            TenantId = tenantId
        };

        await repo.InsertAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(student);
    }

    public async Task<StudentDto> UpdateAsync(
        Guid id,
        UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>(cancellationToken);
        var student = await GetStudentOrThrowAsync(id, cancellationToken);

        student.Name = request.Name.Trim();
        student.Age = request.Age;

        await repo.UpdateAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(student);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>(cancellationToken);
        var student = await GetStudentOrThrowAsync(id, cancellationToken);

        await repo.DeleteAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ImportApiResponse> ImportAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken) =>
        await ImportCoreAsync(fileStream, fileName, ImportExecutionPhase.Import, cancellationToken);

    public async Task<ImportApiResponse> ValidateImportAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken) =>
        await ImportCoreAsync(fileStream, fileName, ImportExecutionPhase.ValidateOnly, cancellationToken);

    private async Task<ImportApiResponse> ImportCoreAsync(
        Stream fileStream,
        string fileName,
        ImportExecutionPhase phase,
        CancellationToken cancellationToken)
    {
        var templateCode = await RequireTemplateCodeAsync(
            ImportExportSettingDefinition.StudentImportTemplateCodeKey,
            cancellationToken);

        var result = await importService.ImportAsync(new ImportRequest
        {
            TemplateCode = templateCode,
            FileStream = fileStream,
            FileName = fileName,
            Phase = phase,
            EntityKey = ImportExportEntityKeys.Student
        }, cancellationToken);

        return new ImportApiResponse(
            result.Sheets,
            result.ProcessedRowCount,
            result.FailedRowCount,
            result.SavedCount);
    }

    public async Task<FileDownloadResponse> ExportAsync(
        string? fileName,
        CancellationToken cancellationToken)
    {
        var templateCode = await RequireTemplateCodeAsync(
            ImportExportSettingDefinition.StudentExportTemplateCodeKey,
            cancellationToken);

        var repo = await unitOfWork.GetRepositoryAsync<IQueryRepository<Student>>(cancellationToken);
        var students = (await repo.ListAsync(cancellationToken: cancellationToken))
            .OrderBy(s => s.Name)
            .ToList();

        var result = await exportService.ExportAsync(new ExportRequest<Student>
        {
            TemplateCode = templateCode,
            Data = students,
            FileName = fileName
        }, cancellationToken);

        return new FileDownloadResponse
        {
            FileName = result.FileName,
            ContentType = ExportResult.ContentType,
            ContentBase64 = Convert.ToBase64String(result.Content)
        };
    }

    private async Task<string> RequireTemplateCodeAsync(string key, CancellationToken cancellationToken)
    {
        var templateCode = await importExportSettingReader
            .GetTemplateCodeAsync(key, cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(templateCode))
        {
            throw new BadRequestException(
                BaseErrorCode.Error,
                $"Setting '{key}' is missing or empty. Configure groups {ImportExportSettingDefinition.ImportGroupName} / {ImportExportSettingDefinition.ExportGroupName}.");
        }

        return templateCode;
    }

    private async Task<Student> GetStudentOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>(cancellationToken);
        var student = await repo.GetByIdAsync(x => x.Id == id, cancellationToken);
        return student ?? throw new NotFoundException(BaseErrorCode.Error, $"Student '{id}' was not found.");
    }

    private async Task<Guid> GetTenantIdOrThrowAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new BadRequestException(BaseErrorCode.Error, "TenantId is required. Send header X-Tenant-Id.");

        return tenantId.Value;
    }

    private static StudentDto MapToDto(Student student) =>
        new(student.Id, student.Name, student.Age);
}
