# application-dotnet

Skill tích hợp **Jarvis.Application** — CQRS dispatcher (`ICommand` / `IQuery`).

Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Layer Application chưa có Jarvis CQRS | [workflows/init.md](./workflows/init.md) |
| Thêm command/query handler | [workflows/add.md](./workflows/add.md) |

## Cách gọi

```text
@.opencode/skills/application-dotnet/workflows/init.md

Init Jarvis.Application cho MyApp.Application — AddCoreApplication + mẫu handler.
```

## Extension

| Extension | Mục đích |
|-----------|----------|
| `AddCoreApplication` | `AddCommandQuery()` — dispatcher |
| `AddCommandQuery` | Chỉ dispatcher (nếu không cần wrapper khác) |

Handler: `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResult>` — đăng ký `AddScoped` trong Application hoặc Host module.

Mẫu: [templates/application-extension.cs](./templates/application-extension.cs).

## Liên quan

- [jarvis-dotnet](../jarvis-dotnet/README.md) — scaffold Application project
- [entityframework-dotnet](../entityframework-dotnet/README.md) — handler dùng UoW/repository
