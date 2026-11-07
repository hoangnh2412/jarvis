using Sample.Services;

namespace Sample.Extensions;

public static class SampleApplicationServicesExtensions
{
    public static WebApplicationBuilder AddSampleApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddSampleSettings();
        builder.AddSampleTenants();
        builder.AddSampleOrgs();
        builder.Services.AddScoped<ImportExportSettingReader>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.AddSampleFiles();

        return builder;
    }
}
