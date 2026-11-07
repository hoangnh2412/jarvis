# ImportExport Entity Registry — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cho phép user chọn đối tượng (entity key) khi tạo Excel template → BE dùng registry + reflection trả danh sách properties → UI auto-fill `fieldName` trong bảng mapping.

**Architecture:** Module `Platform.Modules.ImportExport` cung cấp **registry code-first** (`key → Type`) và **schema service** (reflection + DataAnnotations). Host/Sample đăng ký `ImportRow` DTO qua `Configure<ImportExportEntityRegistryOptions>`. Group Settings **`ImportExportEntities`** (tách khỏi `ImportExport`) khai báo catalog key read-only; dropdown UI đọc từ **Registry API**, không dùng `Type.GetType` từ Settings value. Import pipeline **không đổi** — vẫn trả `IReadOnlyDictionary<string, object?>`.

**Tech Stack:** .NET 9, Platform Setting, Platform.Modules.ImportExport (Contracts / Application / Api), React + react-hook-form + zod, xUnit tests.

## Global Constraints

- Không reflection lúc runtime import sang entity — chỉ schema discovery lúc tạo template.
- Module ImportExport không reference type nghiệp vụ của consumer (Sample).
- Reflect trên **Import DTO** (`*ImportRow`), không phải EF entity đầy đủ.
- Key lookup phải qua registry whitelist — unknown key → 404.
- Group Settings mới: `ImportExportEntities` — tách biệt group `ImportExport` (template code runtime).
- API route: `/api/v1/importexport/entities` và `/api/v1/importexport/entities/{key}/fields`.
- Lọc property mặc định: bỏ `Id`, `TenantId`; bỏ indexer; chỉ public instance readable.
- Không thay đổi contract `ColumnMapping`, `IImportProcessor`, `IImportDataPersister`.

---

## File map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `Platform.Modules.ImportExport/Models/ImportExportEntitySchema.cs` | DTO summary + field schema |
| Create | `Platform.Modules.ImportExport/Interfaces/IImportExportEntityRegistry.cs` | Registry port |
| Create | `Platform.Modules.ImportExport/Interfaces/IImportExportSchemaService.cs` | Schema port |
| Create | `Platform.Modules.ImportExport/Exceptions/ImportExportEntityNotFoundException.cs` | 404 entity |
| Modify | `Platform.Modules.ImportExport/Models/ErrorCodes.cs` | `ImportExportEntityNotFound` |
| Modify | `Platform.Modules.ImportExport/Constants/EngineMessages.cs` | Message helper |
| Create | `Platform.Modules.ImportExport.Application/Options/ImportExportEntityRegistryOptions.cs` | Configure entities |
| Create | `Platform.Modules.ImportExport.Application/Services/ImportExportEntityRegistry.cs` | In-memory registry |
| Create | `Platform.Modules.ImportExport.Application/Schemas/ReflectionFieldReader.cs` | GetProperties + annotations |
| Create | `Platform.Modules.ImportExport.Application/Services/ImportExportSchemaService.cs` | Facade |
| Create | `Platform.Modules.ImportExport.Application/DependencyInjection/ImportExportEntityRegistryExtensions.cs` | `AddImportExportEntity<TRow>()` |
| Modify | `Platform.Modules.ImportExport.Application/DependencyInjection/PlatformImportExportApplicationExtensions.cs` | Register services |
| Create | `Platform.Modules.ImportExport.Api/Controllers/ImportExportEntitiesController.cs` | GET endpoints |
| Create | `Sample/ImportExport/Controllers/ImportExportEntitiesController.cs` | Mirror cho Sample host |
| Create | `Sample/Settings/ImportExportEntitiesSettingDefinition.cs` | Group catalog |
| Create | `Sample/ImportExport/Models/StudentImportRow.cs` | DTO reflect |
| Modify | `Sample/ImportExport/SampleImportExportExtensions.cs` | Register Student |
| Modify | `Sample/Extensions/SampleSettingExtensions.cs` | Add provider |
| Create | `Platform.Modules.ImportExport.Tests/Schemas/ImportExportSchemaServiceTests.cs` | Unit tests |
| Modify | `frontend/src/lib/types/index.ts` | TS types |
| Modify | `frontend/src/lib/services/index.ts` | API client |
| Modify | `frontend/src/lib/index.ts` | Re-exports |
| Modify | `frontend/src/lib/pages/TemplateFormPage.tsx` | Entity select + auto-fill |
| Modify | `docs/SAD-Extensibility.md` | Document port |

