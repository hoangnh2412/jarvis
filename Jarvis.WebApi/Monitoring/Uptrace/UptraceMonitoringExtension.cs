namespace Jarvis.WebApi.Monitoring.Uptrace;

public static class UptraceMonitoringExtension
{
    public static Uri ParseUptraceDsn(string dsn)
    {
        Uri otlpGrpcEndpoint = null;

        var uri = new Uri(dsn);
        if (uri.Host == "uptrace.dev" || uri.Host == "api.uptrace.dev")
        {
            otlpGrpcEndpoint = new Uri("https://otlp.uptrace.dev:4317");
        }
        else
        {
            var grpcPort = 14317;

            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Int32.TryParse(query["grpc"], out grpcPort);

            otlpGrpcEndpoint =
                new UriBuilder
                {
                    Scheme = uri.Scheme,
                    Host = uri.DnsSafeHost,
                    Port = grpcPort,
                }.Uri;
        }

        return otlpGrpcEndpoint;
    }
}