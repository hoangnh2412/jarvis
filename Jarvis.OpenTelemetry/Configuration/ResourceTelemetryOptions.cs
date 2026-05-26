namespace Jarvis.OpenTelemetry.Configuration;

public class ResourceTelemetryOptions
{
    /// <summary>Adds <c>host.*</c> resource attributes when available.</summary>
    public bool IncludeHostDetector { get; set; } = true;

    /// <summary>Adds container resource attributes when running in Docker/Kubernetes (CGROUP lines, etc.).</summary>
    public bool IncludeContainerDetector { get; set; }
}
