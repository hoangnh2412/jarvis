---
name: dotnet-runtime-metrics-dashboard
description: >-
  Interprets Grafana "Dotnet Runtime Metrics" panels (Prometheus): GC, memory,
  thread pool, JIT, exceptions, assemblies. Covers dotnet_* (.NET 9+) vs
  process_runtime_dotnet_* (OpenTelemetry.Instrumentation.Runtime). Use when
  explaining .NET runtime dashboards, PromQL panels, or correlating runtime
  signals with latency/CPU issues.
---

# SKILL: .NET Runtime Metrics Dashboard (Grafana / Prometheus)

## 1. Purpose

Giúp agent/developer **đọc đúng ý nghĩa** các panel dashboard runtime .NET trên Grafana (datasource Prometheus), **không nhầm** với metric HTTP/RPS hay metric nghiệp vụ. Skill bám dashboard kiểu **“Dotnet Runtime Metrics”**: hai họ tên song song **`dotnet_*`** (built-in .NET 9+) và **`process_runtime_dotnet_*`** (package `OpenTelemetry.Instrumentation.Runtime`, thường cho .NET 6/7/8).

## 2. When to apply

- User hỏi ý nghĩa panel GC, JIT, thread pool, exception trên Grafana.
- User dán JSON dashboard hoặc PromQL có `dotnet_` / `process_runtime_dotnet_`.
- User muốn biết **runtime vs HTTP**: dashboard này là **tầng CLR**, không thay dashboard API.

## 3. Variables và filter

- **`job`**, **`instance`**: lọc đúng tiến trình / target scrape. Panel trống → kiểm tra label và time range, không chỉ “sai query”.
- Nhiều query dùng `{__name__=~"A|B"}` để **một panel** chạy được trên cả metric built-in và OTel Runtime.

## 4. Panel groups — ý nghĩa và cách đọc

### 4.1 Exceptions and Assemblies

| Panel | Ý nghĩa | Đọc nhanh |
|--------|---------|-----------|
| **Thrown Exceptions** | Số exception (handled + unhandled), thường theo **`error_type`**, **`job`**. | Spike → lỗi logic, dependency, đổi hành vi sau deploy. |
| **Assemblies by Job** | Số assembly đã load. | Ổn định sau warm-up; tăng dài hạn bất thường → load động/plugin. |

### 4.2 .NET GC and Memory

| Panel | Ý nghĩa | Đọc nhanh |
|--------|---------|-----------|
| **GC Pause Time** | Thời gian pause GC (tùy query: tốc độ / xu hướng). | Pause cao → ảnh hưởng latency; đi cùng allocation/generation. |
| **GC Collection Attempts** | Số lần GC theo **generation** (`gc_heap_generation`). | Gen0/1 cao có thể bình thường; **Gen2** tăng mạnh → áp lực heap / long-lived objects. |
| **GC Memory Committed** | Bộ nhớ committed cho heap/GC. | Xu hướng tăng dài hạn → leak hoặc cache lớn; so sánh trước/sau deploy. |
| **GC Heap Fragmentation** | Phân mảnh heap sau GC. | Cao → có thể tăng áp lực bộ nhớ / compact. |
| **Last Collection Heap Size** | Kích thước heap theo generation sau GC gần nhất. | Theo dõi tăng trưởng heap theo thời gian và traffic. |

### 4.3 CPU and ThreadPool

| Panel | Ý nghĩa | Đọc nhanh |
|--------|---------|-----------|
| **ThreadPool Workload** | Work item thread pool **đã hoàn thành** (throughput). | Tăng theo load; =0 bất thường → scrape/process. |
| **ThreadPool Threads and Timers** | Số thread pool + **timer**. | Timer/thread tăng bất thường → kiểm tra scheduling background. |
| **CPU Seconds by Job** | CPU process (thường **`dotnet_process_cpu_time_seconds_total`**, `cpu_mode`). | **Yêu cầu .NET 9+** cho họ `dotnet_*`; kernel vs user gợi ý I/O vs code. |
| **Available CPUs** | Logical CPU process nhìn thấy. | **.NET 9+**; context container/cgroup. |

### 4.4 .NET JIT

| Panel | Ý nghĩa | Đọc nhanh |
|--------|---------|-----------|
| **JIT Compilation Time** | Thời gian JIT đã tiêu tốn. | Spike sau deploy/cold start bình thường; kéo dài → nhiều code path mới. |
| **JIT Compilations** | Số method đã JIT. | Giảm sau warm-up; tăng lại liên tục → generic/dynamic load. |
| **JIT Compiled IL Bytes** | Khối lượng IL đã JIT. | Tương quan với số lần biên dịch. |

## 5. Dashboard vs other signals

- **Runtime dashboard**: GC, JIT, thread pool, exception **CLR** — giải thích *tại sao* CPU/latency đổi khi “code business” không đổi rõ.
- **HTTP / custom OTLP metrics** (`http.server.*`, `sample.*`, …): **khác pipeline và tên**; không kỳ vọng thấy chúng trên dashboard này trừ khi panel tự thêm.

## 6. Rules for the agent

- **Always** nêu rõ panel CPU (`dotnet_process_cpu_*`) **phụ thuộc .NET 9+** nếu user dùng runtime cũ hoặc chỉ có `process_runtime_dotnet_*`.
- **Always** phân biệt **spike sau deploy** (JIT, warm-up) với **xu hướng xấu dài hạn** (Gen2, committed memory).
- **Never** kết luận “lỗi business” chỉ từ assembly count hoặc JIT spike một lần.
- **When** user không thấy series: kiểm tra `job`/`instance`, time range, và **Prometheus có nhận metric** (remote write / scrape), không chỉ đổi tên metric.

## 7. Quick reference — metric name families

| Nguồn | Prefix ví dụ | Ghi chú |
|--------|----------------|---------|
| .NET 9+ built-in | `dotnet_` | CPU process, một số counter/histogram runtime |
| OTel Instrumentation.Runtime | `process_runtime_dotnet_` | Tương đương semantic gần với nhiều panel “song song” |

---

*Tham chiếu kiến trúc telemetry app: [`opentelemetry.md`](opentelemetry.md), [`opentelemetry-skill.md`](opentelemetry-skill.md).*