---

## Task 1: Contracts — models, interfaces, exception

**Files:**
- Create: `Platform.Modules.ImportExport/Models/ImportExportEntitySchema.cs`
- Create: `Platform.Modules.ImportExport/Interfaces/IImportExportEntityRegistry.cs`
- Create: `Platform.Modules.ImportExport/Interfaces/IImportExportSchemaService.cs`
- Create: `Platform.Modules.ImportExport/Exceptions/ImportExportEntityNotFoundException.cs`
- Modify: `Platform.Modules.ImportExport/Models/ErrorCodes.cs`
- Modify: `Platform.Modules.ImportExport/Constants/EngineMessages.cs`

**Interfaces — produces:**

```csharp
// Models/ImportExportEntitySchema.cs
namespace Platform.Modules.ImportExport.Models;

public sealed record ImportExportEntitySummary(string Key, string DisplayName);

public sealed record ImportExportFieldSchema(
    string Name,
    string DisplayName,
    string DataType,
    bool Required,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    string? Regex);

public sealed record RegisteredImportExportEntity(string Key, Type RowType, string DisplayName);
```

```csharp
// Interfaces/IImportExportEntityRegistry.cs
namespace Platform.Modules.ImportExport.Interfaces;

public interface IImportExportEntityRegistry
{
    IReadOnlyList<ImportExportEntitySummary> List();
    bool TryGet(string key, out RegisteredImportExportEntity entity);
}
```

```csharp
// Interfaces/IImportExportSchemaService.cs
namespace Platform.Modules.ImportExport.Interfaces;

public interface IImportExportSchemaService
{
    IReadOnlyList<ImportExportEntitySummary> ListEntities();
    IReadOnlyList<ImportExportFieldSchema> GetFields(string key);
}
```

```csharp
// Exceptions/ImportExportEntityNotFoundException.cs
public sealed class ImportExportEntityNotFoundException : ExcelEngineException
{
    public ImportExportEntityNotFoundException(string key)
        : base(ErrorCodes.ImportExportEntityNotFound, EngineMessages.ImportExportEntityNotFound(key)) { }
}
```

- [ ] **Step 1:** Add `ErrorCodes.ImportExportEntityNotFound` and `EngineMessages.ImportExportEntityNotFound(key)`.
- [ ] **Step 2:** Create files above.
- [ ] **Step 3:** Build Contracts project.

```bash
dotnet build modules/importexport/Platform.Modules.ImportExport/Platform.Modules.ImportExport.csproj
```

Expected: Build succeeded.

- [ ] **Step 4:** Commit

```bash
git add modules/importexport/Platform.Modules.ImportExport/
git commit -m "feat(importexport): add entity registry contracts and schema DTOs"
```

---

## Task 2: Registry options + implementation

**Files:**
- Create: `Platform.Modules.ImportExport.Application/Options/ImportExportEntityRegistryOptions.cs`
- Create: `Platform.Modules.ImportExport.Application/Services/ImportExportEntityRegistry.cs`

**Interfaces:**
- Consumes: `ImportExportEntitySummary`, `RegisteredImportExportEntity` from Task 1
- Produces: `IImportExportEntityRegistry` singleton built from `IOptions<ImportExportEntityRegistryOptions>`

- [ ] **Step 1:** Write failing test `Registry_List_ReturnsRegisteredEntities` in Task 6 file (stub registry inline first).

- [ ] **Step 2:** Implement options builder:

