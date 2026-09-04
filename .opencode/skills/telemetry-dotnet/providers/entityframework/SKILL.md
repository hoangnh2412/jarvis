---
name: telemetry-dotnet-entityframework
description: Bật EF Core trace qua OpenTelemetry.Instrumentation.EntityFrameworkCore trong ConfigureTrace. Dùng khi cần span truy vấn DbContext trên pipeline trace.
dependencies:
  - OpenTelemetry.Instrumentation.EntityFrameworkCore
---

# EF Core trace

```csharp
.ConfigureTrace(options =>
{
    options.AddEntityFrameworkCoreInstrumentation(efOptions =>
    {
        efOptions.SetDbStatementForText = true;
        efOptions.SetDbStatementForStoredProcedure = true;
    });
})
```

Không cần section `OTEL` riêng — dùng chung `OTEL:Tracing` OTLP/sampler/filter path.
