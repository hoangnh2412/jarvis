using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.EntityFramework.Services;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.EntityFramework.Extensions;

/// <summary>
/// Đăng ký service danh mục org-structure (loại đơn vị, cấp/vị trí, chức vụ, pháp nhân, thành viên)
/// trên entity của host. Dữ liệu mặc định seed lấy từ <see cref="OrgStructureCatalogOptions"/> do host cấu hình.
/// </summary>
public static class OrgStructureCatalogExtensions
{
    public static IHostApplicationBuilder AddOrgStructureCatalogs<TUnitType, TJobGrade, TJobPosition, TMember, TUnit, TJobTitle, TLegalEntity>(
        this IHostApplicationBuilder builder,
        Action<OrgStructureCatalogOptions>? configure = null)
        where TUnitType : class, IOrgUnitTypeEntity
        where TJobGrade : class, IOrgNamedCatalogEntity
        where TJobPosition : class, IOrgNamedCatalogEntity
        where TMember : class, IOrgUnitMemberEntity
        where TUnit : class, IOrganizationalUnitEntity
        where TJobTitle : class, IJobTitleEntity
        where TLegalEntity : class, ILegalEntityProfileEntity
    {
        if (configure is not null)
            builder.Services.Configure(configure);

        builder.Services.AddScoped<IOrgUnitTypeService, OrgUnitTypeService<TUnitType, TUnit>>();
        builder.Services.AddScoped<IJobGradeService, JobGradeService<TJobGrade>>();
        builder.Services.AddScoped<IJobPositionService, JobPositionService<TJobPosition>>();
        builder.Services.AddScoped<IJobTitleCatalogService, JobTitleCatalogService<TJobTitle>>();
        builder.Services.AddScoped<ILegalEntityProfileService, LegalEntityProfileService<TLegalEntity>>();
        builder.Services.AddScoped<IOrgUnitMemberService, OrgUnitMemberService<TMember, TUnit, TJobPosition, TJobGrade, TJobTitle>>();
        return builder;
    }
}
