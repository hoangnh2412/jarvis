using Serilog;

namespace Sample.Extensions;

public static class SampleSerilogExtensions
{
    public static WebApplicationBuilder AddSampleSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, _, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext(),
            writeToProviders: true);

        return builder;
    }
}
