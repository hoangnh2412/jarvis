---
name: telemetry-dotnet-trace-plugin
description: Đăng ký custom ITraceInstrumentation singleton để mở rộng TracerProviderBuilder. Dùng khi cần instrumentation trace không có sẵn trong Jarvis core.
dependencies:
  - Jarvis.OpenTelemetry
---

# Custom ITraceInstrumentation

```csharp
public sealed class MyTraceInstrumentation : ITraceInstrumentation
{
    public TracerProviderBuilder AddInstrumentation(IServiceProvider sp, TracerProviderBuilder builder)
    {
        return builder.AddSource("MyApp.Custom");
    }
}

// Đăng ký trong AddJarvisOpenTelemetry callback — Singleton:
services.AddSingleton<ITraceInstrumentation, MyTraceInstrumentation>();
```

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddSingleton<ITraceInstrumentation, MyTraceInstrumentation>();
    })
    .ConfigureResource()
    .ConfigureTrace()
    // …
    ;
```

**ITraceExporter** (exporter bổ sung):

```csharp
services.AddSingleton<ITraceExporter, MyTraceExporter>();
```
