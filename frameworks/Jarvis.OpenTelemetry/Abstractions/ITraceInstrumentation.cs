using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Abstractions;

public interface ITraceInstrumentation
{
    /// <param name="builder">Tracer provider under construction.</param>
    /// <param name="serviceProvider">Root service provider when the tracer is built; use for <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>, options, connection factories, etc.</param>
    TracerProviderBuilder AddInstrumentation(IServiceProvider serviceProvider, TracerProviderBuilder builder);
}
