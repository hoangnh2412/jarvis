# SKILL: Jarvis.HealthChecks — thiết lập và mở rộng

## 1. Purpose

Hướng dẫn agent/developer **tích hợp** `Jarvis.HealthChecks` vào ASP.NET Core: DI (`AddHealthChecks`), HTTP (`UseHealthChecks`), cấu hình `HealthChecks`, startup probe, và **readiness do host đăng ký** (DB, Redis, HTTP, …). Có thể mở rộng bằng các gói **[AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)** (Xabaril) tương ứng .NET / phiên bản NuGet của dự án. Kiến trúc chi tiết: [`healthcheck.md`](healthcheck.md).

## 2. Scope

Skill này áp dụng cho:

- Ứng dụng **.NET / ASP.NET Core** dùng **minimal hosting** (`WebApplication`).
- Repo có (hoặc sẽ thêm) project reference tới **`Jarvis.HealthChecks`**.
- Cần endpoint **`/health/live`**, **`/health/ready`**, **`/health/startup`**, **`/health`** (và tùy chọn UI, Prometheus).
- Bổ sung check cho **dịch vụ ngoài** (MongoDB, RabbitMQ, Kafka, Azure, S3, Elasticsearch, …) qua package `AspNetCore.HealthChecks.*` từ repo Xabaril.

Không áp dụng cho:

- gRPC Health (cần stack riêng).
- Thay thế hoàn toàn **readiness** bằng cấu hình chỉ trong Core (Core **không** đăng ký SQL/Redis/HTTP readiness).
- Ứng dụng không dùng `IHostApplicationBuilder` / `WebApplication` theo chuẩn hiện đại.

## 3. Role

Bạn là **Senior Backend Engineer** (hoặc reviewer) chịu trách nhiệm:

- Đăng ký health đúng thứ tự, đúng tag, không duplicate check vô nghĩa.
- Phân biệt rõ **liveness** (Core + System) vs **readiness** (host).

## 4. Input

Dữ liệu đầu vào expected:

- **`Program.cs`** / pipeline hiện tại của host.
- **`appsettings.json`** (hoặc tương đương) — section `HealthChecks` và section readiness do team định nghĩa.
- **Requirement**: dependency nào là readiness (Postgres, Redis, URL ngoài, …).
- (Tùy chọn) Log probe fail, response JSON `/health`.

## 5. Output

Format đầu ra bắt buộc:

- **Code**: extension readiness (`IHostApplicationBuilder`), `IHealthCheck` tùy chỉnh nếu cần, chỉnh `Program.cs`.
- **Markdown**: cập nhật `appsettings` (keys, ví dụ).
- **Checklist** (cuối task): probe đã map; readiness có tag `readiness`; timeout hợp lý; startup không kẹt nếu dùng auto-complete.

## 6. Rules

Các nguyên tắc bắt buộc:

- **Always** gọi `builder.AddHealthChecks()` (Jarvis) **trước** `builder.Services.AddHealthChecks()` trong extension readiness của host (chuỗi thứ hai **append** cùng pipeline).
- **Always** gắn **`HealthCheckTags.Readiness`** cho check phụ thuộc hạ tầng / HTTP ngoài; liveness do Core quyết định, không gắn nhầm.
- **Always** gọi `app.UseHealthChecks()` sau khi build `WebApplication` và khi đã map controller / middleware cần thiết cho app (thứ tự pipeline theo chuẩn app).
- **Prefer** resolve connection string / Redis qua **đường dẫn key đầy đủ** trong config (`ConnectionStrings:SampleDbContext`, …) thay vì hard-code section.
- **Prefer** timeout readiness từ `HealthChecks:DefaultTimeoutSeconds` (clamp 1–120) thống nhất.
- **Never** bỏ `UseHealthChecks` nhưng vẫn kỳ vọng endpoint Jarvis hoạt động.
- **Never** đăng ký hai lần cùng tên check (`name` trong `AddCheck`) trừ khi có lý do rõ.
- **When** dùng extension Xabaril (`AddMongoDb`, `AddKafka`, …): thêm **`PackageReference`** đúng package vào project đang chứa lời gọi; căn chỉnh **version HealthChecks** (vd. 9.x với .NET 9) để tránh lỗi binding.

## 7. Process

Các bước xử lý:

