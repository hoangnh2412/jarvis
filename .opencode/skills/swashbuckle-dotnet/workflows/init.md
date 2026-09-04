# Workflow: Khởi tạo Jarvis Swashbuckle

Áp dụng khi Host **chưa** có `AddCoreSwagger()`.

## Checklist

```text
- [ ] 1. Package Jarvis.Swashbuckle trên Host
- [ ] 2. AddCoreWebApi + ApiVersioning (nếu chưa có)
- [ ] 3. AddCoreSwagger + UseCoreSwagger
- [ ] 4. appsettings Swagger
- [ ] 5. GenerateDocumentationFile (tùy chọn examples)
- [ ] 6. Mở /swagger
```

## Bước 1 — Package

```xml
<PackageReference Include="Jarvis.Swashbuckle" Version="1.0.1" />
```

## Bước 2 — Program / HostLayerExtension

[templates/program-setup.cs](../templates/program-setup.cs):

```csharp
using Jarvis.Swashbuckle;

builder.AddCoreWebApi();
builder.Services.AddApiVersioning(options =>
{
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
  options.GroupNameFormat = "'v'VVV";
  options.SubstituteApiVersionInUrl = true;
});

builder.AddCoreSwagger();

var app = builder.Build();
app.UseCoreSwagger();
app.MapControllers();
```

## Bước 3 — appsettings

[templates/appsettings-swagger.json](../templates/appsettings-swagger.json)

## Bước 4 — Validate

- `Swagger:Enable` = true
- Browser `/swagger` — document v1
- Controller có `[ApiVersion("1.0")]` nếu dùng versioning

## Sau init

Security: [workflows/add.md](add.md) + [providers/](../providers/).
