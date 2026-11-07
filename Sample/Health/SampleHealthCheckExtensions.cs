using Platform.HealthChecks;

namespace Sample.Health;

public static class SampleHealthCheckExtensions
{
    public const string EnableConfigurationKey = "HealthChecks:Enable";

    public static WebApplicationBuilder AddSampleHealthChecks(this WebApplicationBuilder builder)
    {
        if (!IsEnabled(builder.Configuration))
            return builder;

        var timeoutSeconds = Math.Clamp(builder.Configuration.GetValue("HealthChecks:DefaultTimeoutSeconds", 5), 1, 120);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        builder.Services.AddHttpClient(SampleDogCeoApiHealthCheck.HttpClientName, client =>
        {
            client.Timeout = timeout;
        });
        builder.Services.AddHttpClient(SampleArticArtworksApiHealthCheck.HttpClientName, client =>
        {
            client.Timeout = timeout;
        });

        builder.AddHealthChecks();
        builder.AddSampleReadinessHealthChecks();

        return builder;
    }

    public static WebApplication UseSampleHealthChecks(this WebApplication app)
    {
        if (!IsEnabled(app.Configuration))
            return app;

        app.UseHealthChecks();
        return app;
    }

    private static bool IsEnabled(IConfiguration configuration)
        => configuration.GetValue(EnableConfigurationKey, true);
}