```csharp
// Options/ImportExportEntityRegistryOptions.cs
namespace Platform.Modules.ImportExport.Application.Options;

public sealed class ImportExportEntityRegistryOptions
{
    internal List<RegisteredImportExportEntity> Entities { get; } = [];

    public ImportExportEntityRegistryOptions Add<TRow>(string key, string displayName)
        where TRow : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (Entities.Any(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Duplicate import-export entity key '{key}'.");
        Entities.Add(new RegisteredImportExportEntity(key.Trim(), typeof(TRow), displayName.Trim()));
        return this;
    }
}
```

- [ ] **Step 3:** Implement registry:

```csharp
// Services/ImportExportEntityRegistry.cs
public sealed class ImportExportEntityRegistry : IImportExportEntityRegistry
{
    private readonly Dictionary<string, RegisteredImportExportEntity> _map;

    public ImportExportEntityRegistry(IOptions<ImportExportEntityRegistryOptions> options)
    {
        _map = options.Value.Entities
            .ToDictionary(e => e.Key, e => e, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ImportExportEntitySummary> List() =>
        _map.Values
            .OrderBy(e => e.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(e => new ImportExportEntitySummary(e.Key, e.DisplayName))
            .ToList();

    public bool TryGet(string key, out RegisteredImportExportEntity entity) =>
        _map.TryGetValue(key.Trim(), out entity!);
}
```

- [ ] **Step 4:** Build Application project — Expected: succeeded.

- [ ] **Step 5:** Commit — `feat(importexport): add in-memory entity registry`

---

## Task 3: ReflectionFieldReader + ImportExportSchemaService

**Files:**
- Create: `Platform.Modules.ImportExport.Application/Schemas/ReflectionFieldReader.cs`
- Create: `Platform.Modules.ImportExport.Application/Services/ImportExportSchemaService.cs`

**Interfaces:**
- Consumes: `IImportExportEntityRegistry`, `ImportExportFieldSchema`
- Produces: `IImportExportSchemaService.GetFields(key)` throws `ImportExportEntityNotFoundException` when missing

- [ ] **Step 1:** Implement `ReflectionFieldReader`:

```csharp
internal static class ReflectionFieldReader
{
    private static readonly HashSet<string> ExcludedNames =
        new(StringComparer.OrdinalIgnoreCase) { "Id", "TenantId" };

    public static IReadOnlyList<ImportExportFieldSchema> Read(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Where(p => !ExcludedNames.Contains(p.Name))
            .Where(p => IsSupportedScalar(p.PropertyType))
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(MapProperty)
            .ToList();
    }

    private static bool IsSupportedScalar(Type t)
    {
        var u = Nullable.GetUnderlyingType(t) ?? t;
        return u.IsEnum || u == typeof(string) || u == typeof(bool)
            || u == typeof(int) || u == typeof(long) || u == typeof(decimal)
            || u == typeof(double) || u == typeof(float)
            || u == typeof(DateTime) || u == typeof(DateTimeOffset)
            || u == typeof(Guid);
    }

    private static ImportExportFieldSchema MapProperty(PropertyInfo p)
    {
        var required = p.GetCustomAttribute<RequiredAttribute>() != null;
        var maxLength = p.GetCustomAttribute<MaxLengthAttribute>()?.Length;
        var range = p.GetCustomAttribute<RangeAttribute>();
        decimal? min = range != null && decimal.TryParse(range.Minimum.ToString(), out var rmin) ? rmin : null;
        decimal? max = range != null && decimal.TryParse(range.Maximum.ToString(), out var rmax) ? rmax : null;
        var regex = p.GetCustomAttribute<RegularExpressionAttribute>()?.Pattern;

        return new ImportExportFieldSchema(
            p.Name, p.Name, MapDataType(p.PropertyType),
            required, maxLength, min, max, regex);
    }

    private static string MapDataType(Type t)
    {
        var u = Nullable.GetUnderlyingType(t) ?? t;
        if (u == typeof(string)) return "string";
        if (u == typeof(bool)) return "boolean";
        if (u == typeof(DateTime) || u == typeof(DateTimeOffset)) return "datetime";
        if (u == typeof(Guid)) return "guid";
        if (u.IsEnum) return "string";
        return "number";
    }
}
```

- [ ] **Step 2:** Implement schema service:

