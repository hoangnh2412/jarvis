# Workflow: Thêm handler CQRS

Áp dụng khi **đã có** `AddCoreApplication()` và cần thêm command/query.

## Checklist

```text
- [ ] 1. Định nghĩa command/query trong Application.Contracts hoặc Application
- [ ] 2. Implement ICommandHandler / IQueryHandler
- [ ] 3. AddScoped handler trong ApplicationLayerExtension
- [ ] 4. Controller/API gọi dispatcher.SendAsync
- [ ] 5. Unit test handler (tùy project)
```

## Handler đăng ký

```csharp
builder.Services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
// hoặc đăng ký interface marker của app
```

## Async

Jarvis hỗ trợ async dispatcher — handler implement async method tương ứng contract.

## Liên quan EF

Handler inject `IAppUnitOfWork` / repository — xem [entityframework-dotnet](../../entityframework-dotnet/README.md). Job: `SwitchDbContextAsync` trước khi query.
