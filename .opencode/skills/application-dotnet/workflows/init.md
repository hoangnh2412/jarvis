# Workflow: Khởi tạo Jarvis.Application

Áp dụng khi project **Application** chưa có `AddCoreApplication()`.

## Checklist

```text
- [ ] 1. Package Jarvis.Application.Contracts + Jarvis.Application
- [ ] 2. Reference Jarvis.Domain
- [ ] 3. ApplicationLayerExtension — AddCoreApplication()
- [ ] 4. Host gọi AddApplicationLayer() (hoặc tương đương)
- [ ] 5. Đăng ký ít nhất một ICommandHandler / IQueryHandler
- [ ] 6. dotnet build
```

## Bước 1 — Packages (Application csproj)

```xml
<PackageReference Include="Jarvis.Application.Contracts" Version="1.2.1" />
<PackageReference Include="Jarvis.Application" Version="1.2.1" />
```

## Bước 2 — Extension

[templates/application-extension.cs](../templates/application-extension.cs):

```csharp
public static IHostApplicationBuilder AddApplicationLayer(this IHostApplicationBuilder builder)
{
    builder.AddCoreApplication();
    // Đăng ký handler:
    // builder.Services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
    return builder;
}
```

## Bước 3 — Handler mẫu

[templates/command-handler.cs](../templates/command-handler.cs)

## Bước 4 — Controller (Host)

```csharp
public class OrdersController(ICommandDispatcher commands) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderCommand cmd, CancellationToken ct)
        => Ok(await commands.SendAsync(cmd, ct));
}
```

## Validate

- DI resolve dispatcher + handler
- Không circular reference Application → Host

## Anti-patterns

- Đăng ký handler trong Domain layer
- Quên `AddScoped` handler — runtime DI fail
