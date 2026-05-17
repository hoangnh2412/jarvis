---
name: jarvis-dotnet-healthchecks
description: Cài Jarvis.HealthChecks — package và bootstrap tối thiểu. Chi tiết readiness providers dùng skill healthcheck-dotnet.
dependencies:
  - Jarvis.HealthChecks
---

# Health checks

## Package

| PackageId | Version* |
|---|---|
| `Jarvis.HealthChecks` | 1.0.0 |

## Bootstrap tối thiểu

```csharp
using Jarvis.HealthChecks;

builder.AddHealthChecks();
// builder.Add{App}ReadinessHealthChecks(); — host-owned

var app = builder.Build();
app.UseHealthChecks();
```

Endpoints: `/health/live`, `/health/ready`, `/health/startup`, `/health`.

## Config

```json
{
  "HealthChecks": {
    "DefaultTimeoutSeconds": 5,
    "MarkStartupCompleteOnApplicationStarted": true,
    "Readiness": { }
  }
}
```

## Skill chuyên sâu

Providers (PostgreSQL, MySQL, Redis, …), workflows init/add:

→ **[healthcheck-dotnet](../../healthcheck-dotnet/SKILL.md)**
