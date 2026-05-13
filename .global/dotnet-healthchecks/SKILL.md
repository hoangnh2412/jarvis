---
name: dotnet-healthchecks
description: Thiết lập healthcheck cho project .NET C#
license: MIT
compatibility: opencode
metadata:
  audience: hoangnh
  workflow: github
---

# SKILL: Jarvis.HealthChecks — Core

## 1. Purpose

Hướng dẫn agent/developer tích hợp `Jarvis.HealthChecks`
vào ASP.NET Core:

- DI (`AddHealthChecks`)
- HTTP (`UseHealthChecks`)
- startup probe
- liveness
- readiness orchestration

Provider-specific readiness
(PostgreSQL, Oracle, MySQL...)
được tách riêng trong `rules/*.md`.

---

## 2. Scope

Áp dụng cho:

- ASP.NET Core minimal hosting
- `WebApplication`
- `/health/live`
- `/health/ready`
- `/health/startup`
- `/health`
- HealthChecks UI
- Prometheus exporter

Không áp dụng cho:

- gRPC health
- provider-specific implementation
- legacy Startup.cs hosting model

---

## 3. Role

Bạn là Senior Backend Engineer chịu trách nhiệm:

- thiết kế healthcheck đúng mô hình
- phân biệt liveness/readiness
- orchestration readiness providers
- tránh duplicate checks

---

## 4. Rules

- Always gọi `builder.AddHealthChecks()` trước provider registrations.
- Always gọi `app.UseHealthChecks()`.
- Always dùng:
  - liveness cho process/runtime
  - readiness cho infrastructure dependencies.
- Never đặt database dependency vào liveness.
- Prefer timeout từ:
  `HealthChecks:DefaultTimeoutSeconds`
- Prefer config path đầy đủ:
  `ConnectionStrings:MainDb`

---

## 5. Endpoints

| Endpoint | Purpose |
|---|---|
| `/health/live` | Process liveness |
| `/health/ready` | Infrastructure readiness |
| `/health/startup` | Startup workflow |
| `/health` | Full aggregate |

---

## 6. Startup Probe

Nếu:

```json
{
  "HealthChecks": {
    "MarkStartupCompleteOnApplicationStarted": true
  }
}
```

thì không cần gọi:

```csharp
MarkStartupComplete()
```

Nếu disable auto complete:

```json
{
  "HealthChecks": {
    "MarkStartupCompleteOnApplicationStarted": false
  }
}
```

thì phải gọi:

```csharp
MarkStartupComplete()
```

sau:
- migrate db
- warmup cache
- preload services

---

## 7. Process

1. Analyze Program.cs
2. Analyze readiness dependencies
3. Load required providers
4. Register healthchecks
5. Configure middleware
6. Validate endpoints

---

## 8. Configuration Rules

Always resolve config qua full path:

```csharp
configuration["ConnectionStrings:MainDb"]
```

Không hardcode connection string.

Timeout phải clamp:
- min: 1
- max: 120

---

## 9. Output

Bắt buộc trả về:

- Program.cs changes
- readiness registration
- appsettings.json updates
- validation checklist

---

## 10. Checklist

- builder.AddHealthChecks()
- app.UseHealthChecks()
- readiness tagged correctly
- startup probe validated
- provider package installed
- /health/ready working

---

## 11. Provider Loading

Chỉ load provider cần thiết từ:

```text
rules/
```

Ví dụ:
- `rules/postgresql.md`
- `rules/oracle.md`

Không load providers không sử dụng.

---

## 12. HealthCheck Structure

Tất cả readiness healthcheck phải được tách riêng trong folder:

```text
HealthChecks/
```

Mỗi dependency là một file riêng:

```text
HealthChecks/
├── PostgreSqlHealthCheck.cs
├── OracleHealthCheck.cs
├── MinIOHealthCheck.cs
├── RedisHealthCheck.cs
└── KafkaHealthCheck.cs
```

---

## Rules

- Always tách mỗi provider thành file riêng
- Always đặt logic healthcheck trong:
  `*HealthCheck.cs`
- Always gom registration tại:
  `ReadinessHealthCheckExtensions`
- Never viết toàn bộ readiness logic trực tiếp trong `Program.cs`
- Never combine nhiều provider trong cùng một healthcheck class

---

## Registration Pattern

```csharp
builder.AddHealthChecks();

builder.Services
    .AddHealthChecks()
    .AddCheck<PostgreSqlHealthCheck>("postgresql")
    .AddCheck<MinIOHealthCheck>("minio");
```

---

## Mapping Rules

| Rule File | Implementation |
|---|---|
| `providers/postgresql.md` | `PostgreSqlHealthCheck.cs` |
| `providers/oracle.md` | `OracleHealthCheck.cs` |
| `providers/minio.md` | `MinIOHealthCheck.cs` |
| `providers/redis.md` | `RedisHealthCheck.cs` |