```csharp
public sealed class ImportExportSchemaService(IImportExportEntityRegistry registry)
    : IImportExportSchemaService
{
    public IReadOnlyList<ImportExportEntitySummary> ListEntities() => registry.List();

    public IReadOnlyList<ImportExportFieldSchema> GetFields(string key)
    {
        if (!registry.TryGet(key, out var entity))
            throw new ImportExportEntityNotFoundException(key);
        return ReflectionFieldReader.Read(entity.RowType);
    }
}
```

- [ ] **Step 3:** Register DI in `PlatformImportExportApplicationExtensions.cs`:

```csharp
services.AddOptions<ImportExportEntityRegistryOptions>();
services.TryAddSingleton<IImportExportEntityRegistry, ImportExportEntityRegistry>();
services.TryAddScoped<IImportExportSchemaService, ImportExportSchemaService>();
```

- [ ] **Step 4:** Commit — `feat(importexport): add schema service with reflection field reader`

---

## Task 4: DI extension for consumers

**Files:**
- Create: `Platform.Modules.ImportExport.Application/DependencyInjection/ImportExportEntityRegistryExtensions.cs`

- [ ] **Step 1:** Add extension method:

```csharp
public static class ImportExportEntityRegistryExtensions
{
    public static IServiceCollection ConfigureImportExportEntities(
        this IServiceCollection services,
        Action<ImportExportEntityRegistryOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
}
```

- [ ] **Step 2:** Document usage in XML comment (Sample example).

- [ ] **Step 3:** Commit — `feat(importexport): add ConfigureImportExportEntities extension`

---

## Task 5: API controllers

**Files:**
- Create: `Platform.Modules.ImportExport.Api/Controllers/ImportExportEntitiesController.cs`
- Create: `Sample/ImportExport/Controllers/ImportExportEntitiesController.cs`

**Endpoints:**

```http
GET /api/v1/importexport/entities
GET /api/v1/importexport/entities/{key}/fields
```

- [ ] **Step 1:** Module API controller (follow `TemplatesController` / `BaseApiController` pattern):

```csharp
[ApiController]
[Route("api/v1/importexport/entities")]
public sealed class ImportExportEntitiesController(IImportExportSchemaService schemaService)
    : BaseApiController
{
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<ImportExportEntitySummary>>> List()
        => Ok(schemaService.ListEntities());

    [HttpGet("{key}/fields")]
    public ActionResult<ApiResponse<IReadOnlyList<ImportExportFieldSchema>>> Fields(string key)
        => Ok(schemaService.GetFields(key));
}
```

- [ ] **Step 2:** Mirror controller in Sample (same routes, inject `IImportExportSchemaService`).

- [ ] **Step 3:** Manual smoke — start Sample, curl:

```bash
curl -s http://localhost:PORT/api/v1/importexport/entities
curl -s http://localhost:PORT/api/v1/importexport/entities/Student/fields
```

Expected: Student entity + fields `Name`, `Age`.

- [ ] **Step 4:** Commit — `feat(importexport): add entities schema API endpoints`

---

## Task 6: Unit tests

**Files:**
- Create: `Platform.Modules.ImportExport.Tests/Schemas/ImportExportSchemaServiceTests.cs`

- [ ] **Step 1:** Add test DTO inside test file:

```csharp
private sealed class TestImportRow
{
    [Required][MaxLength(100)] public string Name { get; set; } = "";
    [Range(1, 99)] public int Age { get; set; }
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
```

- [ ] **Step 2:** Tests:

```csharp
[Fact]
public void ListEntities_ReturnsRegisteredSummary()
{
    var registry = CreateRegistry(o => o.Add<TestImportRow>("Test", "Test Entity"));
    var sut = new ImportExportSchemaService(registry);
    var list = sut.ListEntities();
    Assert.Single(list);
    Assert.Equal("Test", list[0].Key);
}

[Fact]
public void GetFields_ExcludesIdAndTenantId()
{
    var sut = CreateSut();
    var fields = sut.GetFields("Test");
    Assert.DoesNotContain(fields, f => f.Name is "Id" or "TenantId");
    Assert.Contains(fields, f => f.Name == "Name" && f.Required && f.MaxLength == 100);
}

[Fact]
public void GetFields_UnknownKey_ThrowsNotFound()
{
    var sut = CreateSut();
    Assert.Throws<ImportExportEntityNotFoundException>(() => sut.GetFields("Missing"));
}
```

