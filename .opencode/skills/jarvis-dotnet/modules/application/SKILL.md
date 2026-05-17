---
name: jarvis-dotnet-application
description: Cài Jarvis.Application — CQRS dispatcher (command/query). Dùng khi project dùng pattern ICommand/IQuery với handler DI.
dependencies:
  - Jarvis.Application
  - Jarvis.Application.Contracts
---

# Application (CQRS)

## Packages

| Project | PackageId |
|---|---|
| Jarvis.Application.Contracts | `Jarvis.Application.Contracts` |
| Jarvis.Application | `Jarvis.Application` |

Phụ thuộc `Jarvis.Domain`.

## Program.cs

```csharp
using Jarvis.Application;

builder.AddCoreApplication();
// hoặc chỉ dispatcher:
// builder.AddCommandQuery();
```

## Đăng ký handler

Handler implement `ICommandHandler<>`, `IQueryHandler<>`, … — đăng ký `AddScoped` trong host:

```csharp
builder.Services.AddScoped<IMyCommandHandler, MyCommandHandler>();
```

## Extension

| Extension | Mục đích |
|---|---|
| `AddCoreApplication` | `AddCommandQuery()` |
| `AddCommandQuery` | `ICommandDispatcher`, `IQueryDispatcher`, async variants |
