# foundation-dotnet

Skill thiết lập **Jarvis foundation** — JSON chuẩn, CORS, WebApi, `IWorkContext`, middleware `ApiResponseWrapper`.

Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Host chưa có Jarvis Mvc/Json | [workflows/init.md](./workflows/init.md) |
| Chỉnh CORS / wrapper / JSON policy | [workflows/add.md](./workflows/add.md) |

Scaffold solution mới → [jarvis-dotnet](../jarvis-dotnet/README.md) (đã wire foundation trong template).

## Cách gọi

```text
@.opencode/skills/foundation-dotnet/workflows/init.md

Init Jarvis foundation cho MyApp.Host — ApiResponseWrapper cho /api.
```

## Extension chính

| Extension | Mục đích |
|-----------|----------|
| `AddCoreJson` | Controllers + JSON/Newtonsoft + BadRequest `BaseResponse` |
| `AddCoreWebApi` | `AddEndpointsApiExplorer`, `HttpContextAccessor` |
| `AddCoreDomain` | `IWorkContext` |
| `AddCoreCors` | CORS từ section `Cors` |
| `UseCoreMiddleware<T>` | Pipeline middleware (wrapper, …) |

Mẫu: [templates/program-setup.cs](./templates/program-setup.cs).

## Liên quan

- [swashbuckle-dotnet](../swashbuckle-dotnet/README.md) — sau `AddCoreWebApi`
- [jarvis-dotnet](../jarvis-dotnet/README.md) — scaffold
