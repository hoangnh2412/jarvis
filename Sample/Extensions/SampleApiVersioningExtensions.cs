using Asp.Versioning;

namespace Sample.Extensions;

public static class SampleApiVersioningExtensions
{
    public static WebApplicationBuilder AddSampleApiVersioning(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(int.Parse(Sample.ApiVersions.MajorVersion), int.Parse(Sample.ApiVersions.MinorVersion));
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return builder;
    }
}
