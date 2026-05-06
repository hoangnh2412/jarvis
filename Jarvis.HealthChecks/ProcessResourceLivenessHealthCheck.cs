// Jarvis.HealthChecks — process-resources liveness: CLR memory ratio vs ceiling, CPU sample vs threshold; short IMemoryCache TTL.
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Jarvis.HealthChecks;

/// <summary>
/// Liveness-only check: <c>memory_load</c> = CLR allocated MB (see <c>GC.GetTotalMemory(false)</c>) vs <see cref="JarvisHealthCheckOptions.ProcessAllocatedMemoryMegabytesCeiling"/>;
/// <c>cpu_load</c> = measured process CPU vs <see cref="JarvisHealthCheckOptions.CpuThresholdPercent"/>; no network.
/// Chỉ liveness: <c>memory_load</c> = % RAM CLR ước lượng của app so trần MB; <c>cpu_load</c> = % CPU app so trần CPU; không mạng.
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
        var memoryStatus = EvaluateMemory(out var memoryLoadPct, out var allocatedMb);
        if (memoryStatus != HealthStatus.Healthy)
        {
            var fail = HealthCheckResult.Unhealthy(
                $"{FormatMemoryDescription(memoryLoadPct, allocatedMb)} (fail_if_load_percent>={_options.MemoryThresholdPercent}); elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
            SetCache(ttl, fail);
            return Task.FromResult(fail);
        }

        var cpuStatus = EvaluateCpu(out var cpuLoadPct, out var measuredCpuPct);
        if (cpuStatus != HealthStatus.Healthy)
        {
            var fail = HealthCheckResult.Unhealthy(
                $"{FormatCpuDescription(cpuLoadPct, measuredCpuPct)} (fail_if_measured_cpu_percent>={_options.CpuThresholdPercent}); {FormatMemoryDescription(memoryLoadPct, allocatedMb)}; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
            SetCache(ttl, fail);
            return Task.FromResult(fail);
        }

        var ok = HealthCheckResult.Healthy(
            $"{FormatMemoryDescription(memoryLoadPct, allocatedMb)}; {FormatCpuDescription(cpuLoadPct, measuredCpuPct)}; elapsed_ms={sw.Elapsed.TotalMilliseconds:F1}");
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
    /// EN: <c>memory_load</c> = (CLR allocated MB / <see cref="JarvisHealthCheckOptions.ProcessAllocatedMemoryMegabytesCeiling"/>) * 100; skip if ceiling ≤ 0.
    /// VI: <c>memory_load</c> = (MB CLR ước lượng / trần MB) * 100; bỏ qua nếu trần ≤ 0.
    /// </summary>
    private HealthStatus EvaluateMemory(out double memoryLoadPercent, out double allocatedMegabytes)
    {
        const double bytesPerMb = 1024d * 1024d;
        allocatedMegabytes = GC.GetTotalMemory(false) / bytesPerMb;
        memoryLoadPercent = 0;
        var ceilingMb = _options.ProcessAllocatedMemoryMegabytesCeiling;
        if (ceilingMb <= 0)
            return HealthStatus.Healthy;

        memoryLoadPercent = 100.0 * allocatedMegabytes / ceilingMb;
        return memoryLoadPercent >= _options.MemoryThresholdPercent ? HealthStatus.Unhealthy : HealthStatus.Healthy;
    }

    /// <summary>
    /// EN: Measured process CPU% (machine-normalized); <c>cpu_load</c> = (measured / ceiling) * 100. Skip if ceiling ≤ 0.
    /// VI: CPU% tiến trình đo được; <c>cpu_load</c> = (đo được / trần) * 100. Bỏ qua nếu trần ≤ 0.
    /// </summary>
    private HealthStatus EvaluateCpu(out double cpuLoadPercent, out double measuredCpuPercent)
    {
        measuredCpuPercent = 0;
        cpuLoadPercent = 0;
        var cpuCeiling = _options.CpuThresholdPercent;
        if (cpuCeiling <= 0)
            return HealthStatus.Healthy;

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
            measuredCpuPercent = 100.0 * cpuDeltaMs / (wallDeltaMs * processors);
            cpuLoadPercent = 100.0 * measuredCpuPercent / cpuCeiling;
            return measuredCpuPercent >= cpuCeiling ? HealthStatus.Unhealthy : HealthStatus.Healthy;
        }
    }

    private string FormatMemoryDescription(double memoryLoadPercent, double allocatedMb)
    {
        var ceiling = _options.ProcessAllocatedMemoryMegabytesCeiling;
        return ceiling <= 0
            ? $"memory_load=n/a (memory_ceiling_mb=0; allocated_mb={allocatedMb:F1})"
            : $"memory_load={memoryLoadPercent:F1}% (allocated_mb={allocatedMb:F1} / memory_ceiling_mb={ceiling})";
    }

    private string FormatCpuDescription(double cpuLoadPercent, double measuredCpuPercent)
    {
        var ceiling = _options.CpuThresholdPercent;
        return ceiling <= 0
            ? $"cpu_load=n/a (cpu_ceiling_percent=0; measured_cpu_percent={measuredCpuPercent:F1})"
            : $"cpu_load={cpuLoadPercent:F1}% (measured_cpu_percent={measuredCpuPercent:F1} / cpu_ceiling_percent={ceiling})";
    }
}
