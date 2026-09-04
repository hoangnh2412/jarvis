# Scaffold → skill `*-dotnet`

Template scaffold **đã wire tối thiểu** (F5 Swagger). **Không** nhân đôi hướng dẫn module — mở skill độc lập khi init/add/bổ sung.

Hub: [.opencode/README.md](../../../README.md) (repo Jarvis gốc).

## Đã có trong template (chỉ chỉnh khi cần)

| Vùng code | Skill | File template |
|-----------|-------|----------------|
| `AddCoreJson` / CORS / WebApi / wrapper | [foundation-dotnet](../../foundation-dotnet/README.md) | [layers/HostLayerExtension.cs](layers/HostLayerExtension.cs) |
| `AddCoreApplication` | [application-dotnet](../../application-dotnet/README.md) | [layers/ApplicationLayerExtension.cs](layers/ApplicationLayerExtension.cs) |
| `AddJarvisCaching` → `AddEntityFramework` → `AddCoreDbContext` | [caching-dotnet](../../caching-dotnet/README.md) · [entityframework-dotnet](../../entityframework-dotnet/README.md) | [layers/InfrastructureLayerExtension.cs](layers/InfrastructureLayerExtension.cs) |
| `AddJarvisOpenTelemetry` / enricher | [telemetry-dotnet](../../telemetry-dotnet/README.md) | HostLayerExtension, Enrich*Service.cs |
| `AddCoreSwagger` | [swashbuckle-dotnet](../../swashbuckle-dotnet/README.md) | HostLayerExtension |
| `AddHealthChecks` | [healthcheck-dotnet](../../healthcheck-dotnet/README.md) | HostLayerExtension |

## Chưa wire — thêm qua skill

| Nhu cầu | Skill | Workflow |
|---------|-------|----------|
| JWT / API Key / Cognito | [authentication-dotnet](../../authentication-dotnet/README.md) | `workflows/init.md` + `providers/*` |
| SMTP email | [notification-dotnet](../../notification-dotnet/README.md) | `providers/mailkit-smtp/SKILL.md` |
| Redis cache / invalidation | [caching-dotnet](../../caching-dotnet/README.md) | `workflows/add.md` |
| EF đổi pattern (single / dedicated / hybrid) | [entityframework-dotnet](../../entityframework-dotnet/README.md) | `patterns/*` |
| Blob FileSystem / MinIO | [blobstoring-dotnet](../../blobstoring-dotnet/README.md) | `workflows/init.md` |
| Swagger security scheme | [swashbuckle-dotnet](../../swashbuckle-dotnet/README.md) | `providers/jwt-security`, `api-key-security` |
| OTEL Redis / EF trace | [telemetry-dotnet](../../telemetry-dotnet/README.md) | `providers/redis`, `entityframework` |
| Đọc dashboard runtime CLR | [analyze-metric-dotnet](../../analyze-metric-dotnet/README.md) | `workflows/analyze.md` |

## Prompt sau scaffold (copy)

```text
@.opencode/skills/authentication-dotnet/providers/jwt/SKILL.md
Thêm JWT cho {Product}.Host
```

```text
@.opencode/skills/caching-dotnet/workflows/add.md
Bật Redis distributed + memory invalidation cho {Product}.Host
```

```text
@.opencode/skills/entityframework-dotnet/patterns/single-db/SKILL.md
Đổi EF sang single DB multitenancy cho {Product}
```

Orchestrator solution: [jarvis-dotnet](../README.md) → `workflows/add.md`.