- [ ] **Step 3:** Run tests:

```bash
dotnet test modules/importexport/Platform.Modules.ImportExport.Tests/Platform.Modules.ImportExport.Tests.csproj --filter "FullyQualifiedName~ImportExportSchemaServiceTests"
```

Expected: All passed.

- [ ] **Step 4:** Commit — `test(importexport): add schema service unit tests`

---

## Task 7: Sample — Settings group + StudentImportRow + registration

**Files:**
- Create: `Sample/Settings/ImportExportEntitiesSettingDefinition.cs`
- Create: `Sample/ImportExport/Models/StudentImportRow.cs`
- Modify: `Sample/ImportExport/SampleImportExportExtensions.cs`
- Modify: `Sample/Extensions/SampleSettingExtensions.cs`

- [ ] **Step 1:** `StudentImportRow`:

```csharp
namespace Sample.ImportExport.Models;

public sealed class StudentImportRow
{
    [Required][MaxLength(200)]
    public string Name { get; set; } = "";

    [Range(1, 150)]
    public int Age { get; set; }
}
```

- [ ] **Step 2:** Settings definition (catalog read-only, key = entity key):

```csharp
public sealed class ImportExportEntitiesSettingDefinition : ISettingDefinitionProvider
{
    public const string GroupName = "ImportExportEntities";
    public const string StudentKey = "ImportExportEntities.Student";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(GroupName, g =>
        {
            g.DisplayName = "Import / Export — Đối tượng";
            g.Description = "Catalog đối tượng dùng khi tạo template Excel (key = mã entity).";
            g.Order = 6;
        });

        context.AddSetting(GroupName, StudentKey, s =>
        {
            s.Name = "Học sinh";
            s.Description = "Entity key: Student — dùng khi tạo template import/export học sinh.";
            s.Type = SettingValueTypes.Text;
            s.DefaultValue = "Student";
            s.IsReadOnly = true;
        });
    }
}
```

- [ ] **Step 3:** Register in Sample:

```csharp
// SampleImportExportExtensions.cs
builder.Services.ConfigureImportExportEntities(o =>
    o.Add<StudentImportRow>("Student", "Học sinh"));

// SampleSettingExtensions.cs
.AddProvider<ImportExportEntitiesSettingDefinition>();
```

- [ ] **Step 4:** (Optional) Startup validator — warn if Setting default key not in registry.

- [ ] **Step 5:** Commit — `feat(sample): register Student import row and ImportExportEntities settings`

---

## Task 8: Frontend — API client + TemplateFormPage

**Files:**
- Modify: `frontend/src/lib/types/index.ts`
- Modify: `frontend/src/lib/services/index.ts`
- Modify: `frontend/src/lib/index.ts`
- Modify: `frontend/src/lib/pages/TemplateFormPage.tsx`

- [ ] **Step 1:** Add types:

```typescript
export type ImportExportEntitySummary = { key: string; displayName: string }
export type ImportExportFieldSchema = {
  name: string
  displayName: string
  dataType: string
  required: boolean
  maxLength?: number | null
  min?: number | null
  max?: number | null
  regex?: string | null
}
```

- [ ] **Step 2:** Add services:

```typescript
export async function listImportExportEntities() {
  const { data } = await http.get('/api/v1/importexport/entities')
  return unwrapArray<ImportExportEntitySummary>(data, 'Không tải được danh sách đối tượng')
}

export async function getImportExportEntityFields(key: string) {
  const { data } = await http.get(
    `/api/v1/importexport/entities/${encodeURIComponent(key)}/fields`,
  )
  return unwrapArray<ImportExportFieldSchema>(data, 'Không tải được fields')
}
```