1. **Analyze** — Đọc `Program.cs`, `JarvisHealthCheckOptions`, và [`healthcheck.md`](healthcheck.md) nếu cần phân tách tag/endpoint.
2. **Design** — Liệt kê readiness; chọn **extension** tương ứng (tìm package trên NuGet: [`AspNetCore.HealthChecks`](https://www.nuget.org/packages?q=AspNetCore.HealthChecks)) hoặc `IHealthCheck` tự viết; quyết định keys trong config.
3. **Implement**
   - Thêm `builder.AddHealthChecks()` + `builder.AddXxxReadinessHealthChecks()`.
   - `app.UseHealthChecks()`; công việc đồng bộ trước `Run()` (migrate, …) giữ **trước** `app.Run()`.
   - Mặc định **`MarkStartupCompleteOnApplicationStarted: true`**: không cần gọi `MarkStartupComplete()` trong `Program.cs` trừ khi tắt auto và có init bất đồng bộ sau start.
4. **Validate** — Gọi `/health/live`, `/health/ready`, `/health/startup`, `/health`; xác nhận readiness xuất hiện khi đã add; sửa `HealthChecks:System` trên Windows nếu disk/file fail.

## 8. Constraints

Giới hạn:

- **Performance**: liveness có cache ngắn (`ResultCacheMilliseconds`); System checks timeout 500 ms mỗi check — không dùng readiness query nặng trên `/health/live`.
- **Memory**: giới hạn `HealthChecks:System` / CLR ceiling phải phù hợp môi trường (container vs bare metal).
- **Compatibility**: HealthChecks UI cần **URI tuyệt đối** trong `Ui:Endpoints`; disk mặc định `/` có thể sai trên Windows — override trong config.

## 9. Examples

### Input

- Host cần Postgres + Redis readiness; config đã có `ConnectionStrings:SampleDbContext` và Redis under `Cache:...`.

### Output

**`appsettings.json` (rút gọn):**

```json
"HealthChecks": {
  "DefaultTimeoutSeconds": 5,
  "MarkStartupCompleteOnApplicationStarted": true,
  "Readiness": {
    "Database": "ConnectionStrings:SampleDbContext",
    "Redis": "Cache:DistGroups:Redis:Default:Configuration"
  }
}
```

**`Program.cs` (rút gọn):**

```csharp
using Jarvis.HealthChecks;

builder.AddHealthChecks();
builder.AddYourReadinessHealthChecks();

var app = builder.Build();
app.MapControllers();
app.UseHealthChecks();
app.EnsureMigrateDb<IYourUnitOfWork>();
app.Run();
```

**Readiness — resolve key path (pattern):**

```csharp
var keyPath = readiness.GetValue<string>("Database");
var cs = configuration[keyPath!.Trim()];
if (!string.IsNullOrWhiteSpace(cs))
    healthChecks.AddNpgSql(cs, name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
```

Tham chiếu mã đầy đủ: `Sample/Health/SampleReadinessHealthCheckExtensions.cs`, `SampleDogCeoApiHealthCheck.cs`, `SampleArticArtworksApiHealthCheck.cs`.

## 10. Anti-patterns

Những điều cần tránh:

- **`/health/ready` “Healthy” nhưng không có DB** — extension readiness chưa gọi, hoặc connection string rỗng nên **không** `AddNpgSql` / tương đương.
- **Startup probe fail vô hạn** — `MarkStartupCompleteOnApplicationStarted: false` mà quên gọi `MarkStartupComplete()`, hoặc exception trước `ApplicationStarted`.
- **System liveness fail trên Windows** — không chỉnh `HealthChecks:System:DiskDrives` / `MonitorFiles`.
- **Trùng tên check** — hai `AddCheck` cùng `name` gây lỗi hoặc hành vi khó đoán.
- **Nhầm tag** — đặt check DB dưới tag `liveness` làm liveness phụ thuộc DB (trái với mô hình Jarvis: dependency cứng thuộc **readiness**).
- **Thiếu PackageReference** — gọi `AddKafka` / `AddMongoDb` trên host nhưng không thêm package `AspNetCore.HealthChecks.Kafka` / `MongoDb` vào **csproj của host** (transitive từ `Jarvis.HealthChecks` **không** kéo theo mọi extension; Jarvis chỉ khai báo sẵn UI, UI.Client, System, Prometheus — xem `Jarvis.HealthChecks.csproj`).

---

## 11. Hệ sinh thái Xabaril — thêm healthcheck cho thành phần khác

Nguồn chính: **[Xabaril / AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)** — tập hợp package `AspNetCore.HealthChecks.*` (health check cho SQL Server, Npgsql, Redis, MongoDB, RabbitMQ, Kafka, Elasticsearch, AWS, Azure, Network/Uris, Hangfire, SignalR, v.v.). README và bảng NuGet trên repo là **danh mục đầy đủ**; khi thêm dependency mới, tra tên package và API `Add…` tương ứng trong tài liệu/NuGet của package đó.

### 11.1 Liên hệ với Jarvis

| Thành phần | Ghi chú |
|------------|---------|
| `Jarvis.HealthChecks` | Đã reference một phần stack Xabaril: **UI**, **UI.Client**, **System**, **Prometheus.Metrics** (ví dụ phiên bản 9.x cùng dòng .NET 9). |
| Extension khác (MongoDB, SQS, Uris, …) | Thêm **`PackageReference`** vào **project host** (ví dụ `Sample`) hoặc thư viện readiness dùng chung — **phiên bản nên lệch tối thiểu** với các package HealthChecks hiện có (cùng major 9.x cho .NET 9). |
| `IHealthChecksBuilder` | Mọi `AddXxx(...)` của Xabaril gắn vào **cùng pipeline** với Jarvis: sau `builder.AddHealthChecks()` (Jarvis), trong extension readiness gọi `builder.Services.AddHealthChecks()` rồi chuỗi `AddRabbitMq` / `AddSqlServer` / … |

### 11.2 Quy trình thêm một check mới (từ Xabaril)

1. Trên GitHub, mục **Health Checks** / bảng package: xác định tên NuGet (vd. `AspNetCore.HealthChecks.Rabbitmq`).
2. Thêm package vào `.csproj` host: `dotnet add package AspNetCore.HealthChecks.Rabbitmq` (hoặc chỉnh version trong repo).
3. Trong extension readiness (sau `var healthChecks = builder.Services.AddHealthChecks();`), gọi overload phù hợp — thường có `name`, `failureStatus`, **`tags`**, `timeout`, connection string hoặc factory `serviceProvider => …`.
4. Với dependency hạ tầng / broker / cloud: đặt **`tags: [HealthCheckTags.Readiness]`** và `failureStatus: HealthStatus.Unhealthy` (hoặc `Degraded` nếu chỉ cảnh báo).
5. Ưu tiên đọc connection / URL từ config (pattern đường dẫn key đầy đủ như mục 9).
6. Build, gọi `/health/ready` và `/health` để xác nhận entry mới.

### 11.3 Gợi ý theo nhóm (không thay thế bảng đầy đủ trên repo)

- **CSDL / lưu trữ**: SqlServer, Npgsql, MySql, Oracle, MongoDB, Redis, CosmosDb, RavenDB, Sqlite, ClickHouse, …
- **Message / stream**: RabbitMQ, Kafka, Nats, Azure Service Bus / Event Hubs, SQS, SNS, …
- **Cloud**: AWS (S3, Secrets Manager, …), Azure (Key Vault, Blobs, Tables, …), GCP Firestore, …
- **Tìm kiếm / analytics**: Elasticsearch, Solr, Azure Search, …
- **Mạng / HTTP**: package **Uris** (nhóm URI), **Network** (TCP, DNS, FTP, …) — có thể dùng thay `IHealthCheck` tự viết cho probe đơn giản.
- **Khác**: Consul, Kubernetes, Hangfire, SignalR, Dapr, …

Chi tiết từng overload: README repo, thư mục `doc/`, hoặc trang NuGet của package.

### 11.4 Publishers & Prometheus (tham khảo)

Repo Xabaril còn có **publisher** (Application Insights, Datadog, Seq, …) và **Prometheus Gateway** — khác với exporter middleware **`AspNetCore.HealthChecks.Prometheus.Metrics`** (Jarvis đã dùng kiểu pull qua `UseHealthChecksPrometheusExporter`). Khi thêm publisher Xabaril, cân nhắc trùng chức năng với **`HealthStatusPublisher`** (log) của Jarvis; tránh gửi trùng metric/log không cần thiết.

### 11.5 Ví dụ (RabbitMQ, readiness)

```csharp
// Sample.csproj: PackageReference Include="AspNetCore.HealthChecks.Rabbitmq" Version="9.0.0" (hoặc bản tương thích)

healthChecks.AddRabbitMQ(
    rabbitConnectionString: configuration["Messaging:RabbitMQ:Uri"]!,
    name: "rabbitmq",
    failureStatus: HealthStatus.Unhealthy,
    tags: [HealthCheckTags.Readiness],
    timeout: probeTimeout);
```

*(Tên phương thức / tham số có thể khác chút giữa các phiên bản package — luôn đối chiếu IntelliSense hoặc README package.)*
