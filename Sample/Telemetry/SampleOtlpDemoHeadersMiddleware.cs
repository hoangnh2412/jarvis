namespace Sample.Telemetry;

/// <summary>
/// Adds a response header captured by OpenTelemetry when <c>CaptureResponseHeaders</c> is enabled.
/// Send <c>x-demo-request</c> on requests to see request-side tags in traces.
/// </summary>
public sealed class SampleOtlpDemoHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("x-demo-response", "jarvis-sample");
        return next(context);
    }
}
