---
name: telemetry-dotnet-metric-plugin
description: Đăng ký custom IMetricInstrumentation singleton để mở rộng MeterProviderBuilder. Dùng khi cần metric instrumentation bổ sung ngoài runtime/aspnet mặc định Jarvis.
dependencies:
  - Jarvis.OpenTelemetry
---

# Custom IMetricInstrumentation

```csharp
public sealed class MyMetricInstrumentation : IMetricInstrumentation
{
    public MeterProviderBuilder AddInstrumentation(IServiceProvider sp, MeterProviderBuilder builder)
    {
        return builder.AddMeter("MyApp.Custom");
    }
}

services.AddSingleton<IMetricInstrumentation, MyMetricInstrumentation>();
```

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddSingleton<IMetricInstrumentation, MyMetricInstrumentation>();
    })
    .ConfigureResource()
    .ConfigureMetric()
    // …
    ;
```

**IMetricExporter** (exporter bổ sung):

```csharp
services.AddSingleton<IMetricExporter, MyMetricExporter>();
```
