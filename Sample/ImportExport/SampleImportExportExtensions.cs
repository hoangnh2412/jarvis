using Platform.ImportExport.MiniExcel.Extensions;
using Platform.Modules.ImportExport.API.Extensions;
using Platform.Modules.ImportExport.DependencyInjection;
using Platform.Modules.ImportExport.EntityFramework.Extensions;
using Platform.Multitenancy;
using Sample.Entities;
using Sample.ImportExport.Constants;
using Sample.ImportExport.Models;
using Sample.ImportExport.Services;
using Sample.Persistence;

namespace Sample.ImportExport;

public static class SampleImportExportExtensions
{
    public static WebApplicationBuilder AddSampleImportExport(this WebApplicationBuilder builder)
    {
        // Keyed module persisters — composite IImportDataPersister routes by ImportRequest.EntityKey.
        builder.Services
            .AddImportModulePersister<StudentImportModulePersister>(nameof(StudentImportModulePersister))
            .AddImportModulePersister<UserImportModulePersister>(nameof(UserImportModulePersister));

        // Entity registry — schema discovery for UI tạo template (GET /importexport/entities/{key}/fields).
        // Không validate runtime import; mapping trong template + IImportModulePersister mới quyết định persist.
        //
        // Thêm đối tượng mới:
        // 1. Tạo DTO *ImportRow (Sample/ImportExport/Models/) — public properties + DataAnnotations.
        // 2. Thêm const vào ImportExportEntityKeys + chain .Add<TImportRow>(key, displayName) bên dưới.
        // 3. AddImportModulePersister<TPersister>(ImportExportEntityKeys.X) + module service truyền EntityKey.
        // 4. (Tuỳ chọn) Khai báo key trong ImportExportEntitiesSettingDefinition cho catalog Settings.
        builder.AddCoreImport()
            .ConfigureEntities(options =>
            {
                options.Add<StudentImportRow>(ImportExportEntityKeys.Student, "Student");
                // options.Add<TeacherImportRow>(ImportExportEntityKeys.Teacher, "Giáo viên");
            })
            .UseMiniExcel()
            .UseEntityFramework<SampleExcelTemplate, ITenantUnitOfWork, CurrentTenantInfo>()
            .UseHttpApi();

        builder.AddCoreExport();

        return builder;
    }
}
