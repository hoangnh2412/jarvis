---
name: telemetry-dotnet-logging-plugin
description: Đăng ký custom ILoggingExporter singleton để cấu hình thêm LoggerProviderBuilder. Dùng khi cần exporter log bổ sung ngoài OTLP/console mặc định Jarvis.
dependencies:
  - Jarvis.OpenTelemetry
---

# Custom ILoggingExporter

```csharp
public sealed class MyLoggingExporter : ILoggingExporter
{
    public void Configure(OpenTelemetryLoggerOptions options, IServiceProvider sp)
    {
        // options.AddProcessor(...);
    }
}

services.AddSingleton<ILoggingExporter, MyLoggingExporter>();
```

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddSingleton<ILoggingExporter, MyLoggingExporter>();
    })
    .ConfigureResource()
    .ConfigureLogging()
    // …
    ;
```

Jarvis không `ClearProviders` — Serilog/NLog đăng ký riêng ngoài package này.
