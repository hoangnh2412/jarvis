using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Jarvis.HealthChecks;

/// <summary>
/// Liveness-only check: memory pressure via <c>GC.GetGCMemoryInfo()</c> and CPU estimate from process times; no network.
/// Chỉ dùng cho liveness: áp lực bộ nhớ qua <c>GC.GetGCMemoryInfo()</c> và ước lượng CPU từ thời gian tiến trình; không mạng.
/// </summary>
internal sealed class ProcessResourceLivenessHealthCheck(
    IOptions<JarvisHealthCheckOptions> options,
    IMemoryCache cache)
    : IHealthCheck
{
    private const string CacheKey = "jarvis:health:liveness:process-resources";
    private readonly JarvisHealthCheckOptions _options = options.Value;

    // EN: Serialize CPU sampling across concurrent probe calls / VI: Đồng bộ lấy mẫu CPU khi nhiều probe đồng thời
    private static readonly Lock CpuLock = new();
    private static long lastWallMs;
    private static TimeSpan lastCpuTime;

    // EN: First sample establishes baseline without failing / VI: Lần đo đầu chỉ lấy baseline, không fail
    private static bool cpuWarm;

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var ttl = Math.Clamp(_options.ResultCacheMilliseconds, 0, 5000);
        if (ttl > 0 && cache.TryGetValue(CacheKey, out HealthCheckResult cached))
            return Task.FromResult(cached);

        var sw = Stopwatch.StartNew();
        var memoryStatus = EvaluateMemory(out var memoryPct);
        if (memoryStatus != HealthStatus.Healthy)
        {
            var fail = HealthCheckResult.Unhealthy(
                $"memory_load={memoryPct:F1}% (threshold {_options.MemoryThresholdPercent}%); elapsed_ms={sw.Elapsed.TotalMilliseconds:F1} / " +
                $"tải_bộ_nhớ={memoryPct:F1}% (ngưỡng {_options.MemoryThresholdPercent}%); elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
            SetCache(ttl, fail);
            return Task.FromResult(fail);
        }

        var cpuStatus = EvaluateCpu(out var cpuPct);
        if (cpuStatus != HealthStatus.Healthy)
        {
            var fail = HealthCheckResult.Unhealthy(
                $"cpu={cpuPct:F1}% (threshold {_options.CpuThresholdPercent}%); memory_load={memoryPct:F1}%; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1} / " +
                $"cpu={cpuPct:F1}% (ngưỡng {_options.CpuThresholdPercent}%); tải_bộ_nhớ={memoryPct:F1}%; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
            SetCache(ttl, fail);
            return Task.FromResult(fail);
        }

        var ok = HealthCheckResult.Healthy(
            $"memory_load={memoryPct:F1}%; cpu={cpuPct:F1}%; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1} / " +
            $"tải_bộ_nhớ={memoryPct:F1}%; cpu={cpuPct:F1}%; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
        SetCache(ttl, ok);
        return Task.FromResult(ok);
    }

    /// <summary>
    /// EN: Stores last computed result for <paramref name="ttl"/> milliseconds / VI: Lưu kết quả đã tính trong <paramref name="ttl"/> ms.
    /// </summary>
    private void SetCache(int ttl, HealthCheckResult result)
    {
        if (ttl <= 0)
            return;
        cache.Set(CacheKey, result, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(ttl) });
    }

    /// <summary>
    /// EN: Compares GC memory load ratio to configured threshold / VI: So sánh tải bộ nhớ GC với ngưỡng cấu hình.
    /// </summary>
    private HealthStatus EvaluateMemory(out double loadPercent)
    {
        loadPercent = 0;
        var info = GC.GetGCMemoryInfo();
        if (info.TotalAvailableMemoryBytes <= 0)
            return HealthStatus.Healthy;

        loadPercent = 100.0 * info.MemoryLoadBytes / (double)info.TotalAvailableMemoryBytes;
        return loadPercent >= _options.MemoryThresholdPercent ? HealthStatus.Unhealthy : HealthStatus.Healthy;
    }

    /// <summary>
    /// EN: Approximates CPU% from wall-clock delta and <see cref="Process.TotalProcessorTime"/> / VI: Ước lượng %CPU từ delta thời gian và TotalProcessorTime.
    /// </summary>
    private HealthStatus EvaluateCpu(out double cpuPercent)
    {
        cpuPercent = 0;
        var proc = Process.GetCurrentProcess();
        var wallMs = Environment.TickCount64;
        var cpu = proc.TotalProcessorTime;

        lock (CpuLock)
        {
            if (!cpuWarm)
            {
                lastWallMs = wallMs;
                lastCpuTime = cpu;
                cpuWarm = true;
                return HealthStatus.Healthy;
            }

            var wallDeltaMs = wallMs - lastWallMs;
            var cpuDeltaMs = (cpu - lastCpuTime).TotalMilliseconds;
            lastWallMs = wallMs;
            lastCpuTime = cpu;

            if (wallDeltaMs <= 0)
                return HealthStatus.Healthy;

            var processors = Math.Max(1, Environment.ProcessorCount);
            cpuPercent = 100.0 * cpuDeltaMs / (wallDeltaMs * processors);
            return cpuPercent >= _options.CpuThresholdPercent ? HealthStatus.Unhealthy : HealthStatus.Healthy;
        }
    }
}