- [ ] **Step 3:** Update `TemplateFormPage.tsx`:
  - `useEffect` load entities on mount
  - `<select>` chọn entity (optional, placeholder "— Chọn đối tượng —")
  - On change: confirm nếu mappings đã có data → `getImportExportEntityFields(key)` → map:

```typescript
fields.map((f) => ({
  columnName: '',
  fieldName: f.name,
  required: f.required,
  unique: false,
  regex: f.regex ?? null,
  maxLength: f.maxLength ?? null,
  min: f.min ?? null,
  max: f.max ?? null,
}))
```

- [ ] **Step 4:** Export new types/functions from `lib/index.ts`.

- [ ] **Step 5:** Manual UI test — tạo template, chọn "Học sinh", verify 2 rows Name/Age.

- [ ] **Step 6:** Commit — `feat(importexport-ui): entity selector auto-fills template mappings`

---

## Task 9: Documentation

**Files:**
- Modify: `modules/importexport/docs/SAD-Extensibility.md`

- [ ] **Step 1:** Add section **Entity schema registry**:

```text
Consumer registers ImportRow DTO:
  services.ConfigureImportExportEntities(o => o.Add<MyImportRow>("MyKey", "Display"));

Template UI:
  GET /api/v1/importexport/entities
  GET /api/v1/importexport/entities/{key}/fields

Settings group ImportExportEntities = catalog documentation (read-only keys).
Runtime import/export template codes remain in group ImportExport.
```

- [ ] **Step 2:** Commit — `docs(importexport): document entity registry extensibility`

---

## Verification checklist (full flow)

- [ ] `dotnet test modules/importexport/Platform.Modules.ImportExport.Tests`
- [ ] Settings admin shows group `ImportExportEntities` with "Học sinh"
- [ ] `GET /api/v1/importexport/entities` → `[{ key: "Student", displayName: "Học sinh" }]`
- [ ] `GET .../Student/fields` → `Name`, `Age` (no Id/TenantId)
- [ ] UI tạo template: chọn entity → mapping auto-fill
- [ ] Lưu template + import Student vẫn hoạt động (`StudentImportPersister`)
- [ ] Group `ImportExport` (template code) không bị ảnh hưởng

---

## Commit strategy (summary)

1. `feat(importexport): add entity registry contracts and schema DTOs`
2. `feat(importexport): add in-memory entity registry`
3. `feat(importexport): add schema service with reflection field reader`
4. `feat(importexport): add ConfigureImportExportEntities extension`
5. `feat(importexport): add entities schema API endpoints`
6. `test(importexport): add schema service unit tests`
7. `feat(sample): register Student import row and ImportExportEntities settings`
8. `feat(importexport-ui): entity selector auto-fills template mappings`
9. `docs(importexport): document entity registry extensibility`

---

## Out of scope (GĐ1.5 / GĐ2)

| Item | Description |
|------|-------------|
| Read Excel headers | Auto-fill `columnName` from uploaded file header row |
| Persist `entityKey` on template | Metadata in `MappingsJson` for re-sync on edit |
| Tenant enable/disable entity | Filter `ListEntities` by Setting value `true/false` |
| `ImportExportEntityAttribute` | Custom display names on fields |

---

## Self-review (spec coverage)

| Requirement | Task |
|-------------|------|
| Registry code-first key → Type | Task 2, 4 |
| Reflection for field list only | Task 3 |
| Separate Settings group | Task 7 |
| API for UI | Task 5 |
| Sample Student demo | Task 7 |
| Frontend auto-fill | Task 8 |
| Unit tests | Task 6 |
| No import pipeline change | Global constraint — no task modifies processors |
| Docs | Task 9 |

No placeholder steps. Types consistent: `ImportExportEntitySummary`, `ImportExportFieldSchema`, `IImportExportSchemaService` used throughout.

---

## Execution handoff

**Plan saved to:** `modules/importexport/docs/plans/2026-08-10-importexport-entity-registry.md`

**Two execution options:**

1. **Subagent-Driven (recommended)** — dispatch fresh subagent per task, review between tasks
2. **Inline Execution** — implement tasks sequentially in one session with checkpoints

**Which approach?**
