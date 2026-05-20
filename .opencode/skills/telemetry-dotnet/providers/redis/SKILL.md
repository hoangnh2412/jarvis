---
name: telemetry-dotnet-redis
description: Bật Redis trace instrumentation qua Jarvis plug-in hoặc OpenTelemetry.Instrumentation.StackExchangeRedis. Dùng khi cần span Redis trên trace pipeline; IConnectionMultiplexer phải đăng ký trước.
dependencies:
  - Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis
  - OpenTelemetry.Instrumentation.StackExchangeRedis
  - StackExchange.Redis
---

# Redis trace

**Cách 1 — Jarvis package (plug-in `ITraceInstrumentation`):**

```csharp
// ProjectReference: Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis
// IConnectionMultiplexer phải có trong DI trước khi build host

builder.Services.AddKeyedSingleton<IConnectionMultiplexer>("Default", (_, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return ConnectionMultiplexer.Connect(config["Cache:Redis:Configuration"]!);
});

builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddJarvisRedisTraceInstrumentation();
    })
    .ConfigureResource()
    .ConfigureTrace()
    // …
    ;
```

**Cách 2 — OTEL package trong `ConfigureTrace`:**

```csharp
.ConfigureTrace(options =>
{
    options.AddRedisInstrumentation(sp =>
    {
        var mux = sp.GetRequiredKeyedService<IConnectionMultiplexer>("Default");
        return mux.GetDatabase();
    });
})
```

**Jarvis caching — invalidation connection riêng:**

Memory invalidation dùng `MemoryCacheInvalidationDefaults.ConnectionServiceKey` (không trùng `DistributedGroups`).

```csharp
builder.AddJarvisCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation(); // đọc Cache:MemoryInvalidation:Redis:Configuration

.ConfigureTrace(options =>
{
    options.AddRedisInstrumentation("Default");
    options.AddJarvisCachingMemoryInvalidationRedisInstrumentation();
})
```

Mỗi `AddRedisInstrumentation(keyedName)` chỉ instrument đúng multiplexer đó — thêm connection mới không ảnh hưởng connection cũ.
