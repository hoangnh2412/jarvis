# Hướng dẫn sử dụng Jarvis với OpenCode / AI Agent

> **Đã chuyển:** Bản đồ skill và hướng dẫn sử dụng nằm tại **[`.opencode/README.md`](../.opencode/README.md)** và **README.md** trong từng skill (`jarvis-dotnet`, `telemetry-dotnet`, `healthcheck-dotnet`, `code-review`). Thư mục `docs/` không còn là nguồn chính cho skill AI.

Tài liệu dưới đây giữ làm tham chiếu lịch sử; ưu tiên link trong `.opencode/`.

---

## Mục lục

1. [Tổng quan skill trong `.opencode/`](#tổng-quan-skill-trong-opencode)
2. [Nguyên tắc: ba lớp tài liệu](#nguyên-tắc-ba-lớp-tài-liệu)
3. [Chọn skill nào khi nào](#chọn-skill-nào-khi-nào)
4. [Cách gọi skill trong OpenCode / Cursor](#cách-gọi-skill-trong-opencode--cursor)
5. [Bảng prompt mẫu](#bảng-prompt-mẫu)
6. [Đầu vào agent cần](#đầu-vào-agent-cần)
7. [Hướng dẫn từng skill](#hướng-dẫn-từng-skill)
8. [Chỉnh README gốc repo](#chỉnh-readme-gốc-repo)
9. [Phân vai: README gốc vs skill](#phân-vai-readme-gốc-vs-skill)
10. [Thứ tự triển khai tài liệu (đội nội bộ)](#thứ-tự-triển-khai-tài-liệu-đội-nội-bộ)

---

## Tổng quan skill trong `.opencode/`

Các skill nằm tại `.opencode/skills/`. Mỗi skill có cấu trúc nội bộ thống nhất:

| Thành phần | Vai trò |
|------------|---------|
| **`SKILL.md`** | Quy tắc, catalog, output bắt buộc — **dành cho agent** |
| **`workflows/`** | Checklist từng bước (scaffold, init, add) |
| **`modules/` hoặc `providers/`** | Chi tiết theo module / dependency |
| **`templates/`** | Code mẫu copy vào solution |
| **`README.md`** | Hướng dẫn cho **người** (khi có) |

### Danh sách skill hiện có

| Skill | Vai trò | Hướng dẫn cho người |
|-------|---------|---------------------|
| [`jarvis-dotnet`](../.opencode/skills/jarvis-dotnet/SKILL.md) | Orchestrator: scaffold / init / add module Jarvis | [README](../.opencode/skills/jarvis-dotnet/README.md) |
| [`healthcheck-dotnet`](../.opencode/skills/healthcheck-dotnet/SKILL.md) | `Jarvis.HealthChecks`: init + provider (PostgreSQL, Redis, …) | Xem [mục healthcheck](#healthcheck-dotnet) |
| [`telemetry-dotnet`](../.opencode/skills/telemetry-dotnet/SKILL.md) | `Jarvis.OpenTelemetry`: trace / metric / log OTLP | Xem [mục telemetry](#telemetry-dotnet) |
| [`code-review`](../.opencode/skills/code-review/SKILL.md) | Review PR C#/.NET trước merge | [README đầy đủ](../.opencode/skills/code-review/README.md) — **chuẩn mẫu** |

**Chuẩn mẫu cho README skill:** [`.opencode/skills/code-review/README.md`](../.opencode/skills/code-review/README.md) — cấu trúc *khi nào dùng → cách gọi → chuẩn bị → prompt mẫu → đọc kết quả → tài liệu liên quan*.

---

## Nguyên tắc: ba lớp tài liệu

```text
README.md (repo)                         → Giới thiệu framework + bắt đầu nhanh
docs/tutorial-index.md (file này)        → Hub: skill nào, khi nào, prompt mẫu
.opencode/skills/<skill>/README.md       → Hướng dẫn chi tiết từng skill (nên bổ sung)
.opencode/skills/<skill>/SKILL.md        → Chỉ cho agent — không copy dài sang README
```

| Nội dung | Đặt ở đâu | Lý do |
|----------|-----------|--------|
| Kiến trúc 5 layer, module Jarvis, bảng NuGet | [README.md](../README.md) | Onboarding dev mới |
| «Tôi muốn X → skill Y → prompt Z» | **docs/tutorial-index.md** (file này) | Tránh lẫn nhiều skill |
| Prompt, chuẩn bị, đọc output | `skills/<name>/README.md` | Giống `code-review` |
| Quy tắc kỹ thuật, checklist agent | `SKILL.md` + `workflows/` | Agent đọc khi thực thi |

**Không** copy nguyên `SKILL.md` sang README — chỉ link và tóm tắt một đến hai câu.

---

## Chọn skill nào khi nào

```text
Folder trống, tạo backend mới          → jarvis-dotnet (scaffold)
Đã có solution, gắn Jarvis             → jarvis-dotnet (init)
Thêm JWT / EF / Caching / …            → jarvis-dotnet (add) + modules/*.md
Thêm /health/ready PostgreSQL          → healthcheck-dotnet (add)
Bật OTLP trace / metric / log          → telemetry-dotnet (init / add)
Trước khi mở PR                        → code-review
```

### Decision tree (chi tiết)

| Tình huống | Skill | Workflow / tài liệu |
|------------|-------|---------------------|
| Tạo repo `{product}-backend` từ đầu, 5 project, F5 Swagger | `jarvis-dotnet` | [workflows/scaffold.md](../.opencode/skills/jarvis-dotnet/workflows/scaffold.md) |
| Solution .NET có sẵn, muốn cài Jarvis theo layer | `jarvis-dotnet` | [workflows/init.md](../.opencode/skills/jarvis-dotnet/workflows/init.md) |
| Đã có Jarvis, thêm module (JWT, EF, Caching, …) | `jarvis-dotnet` | [workflows/add.md](../.opencode/skills/jarvis-dotnet/workflows/add.md) + [modules/](../.opencode/skills/jarvis-dotnet/modules/) |
| Chưa có health endpoint Jarvis | `healthcheck-dotnet` | [workflows/init.md](../.opencode/skills/healthcheck-dotnet/workflows/init.md) |
| Đã có `/health/live`, thêm readiness dependency | `healthcheck-dotnet` | [workflows/add.md](../.opencode/skills/healthcheck-dotnet/workflows/add.md) + [providers/](../.opencode/skills/healthcheck-dotnet/providers/) |
| Chưa có OpenTelemetry Jarvis | `telemetry-dotnet` | [workflows/init.md](../.opencode/skills/telemetry-dotnet/workflows/init.md) |
| Đã có OTEL core, thêm enrich / EF / Redis plug-in | `telemetry-dotnet` | [workflows/add.md](../.opencode/skills/telemetry-dotnet/workflows/add.md) + [providers/](../.opencode/skills/telemetry-dotnet/providers/) |
| Review diff trước PR | `code-review` | [SKILL.md](../.opencode/skills/code-review/SKILL.md) + [README](../.opencode/skills/code-review/README.md) |

---

## Cách gọi skill trong OpenCode / Cursor

Khuyến nghị **một cú pháp** cho mọi skill — tham chiếu file cụ thể để agent load đúng ngữ cảnh:

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md

Scaffold solution .NET 9 tên Acme từ folder trống, dùng NuGet feed nội bộ.
```

Hoặc gọi orchestrator:

```text
@.opencode/skills/jarvis-dotnet/SKILL.md

Dùng skill jarvis-dotnet scaffold: Product=Acme, product=acme
```

**Lưu ý:** Trong Cursor, có thể dùng `@` kèm đường dẫn tương đương tới file skill hoặc workflow.

---

## Bảng prompt mẫu

| Mục tiêu | Prompt gợi ý |
|----------|----------------|
| **Scaffold** từ folder trống | `Tạo solution backend .NET 9 tên {Product}, theo @.opencode/skills/jarvis-dotnet/workflows/scaffold.md` |
| **Init** Jarvis vào solution có sẵn | `Cài Jarvis vào solution có sẵn … theo @.opencode/skills/jarvis-dotnet/workflows/init.md` |
| **Add** module | `Thêm {module} vào {Product}.Host theo @.opencode/skills/jarvis-dotnet/modules/{module}/SKILL.md` |
| **JWT** | `Thêm JWT vào {Product}.Host theo @.opencode/skills/jarvis-dotnet/modules/authentication/SKILL.md` |
| **Health PostgreSQL** | `Thêm readiness PostgreSQL theo @.opencode/skills/healthcheck-dotnet/providers/postgresql/SKILL.md` |
| **OpenTelemetry** | `Init Jarvis OpenTelemetry theo @.opencode/skills/telemetry-dotnet/workflows/init.md` |
| **Review PR** | `Review PR theo @.opencode/skills/code-review/SKILL.md, base: main` |

### Prompt scaffold «đủ thông tin»

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md

Scaffold backend .NET 9:
- Product: Acme (PascalCase)
- product: acme (kebab)
- Jarvis: NuGet feed nội bộ (không monorepo)
- Mặc định: Swagger + health live + ping, chưa bật PostgreSQL readiness

Sau khi xong: dotnet build và báo URL Swagger.
```

### Prompt review PR (thường dùng)

```text
Review code trước PR theo @.opencode/skills/code-review/SKILL.md
Base branch: main
```

Solution theo kiến trúc Jarvis — bổ sung trong prompt:

```text
Review PR theo code-review; solution theo Jarvis layered architecture
```

---

## Đầu vào agent cần

| Skill | Nên nêu trong prompt |
|-------|----------------------|
| **jarvis-dotnet** (scaffold) | `{Product}` PascalCase, `{product}` kebab, `{JarvisRoot}` monorepo **hoặc** NuGet feed |
| **jarvis-dotnet** (init/add) | Tên solution, project Host, module cần thêm |
| **healthcheck-dotnet** | Provider (PostgreSQL, Redis, …), config path (`ConnectionStrings:MainDb`), không hardcode secret |
| **telemetry-dotnet** | OTLP endpoint (env), có/không enrich, plug-in EF/Redis, sampling |
| **code-review** | Base branch (`main`, `develop`), hoặc danh sách file; commit/stage trước khi review |

---

## Hướng dẫn từng skill

### jarvis-dotnet

**Orchestrator chính** — scaffold solution phân lớp + cài Jarvis trên ASP.NET Core .NET 9.

**Hướng dẫn:** [`.opencode/skills/jarvis-dotnet/README.md`](../.opencode/skills/jarvis-dotnet/README.md) · EF: [modules/entityframework/README.md](../.opencode/skills/jarvis-dotnet/modules/entityframework/README.md) · Cache: [modules/caching/README.md](../.opencode/skills/jarvis-dotnet/modules/caching/README.md)

| Luồng | Workflow | Khi nào |
|-------|----------|---------|
| Scaffold | [workflows/scaffold.md](../.opencode/skills/jarvis-dotnet/workflows/scaffold.md) | Folder trống → solution 5 project + test |
| Init | [workflows/init.md](../.opencode/skills/jarvis-dotnet/workflows/init.md) | Solution có sẵn, gắn Jarvis theo layer |
| Add | [workflows/add.md](../.opencode/skills/jarvis-dotnet/workflows/add.md) | Đã có foundation, thêm module |

**Cấu trúc solution chuẩn:**

```text
{product}-backend/
├── src/{Product}.sln
│   ├── {Product}.Domain.Shared
│   ├── {Product}.Domain
│   ├── {Product}.Application      → Jarvis.Application
│   ├── {Product}.Infrastructure   → Jarvis.EntityFramework
│   └── {Product}.Host             → Jarvis.Mvc, OTEL, HealthChecks, Swagger
└── tests/
```

**Composition root:** `Program.cs` chỉ gọi `AddHostLayer()` / `UseHostLayer()`.

**Modules (atomic)** — chỉ đọc file cần dùng:

| Module | SKILL |
|--------|-------|
| Foundation | [modules/foundation/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/foundation/SKILL.md) |
| Application (CQRS) | [modules/application/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/application/SKILL.md) |
| Entity Framework | [modules/entityframework/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/entityframework/SKILL.md) |
| Caching | [modules/caching/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/caching/SKILL.md) |
| Authentication | [modules/authentication/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/authentication/SKILL.md) |
| Blob storing | [modules/blob-storing/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/blob-storing/SKILL.md) |
| Notification | [modules/notification/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/notification/SKILL.md) |
| Swashbuckle | [modules/swashbuckle/SKILL.md](../.opencode/skills/jarvis-dotnet/modules/swashbuckle/SKILL.md) |
| OpenTelemetry | [telemetry-dotnet](../.opencode/skills/telemetry-dotnet/README.md) (skill độc lập; không còn module stub trong jarvis-dotnet) |
| Health checks | [healthcheck-dotnet](../.opencode/skills/healthcheck-dotnet/README.md) (skill độc lập) |

**Hai cách cài Jarvis:**

| Cách | Khi nào |
|------|---------|
| **ProjectReference** | Monorepo cạnh repo Jarvis (`{JarvisRoot}`) |
| **NuGet** | Repo độc lập, feed nội bộ |

**Lưu ý package:** folder repo `Jarvis.Authentication.*` → NuGet **`Jarvis.Authentications.*`** (có chữ **s**).

**Thứ tự DI (develop):** `AddJarvisCaching()` → `AddEntityFramework()` → `AddCoreDbContext`. Template scaffold đã áp dụng.

**Sau scaffold — kiểm tra:**

- Startup project: `{Product}.Host`
- `dotnet run --project src/{Product}.Host` → Swagger
- `GET /api/ping` — không cần DB
- `GET /health/live` — OK
- `GET /health/ready` — cần PostgreSQL nếu bật DB check

**Output bắt buộc (scaffold):** solution 5 project + 2 test, `*LayerExtension.cs`, `Program.cs` mỏng, `appsettings` + `launchSettings`, `dotnet build` thành công.

---

### healthcheck-dotnet

Skill chuyên **`Jarvis.HealthChecks`** — liveness, startup, readiness.

| Tình huống | Workflow |
|------------|----------|
| Project chưa có healthcheck | [workflows/init.md](../.opencode/skills/healthcheck-dotnet/workflows/init.md) |
| Đã có core, thêm dependency | [workflows/add.md](../.opencode/skills/healthcheck-dotnet/workflows/add.md) |

**Quy tắc cốt lõi:**

- `builder.AddHealthChecks()` (Jarvis) **trước** readiness registrations
- `app.UseHealthChecks()` sau khi build pipeline
- Liveness = process/runtime; readiness = infrastructure
- **Không** đặt database vào liveness
- Config: `ConnectionStrings:MainDb` — không hardcode
- Timeout: `HealthChecks:DefaultTimeoutSeconds` (clamp 1–120)

**Endpoints:**

| Endpoint | Mục đích |
|----------|----------|
| `/health/live` | Liveness |
| `/health/ready` | Readiness |
| `/health/startup` | Startup probe |
| `/health` | Aggregate |

**Providers:**

| Provider | SKILL |
|----------|-------|
| PostgreSQL | [providers/postgresql/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/postgresql/SKILL.md) |
| MySQL | [providers/mysql/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/mysql/SKILL.md) |
| SQL Server | [providers/mssql/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/mssql/SKILL.md) |
| Oracle | [providers/oracle/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/oracle/SKILL.md) |
| Redis | [providers/redis/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/redis/SKILL.md) |
| Kafka | [providers/kafka/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/kafka/SKILL.md) |
| RabbitMQ | [providers/rabbitmq/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/rabbitmq/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/minio/SKILL.md) |
| HTTP | [providers/http/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/http/SKILL.md) |
| SignalR | [providers/signalr/SKILL.md](../.opencode/skills/healthcheck-dotnet/providers/signalr/SKILL.md) |

**Prompt mẫu:**

```text
@.opencode/skills/healthcheck-dotnet/workflows/add.md

Thêm readiness PostgreSQL cho project MyApp.Host.
Connection string: ConnectionStrings:MainDb trong appsettings.
```

---

### telemetry-dotnet

Skill chuyên **`Jarvis.OpenTelemetry`** — trace, metric, log OTLP.

| Tình huống | Workflow |
|------------|----------|
| Project chưa có Jarvis OTEL | [workflows/init.md](../.opencode/skills/telemetry-dotnet/workflows/init.md) |
| Đã có core, thêm instrumentation | [workflows/add.md](../.opencode/skills/telemetry-dotnet/workflows/add.md) |

**Quy tắc cốt lõi:**

- `AddJarvisOpenTelemetry(configuration, configureServices)` → `.ConfigureResource()` → logging / trace / metric
- Plug-in đăng ký **trong** callback `configureServices` (trước `Build()`)
- `app.UseJarvisOpenTelemetry()` khi cần enrich trace/log
- Config: `OTEL:Tracing`, `OTEL:Metric`, `OTEL:Logging`
- `HttpTraceEnrichment`: allowlist header — không capture toàn bộ header
- Không hard-code OTLP secrets; dùng env / secret store
- Options snapshot lúc startup — đổi config runtime cần restart

**Providers:**

| Provider | SKILL |
|----------|-------|
| Enrich trace/log | [providers/enrich/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/enrich/SKILL.md) |
| Redis trace | [providers/redis/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/redis/SKILL.md) |
| EF Core trace | [providers/entityframework/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/entityframework/SKILL.md) |
| Custom trace plug-in | [providers/trace-plugin/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/trace-plugin/SKILL.md) |
| Custom metric plug-in | [providers/metric-plugin/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/metric-plugin/SKILL.md) |
| Custom log exporter | [providers/logging-plugin/SKILL.md](../.opencode/skills/telemetry-dotnet/providers/logging-plugin/SKILL.md) |

**Biến môi trường OTLP (tham chiếu):**

| Mục đích | Ví dụ |
|----------|-------|
| OTLP chung | `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_HEADERS` |
| Trace | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` |
| Metrics | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT` |
| Logs | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` |
| Resource | `OTEL_RESOURCE_ATTRIBUTES`, `OTEL_SERVICE_NAME` |

**Checklist production (tóm tắt):**

- Sampling: `Tracing:Sampler` + `TraceIdRatio` hợp lý
- PII: allowlist header chặt
- Cardinality: tag ổn định
- Noise: `ExcludedPathPrefixes` cho `/health`, `/swagger`
- Secrets: OTLP headers qua env, không commit

**Prompt mẫu:**

```text
@.opencode/skills/telemetry-dotnet/workflows/init.md

Init Jarvis OpenTelemetry cho MyApp.Host.
OTLP endpoint qua biến môi trường, không commit secret.
```

---

### code-review

Review code **C#/.NET** trước PR — checklist đầy đủ, ưu tiên production risk.

**Tài liệu đầy đủ:** [`.opencode/skills/code-review/README.md`](../.opencode/skills/code-review/README.md)

**Khi nào dùng:**

- Trước khi tạo PR trên GitHub
- Sau implement feature/fix, bắt bug sớm
- Cần ý kiến về async / EF / DI / security / concurrency

**Không dùng cho:** format code, naming cosmetic, refactor lớn không liên quan PR.

**Cách gọi:**

```text
Review PR theo @.opencode/skills/code-review/SKILL.md
Base branch: main
```

**Chuẩn bị:**

| Việc nên làm | Lý do |
|--------------|--------|
| Commit hoặc stage thay đổi | Agent lấy diff từ git |
| Biết branch gốc | Mặc định `git diff <base>...HEAD` |
| Ở đúng repo / working tree | Diff đúng phạm vi PR |

**Đọc kết quả:**

| Section | Ý nghĩa |
|---------|---------|
| **Critical Issues** | Phải sửa trước merge |
| **Suggestions** | Nên sửa — rủi ro trung bình |
| **Best Practices & Improvements** | Cải thiện không khẩn cấp |
| **Summary** | `merge-ready` / `needs changes` / `blocked` |
| **Commit Message** | Chỉ khi bạn yêu cầu trong prompt |

**Sau review:** sửa Critical trước → cân nhắc Suggestions → chạy lại review sau thay đổi lớn (EF, auth, transaction).

---

## Chỉnh README gốc repo

[README.md](../README.md) hiện có phầ **Get started** (Cách 1–4) mô tả đúng ý định. Nên bổ sung:

1. Link tới **file này:** *«Chi tiết skill AI: [docs/tutorial-index.md](tutorial-index.md)»*
2. Mỗi «Cách» trỏ workflow:
   - Cách 1 (Scaffold) → `jarvis-dotnet/workflows/scaffold.md`
   - Cách 2 (Add) → `jarvis-dotnet/workflows/add.md`
   - Cách 3 (Init) → `jarvis-dotnet/workflows/init.md`
3. Giữ prompt tiếng Việt tự nhiên, kèm `@.opencode/skills/...` để agent load đúng file

**Bốn cách bắt đầu (tóm tắt từ README):**

| Cách | Mô tả | Skill / workflow |
|------|--------|------------------|
| **1 — Scaffold** (khuyến khích) | Folder trống → solution + Jarvis → F5 Swagger | `jarvis-dotnet` / scaffold |
| **2 — Add** | Đã có foundation, thêm module | `jarvis-dotnet` / add + modules |
| **3 — Init** | Solution .NET có sẵn, cài Jarvis | `jarvis-dotnet` / init |
| **4 — Manual** | `dotnet new`, `dotnet add package` tay | Không cần skill |

---

## Phân vai: README gốc vs skill

| Câu hỏi | Trả lời ở |
|---------|-----------|
| Jarvis là gì, 5 layer, module nào? | [README.md](../README.md) |
| Tôi muốn tạo project mới bằng AI? | **docs/tutorial-index.md** + `jarvis-dotnet` workflows |
| Thêm PostgreSQL vào `/health/ready`? | `healthcheck-dotnet` providers |
| Bật OTLP / enrich trace? | `telemetry-dotnet` workflows |
| Review PR trước merge? | `code-review` README |

---

## Khung README cho từng skill (đội nội bộ)

Khi bổ sung `README.md` trong từng thư mục skill, dùng **cùng khung** như `code-review`:

1. **Khi nào dùng / không dùng**
2. **Cách gọi** (`@.opencode/skills/.../SKILL.md` hoặc workflow cụ thể)
3. **Chuẩn bị** (git, branch, biến môi trường, appsettings)
4. **Prompt mẫu** (3–6 case thường gặp)
5. **Agent sẽ làm gì** (link workflow, output bắt buộc từ `SKILL.md`)
6. **Sau khi chạy** (build, `dotnet run`, kiểm endpoint)
7. **Tài liệu liên quan**

### Thứ tự triển khai tài liệu (đội nội bộ)

1. **docs/tutorial-index.md** (file này) — hub + decision tree + bảng prompt
2. **`.opencode/skills/jarvis-dotnet/README.md`** — skill dùng nhiều nhất
3. **`healthcheck-dotnet/README.md`** + **`telemetry-dotnet/README.md`**
4. **Cập nhật README.md gốc** — đoạn link + workflow
5. (Tuỳ chọn) **`docs/ai-workflows.md`** hoặc CONTRIBUTING nếu team lớn

---

## Tóm tắt

- **`SKILL.md`** = sổ tay cho **agent**; **README skill** = sổ tay cho **người**.
- Dùng **`code-review/README.md`** làm template cho các skill còn thiếu README.
- **`docs/tutorial-index.md`** là bản đồ trung tâm — tránh nhồi hết vào README framework.
- Prompt nên **`@` path cụ thể** (`workflows/`, `modules/`, `providers/`) thay vì chỉ nói «dùng skill jarvis».

---

## Liên kết nhanh

| Tài liệu | Path |
|----------|------|
| Framework overview | [README.md](../README.md) |
| Jarvis orchestrator | [.opencode/skills/jarvis-dotnet/SKILL.md](../.opencode/skills/jarvis-dotnet/SKILL.md) |
| Solution structure reference | [.opencode/skills/jarvis-dotnet/reference/solution-structure.md](../.opencode/skills/jarvis-dotnet/reference/solution-structure.md) |
| Code review (chuẩn README) | [.opencode/skills/code-review/README.md](../.opencode/skills/code-review/README.md) |
| Health checks | [.opencode/skills/healthcheck-dotnet/SKILL.md](../.opencode/skills/healthcheck-dotnet/SKILL.md) |
| OpenTelemetry | [.opencode/skills/telemetry-dotnet/SKILL.md](../.opencode/skills/telemetry-dotnet/SKILL.md) |
