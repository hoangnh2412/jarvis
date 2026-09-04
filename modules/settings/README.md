# Jarvis.Modules.Setting

Module cấu hình dùng chung (**SettingManagement**): quản lý cấu hình toàn hệ thống **theo nhóm (Group)**, lưu giá trị theo **1 Key = 1 row**, định nghĩa metadata theo **code-first Library**, đọc/ghi qua `ISettingManager` (không query DB trực tiếp từ UI/API nghiệp vụ).

## Khả năng chính

| Khả năng | Mô tả | API / trạng thái |
|----------|--------|------------------|
| Quản lý theo Group | Cấu hình được tổ chức theo nhóm (vd. Email, Payment); metadata nhóm + danh sách key trong Library | `GetGroups()`, `GetDefinitions(group?)` |
| Giá trị linh hoạt theo Type | Form input đa dạng: text, number, email, textarea, combobox, radio, multi-select, checkbox, password, datetime, date, image… | Xem [Kiểu giá trị](#kiểu-giá-trị-settingvaluetypes) |
| Validate theo Type | Kiểm tra value khi Create/Update/SaveGroup (`SettingValueValidator`) | BadRequest `98107` |
| Lấy theo Key | Đọc một mục cấu hình | `GetAsync(key)` / `GetOrCreateAsync(key)` |
| Lấy theo Group | Đọc toàn bộ giá trị (hoặc form) của một nhóm | `GetByGroupAsync(group)` / `GetFormAsync(group)` |
| Cập nhật theo Key | Sửa một mục cấu hình | `UpdateAsync(key, value)` |
| Cập nhật theo Group | Lưu nhiều key của một nhóm trong một lần (cả form) | `SaveGroupAsync(group, values)` |

## Phạm vi module

Module **chỉ** làm SettingManagement — định nghĩa + lưu/đọc/cập nhật cấu hình. **Không** thêm chức năng thuộc module khác.

### Trong phạm vi

| Chức năng | Mô tả |
|-----------|--------|
| Library (code-first) | Định nghĩa Group/Key, metadata form (Name, Type, Options, Default…) |
| Persist Value | 1 Key = 1 row SQL (`Group`, `Key`, `Value`, `Type`…) |
| Đọc / ghi theo Key | Get / Create / GetOrCreate / Update / Delete |
| Đọc / ghi theo Group | GetByGroup / GetForm / SaveGroup (upsert cả form) |
| Encrypt Value at rest | Mã hóa field nhạy cảm khi ghi DB |
| Cache Value | Cache theo key; invalidate khi ghi |
| Validate Type | Email / Number / Options / Date / Image (≤ 2MB)… |
| `IsReadOnly` | Thuộc tính của setting: không cho sửa/xóa giá trị đó |

```text
  [Authorization]   [Multitenancy]   [Audit]   [UI]
         │                │             │        │
         └────── không nằm trong Jarvis.Modules.Setting ──────┘
                          │
                          ▼
                   ISettingManager
              (định nghĩa + lưu/đọc Value)
                          │
                          ▼
                    SQL bảng Setting
```

**Lưu ý:**

- `TenantId` trên entity chỉ là **shape lưu trữ** (phân tách dữ liệu theo tenant). Module **không** implement multitenancy — đọc working tenant qua `ICurrentTenant<TTenant>.GetIdAsync()` (ambient / R2).
- Host đăng ký `AddCurrentUser` + `AddCurrentTenant` (không còn `IWorkContext`). Auth / audit thuộc host; Setting không inject user.

## Khái niệm

| Khái niệm | Ý nghĩa |
|-----------|---------|
| **Group** | Nhóm logic (vd. `Email`). Metadata (DisplayName, Order…) chỉ nằm trong code/registry — **không lưu DB**. |
| **Key** | Mã cấu hình, **unique trong tenant** (vd. `Email.Host`). |
| **Definition** | Metadata code-first: Name, Type, Options, DefaultValue, IsReadOnly, IsEncrypted. |
| **Entity row** | Giá trị runtime trong DB (`ISettingEntity`). Tạo lúc runtime, không seed hàng loạt. |
| **Library** | Registry + provider định nghĩa Group/Key và xử lý logic Setting. |
| **SettingModel** | DTO CRUD / runtime: row đã persist (`Id`, `TenantId`, Value plaintext). |
| **SettingFormItemModel** | DTO form UI: definition ⊕ value (hoặc default), có `IsPersisted` / `DefaultValue` / `IsEncrypted`. |

```text
  ISettingDefinitionProvider (code)
           ↓ đăng ký
  ISettingDefinitionRegistry (metadata in-memory)
           ↓
  ISettingManager  ←→  cache (Tenant+Key)  ←→  DB (ISettingEntity)
                           ↕
                    encrypt at rest (AES-GCM)
                    validate Type khi ghi
```

## Packages / phụ thuộc

| Project | Vai trò |
|---------|---------|
| `Jarvis.Modules.Setting` | Library, manager, validator, DI lõi (encryptor dùng `Jarvis.Common`) |
| `Jarvis.Modules.Setting.EntityFramework` | Entity mặc định + `UseEntityFramework` + `ConfigureSetting` |
| `Jarvis.Modules.Setting.API` | HTTP API sẵn (`UseHttpApi`) — form/groups/CRUD; opt-in |
| `Jarvis.DDD.Domain` | `ISettingEntity` |
| `Jarvis.Caching` (+ Redis) | Cache theo `Tenant + Key` |
| `modules/settings/frontend` (`@jarvis/setting`) | UI feature (pages/components) cho host SPA |
| Host app (vd. Sample) | Provider nghiệp vụ, DbContext, endpoint demo (vd. test email), wiring DI |

## Cài đặt nhanh

### Cách khuyến nghị (entity mặc định)

```csharp
// 0. Ambient user/tenant (ADR Multitenancy) — trước Setting
builder.AddCurrentUser<CurrentUserInfo>();
builder.AddCurrentTenant<CurrentTenantInfo>();
// + ICurrentUserStore / ICurrentTenantStore của host

// 1. DI — sau AddJarvisCaching().UseRedisDistributedCache()
builder.AddCoreSetting()
    .UseEntityFramework<IMasterUnitOfWork, CurrentTenantInfo>()
    .UseHttpApi() // opt-in: expose api/v{version}/settings (không Authorize — nội bộ)
    .AddProvider<EmailSettingDefinition>();

// 2. DbContext
modelBuilder.ConfigureSetting(); // hoặc ConfigureSetting(o => { o.TableName = "..."; o.Schema = "..."; })

// 3. Cache — khai báo Cache:Items:Setting trong appsettings (Jarvis.Caching)
```

Bỏ `.UseHttpApi()` nếu host chỉ dùng `ISettingManager` nội bộ hoặc tự viết controller.

### Entity tùy chỉnh của host

```csharp
builder.AddCoreSetting<MySetting, IMasterUnitOfWork, CurrentTenantInfo>()
    .AddProvider<EmailSettingDefinition>();
// Host tự map EF cho MySetting (implement ISettingEntity).
```

### Definition provider (code-first)

```csharp
public sealed class EmailSettingDefinition : ISettingDefinitionProvider
{
    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup("Email", g =>
        {
            g.DisplayName = "Email";
            g.Order = 10;
        });

        context.AddSetting("Email", "Email.Host", s =>
        {
            s.Name = "Host";
            s.Type = SettingValueTypes.Text;
        });

        context.AddSetting("Email", "Email.FromAddress", s =>
        {
            s.Name = "From Address";
            s.Type = SettingValueTypes.Email; // rỗng được phép
            s.DefaultValue = string.Empty;
        });

        context.AddSetting("Email", "Email.Password", s =>
        {
            s.Name = "Password";
            s.Type = SettingValueTypes.Password; // mặc định IsEncrypted = true
            s.IsEncrypted = true;
        });
    }
}
```

### appsettings

```json
{
  "Encryption": {
    "DataEncryptionKey": "<Base64 32-byte AES key>"
  },
  "Cache": {
    "Items": {
      "Setting": {
        "Key": "setting:{tenantId}:{key}",
        "MemSeconds": 300,
        "DistributedSeconds": 3600,
        "DistributedGroup": "Default",
        "DistributedType": "Redis"
      }
    }
  }
}
```

- `Encryption:DataEncryptionKey`: Base64 của khóa AES-256 (32 byte) — dùng chung qua `Jarvis.Common` (`AesGcmStringEncryptionHelper`). Bắt buộc khi có setting `IsEncrypted` / `Password`.
- Cache item cố định tên `Setting` — host khai báo `Cache:Items:Setting` với placeholder `{tenantId}` và `{key}`.
- Redis runtime: `AddJarvisCaching().UseRedisDistributedCache()` + `Cache:Items:Setting` trong appsettings.

## API chính — `ISettingManager`

| Method | Mô tả |
|--------|--------|
| `GetGroups()` / `GetDefinitions(group?)` | Metadata từ Library (không đụng DB) |
| `GetAsync(key)` | Đọc value theo Key (cache → DB), decrypt nếu cần → `SettingModel?` |
| `GetByGroupAsync(group)` | Đọc các row đã lưu theo Group (không kèm default) → `SettingModel[]` |
| `GetFormAsync(group)` | Form UI: mọi definition ⊕ values (hoặc default) → `SettingFormItemModel[]` |
| `CreateAsync(key, value?)` | Tạo row từ definition (runtime); validate Type |
| `GetOrCreateAsync(key)` | Có thì lấy, chưa có thì tạo từ definition |
| `UpdateAsync(key, value)` | Cập nhật theo Key; chặn `IsReadOnly`; validate Type; invalidate cache |
| `SaveGroupAsync(group, values)` | Upsert nhiều key trong 1 lần; chặn `IsReadOnly` (definition/entity); secret rỗng → `98107` |
| `DeleteAsync(key)` | Xóa; chặn `IsReadOnly` (definition/entity); invalidate cache |

### `SettingModel` vs `SettingFormItemModel`

| | `SettingModel` | `SettingFormItemModel` |
|---|---|---|
| Mục đích | CRUD / runtime (đã persist) | Dựng form UI một lần gọi |
| Identity | Có `Id`, `TenantId` | Không |
| Form-only | — | `DefaultValue`, `IsEncrypted`, `IsPersisted` |
| Khi chưa có row | `GetAsync` → `null` | Vẫn trả item với `Value = DefaultValue`, `IsPersisted = false` |

Giá trị trả về luôn là **plaintext**. DB và cache lưu ciphertext cho setting nhạy cảm.

## HTTP API — endpoint cho Frontend

Package `Jarvis.Modules.Setting.API` cung cấp controller sẵn (opt-in qua `UseHttpApi`).
Core `Jarvis.Modules.Setting` **không** kèm HTTP — host tự chọn expose API hoặc chỉ dùng `ISettingManager`.

```csharp
builder.AddCoreSetting()
    .UseEntityFramework<IMasterUnitOfWork, CurrentTenantInfo>()
    .UseHttpApi();
```

Host vẫn thêm endpoint riêng trên cùng prefix nếu cần (Sample: `POST .../settings/email/test`).

Tài liệu request/response/envelope: [`Jarvis.Modules.Setting/doc/openapi.md`](./Jarvis.Modules.Setting/doc/openapi.md).

Base route: `api/v{version}/settings` (ví dụ `api/v1/settings`). Không gắn `[Authorize]` — module jarvis không enforce auth; chưa đăng nhập vẫn gọi API bình thường nếu host không tự bảo vệ. `GET /form` và `GET /` bắt buộc có query `group`.

### Host gắn Auth (mẫu)

Module **không** thêm Auth. Host muốn bảo vệ endpoint thì dùng convention + policy — Sample có file mẫu `Sample/Settings/SettingHttpApiAuthorizationSample.cs`:

```csharp
builder.AddSampleSettings(); // hoặc AddCoreSetting().UseHttpApi()…

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SettingHttpApiAuthorizationSample.SettingAdminPolicy, policy =>
        policy.RequireAuthenticatedUser()); // hoặc RequireRole("Admin")
});
SettingHttpApiAuthorizationSample.ApplyAuthorizeToSettingHttpApi(builder.Services);

// Pipeline:
app.UseAuthentication();
app.UseAuthorization(); // bắt buộc khi đã gắn AuthorizeFilter
app.MapControllers();
```

### Kết luận — API sẽ sử dụng

| Ưu tiên | Method | Endpoint | Mục đích |
|---------|--------|----------|----------|
| ★ Chính | `GET` | `/form?group={group}` | Dựng form: definitions ⊕ values (hoặc default) — **1 call** |
| ★ Chính | `PUT` | `/group/{group}` | Lưu cả form (upsert nhiều key) |
| ★ Chính | `GET` | `/groups` | Menu / tab danh sách nhóm cấu hình |
| Phụ | `GET` | `/definitions?group={group}` | Chỉ metadata Library (không Value) |
| Phụ | `GET` | `?group={group}` | Chỉ Value đã lưu DB (có thể rỗng) |
| Phụ | `GET` | `/{key}` | Đọc 1 setting theo Key |
| Phụ | `POST` | `/{key}` | Tạo 1 row theo Key |
| Phụ | `PUT` | `/{key}` | Cập nhật 1 Value theo Key |
| Phụ | `DELETE` | `/{key}` | Xóa 1 setting theo Key |

**API nội bộ** (không bắt buộc expose HTTP): `GetOrCreateAsync(key)` — service nghiệp vụ đọc cấu hình runtime (vd. SMTP).

Authorization, multitenancy, audit — do host xử lý **trước** khi gọi manager / API.

### Luồng FE khuyến nghị

```text
1. GET /groups
   → vẽ menu / tab nhóm
2. GET /form?group=Email
   → mỗi item có: key, name, type, options, value, isReadOnly…
3. Render form theo type / options (@jarvis/setting)
4. User sửa → PUT /group/Email
   Body: { "values": { "Email.Host": "...", "Email.Port": "587", ... } }
```

Host SPA (Sample):

```tsx
import { SettingPage, TestEmailPage } from '@jarvis/setting'
```

### Options theo Type

Hai ngữ nghĩa (theo `Type`, **không trộn** trong cùng một Key):

| Nhóm | Type | Options là gì | Format |
|------|------|---------------|--------|
| **Choice** | Combobox, Radio, MultiSelect | Các giá trị trong select | `value:label\|value:label\|...` |
| **Constraint** | Text, Textarea, Number, Email, Image | Điều kiện / tham số của giá trị | `key:value\|key:value\|...` |

#### Constraint — helper `SettingTypeOptions` / FE `parseTypeOptions`

| Type | Keys | Ví dụ |
|------|------|--------|
| Email | `regex` | `ForEmail()` → lưu `regex:default` (ngắn); resolve → pattern trong code. `ForEmail(@"^…$")` → pattern riêng |
| Number | `decimals` | `ForNumber(2)` |
| Text | `maxLength` | `ForText(50)` |
| Textarea | `rows` | `ForTextarea(8)` |
| Image | `maxBytes`, `mimeTypes` | `ForImage()` |

- Combobox/Radio/MultiSelect: FE parse choice list; MultiSelect lưu value `a,b,c`.
- Number: value lưu **invariant** (`1234567.89`); UI hiển thị `1.234.567,89`.
- Email: luôn đọc điều kiện từ `Options.regex` (`default` hoặc pattern); không nhúng cả default regex dài xuống DB.
- Image mặc định: ≤ 2MB, MIME `png/jpeg/gif/webp`.

File tham chiếu: `Jarvis.Modules.Setting.API/Controllers/SettingController.cs`, `Sample/Controllers/SettingEmailController.cs`, `Sample/Settings/DemoSettingDefinition.cs`.

### Ví dụ dùng

```csharp
public class SmtpOptionsLoader(ISettingManager settings)
{
    public async Task<string> GetHostAsync(CancellationToken ct)
    {
        var item = await settings.GetOrCreateAsync("Email.Host", ct);
        return item.Value;
    }
}
```

## Quy tắc thiết kế đã chốt

1. **Chỉ SettingManagement** — không audit, không phân quyền, không implement multitenancy trong module này.
2. **Code-first**: định nghĩa Group/Key trong Library; team nghiệp vụ đăng ký provider riêng.
3. **Group metadata không lưu DB** — chỉ registry in-memory.
4. **Key unique theo tenant**; `Id` dùng UUID v7 (`Guid.CreateVersion7()`).
5. **Encrypt khi ghi DB** (AES-GCM, prefix `enc.v1.`). Manager trả plaintext cho caller nội bộ.
6. **Cache**: theo `Tenant + Key`; invalidate khi create/update/delete.
7. **Tạo runtime** — không seed sẵn toàn bộ cấu hình.
8. **`IsReadOnly`**: thuộc tính của setting — không cho update/delete/save-group giá trị đó (cả definition lẫn snapshot entity).
9. **Validate Type** khi ghi; Email/Image/Number/Textarea đọc ràng buộc từ `Options` (regex, maxBytes/MIME, decimals, rows).
10. **Password/encrypted**: chuỗi rỗng/whitespace **không** được persist và **không** mã hóa (`98107`). Client bỏ key khỏi payload nếu không đổi secret. Giá trị non-empty được mã hóa at-rest; API trả plaintext.

## Kiểu giá trị (`SettingValueTypes`)

Giá trị cấu hình linh hoạt theo `Type` — UI render control tương ứng. Value luôn lưu dạng **string** trên DB.

| Const | Gợi ý UI | Format Value | Validate |
|-------|----------|--------------|----------|
| `Text` | Ô text một dòng | string tự do | — |
| `Textarea` | Đa dòng (`Options.rows`) | string tự do | — |
| `Number` | Ô số + ngăn hàng nghìn | số invariant | Bắt buộc; `Options.decimals` giới hạn phần thập phân |
| `Email` | Ô email | địa chỉ email | Rỗng OK; điều kiện từ `Options.regex` (`default` → pattern trong code) |
| `Combobox` | Chọn **một** từ `Options` | một `value` | Phải thuộc Options |
| `Radio` | Radio từ `Options` | một `value` | Phải thuộc Options |
| `MultiSelect` | Chọn **nhiều** từ `Options` | `a,b,c` | Từng phần thuộc Options |
| `Checkbox` | Ô tick bật/tắt | `true` / `false` | Chỉ hai giá trị đó |
| `Switch` | Toggle switch bật/tắt | `true` / `false` | Chỉ hai giá trị đó |
| `Password` | Nhạy cảm → mặc định encrypt | string non-empty | Rỗng → `98107` (không lưu); FE bỏ key khi không đổi |
| `Date` | Chỉ ngày | `yyyy-MM-dd` | Format + ngày hợp lệ |
| `DateTime` | Ngày giờ | `yyyy-MM-ddTHH:mm` hoặc `…:ss` | Format hợp lệ |
| `Image` | Upload ảnh (data URL) | `data:image/…;base64,…` | `Options.maxBytes` + `Options.mimeTypes` |

Backend: `Validation/SettingValueValidator.cs` + `SettingTypeOptions.cs`. Frontend: `validation` + `utils/typeOptions.ts` / `imageConstraints.ts` / `numberFormat.ts`.

## Bảo mật & cache

- Encryptor dùng chung: `AesGcmStringEncryptionHelper` trong `Jarvis.Common` (static helper, không DI).
- Payload: `enc.v1.{base64(nonce|tag|ciphertext)}`.
- Đọc: nếu definition encrypt / type Password / value có prefix → decrypt.
- Cache lưu giá trị persisted (ciphertext với secret); chỉ decrypt sau cache hit trước khi trả caller.
- Cache miss không ghi null vào cache (`GetOrSetAsync` bỏ qua null).
- Image lớn: cân nhắc không dùng data URL trên production (object storage); validator chỉ là lớp bảo vệ kích thước.

## Mã lỗi (`SettingErrorCode`)

| Code | Ý nghĩa |
|------|---------|
| `98101` | Không tìm thấy setting |
| `98102` | Definition chưa đăng ký trong Library |
| `98103` | Key đã tồn tại |
| `98104` | Read-only |
| `98105` | Thiếu tenant context |
| `98106` | Encryption key thiếu/sai |
| `98107` | Giá trị không hợp lệ theo Type (`InvalidValue`) |

## Phân loại file theo chức năng

### 1. Hợp đồng Domain (persist shape)

| File | Công việc |
|------|-----------|
| `Jarvis.DDD.Domain/Entities/ISettingEntity.cs` | Contract tenant-scoped: `Group`, `Key`, `Name`, `Value`, `Type`, `Options`, `Description`, `IsReadOnly`. |

### 2. Package lõi `Jarvis.Modules.Setting`

| File | Công việc |
|------|-----------|
| `SettingValueTypes.cs` | Hằng số Type (Text…Image). |
| `SettingErrorCode.cs` | Catalog lỗi `98101`–`98107`. |
| `Definitions/*` | Library code-first (provider / registry / models). |
| `Validation/SettingValueValidator.cs` | Validate value theo Type khi ghi. |
| `Services/ISettingManager.cs` + `SettingManager.cs` | Facade CRUD + cache + encrypt + validate. |
| `Models/SettingModel.cs` | DTO runtime / CRUD (có Id, TenantId). |
| `Models/SettingFormItemModel.cs` | DTO form UI (`GetFormAsync`). |
| `Extensions/JarvisSettingExtensions.cs` | `AddCoreSetting()` / generic overload + `AddProvider`. |

### 3. `Jarvis.Modules.Setting.EntityFramework`

| File | Công việc |
|------|-----------|
| `Entities/Setting.cs` | Entity mặc định implement `ISettingEntity`. |
| `EntityConfigurations/SettingEntityConfiguration.cs` | Map bảng, unique `(TenantId, Key)`. |
| `Extensions/SettingEntityFrameworkExtensions.cs` | `UseEntityFramework<TUnitOfWork, TTenant>()`. |
| `Extensions/SettingModelBuilderExtensions.cs` | `ConfigureSetting()` trên `ModelBuilder`. |
| `Configuration/SettingModelBuilderConfigurationOptions.cs` | Ghi đè TableName / Schema. |

### 4. `Jarvis.Modules.Setting.API`

| File | Công việc |
|------|-----------|
| `Controllers/SettingController.cs` | HTTP API chuẩn: groups / form / group save / CRUD key. |
| `Extensions/SettingApiExtensions.cs` | `UseHttpApi()` — ApplicationPart. |
| `Models/SettingHttpApiRequests.cs` | DTO request HTTP. |

### 5. Frontend `@jarvis/setting`

| Path | Công việc |
|------|-----------|
| `modules/settings/frontend/src/features/settings/` | Pages, components (DatePicker, Image, Select), validation, API client. |
| Host `Sample/clients/web` | Import package, route `/settings`, build vào `wwwroot`. |

### 6. Host Sample

| File | Công việc |
|------|-----------|
| `Sample/Extensions/SampleSettingExtensions.cs` | `AddCoreSetting().UseEntityFramework&lt;…, CurrentTenantInfo&gt;().UseHttpApi().AddProvider…` |
| `Sample/Persistence/MasterDbContext.cs` | `DbSet<Setting>` + `ConfigureSetting()`. |
| `Sample/Settings/*SettingDefinition.cs` | Provider nghiệp vụ (Email, Demo, Localization…). |
| `Sample/Controllers/SettingEmailController.cs` | Endpoint demo test email (ngoài package API). |
| `Sample/appsettings.json` | Section `Setting` + `Cache:Items:Setting`. |

### 7. Tài liệu

| File | Công việc |
|------|-----------|
| [`README.md`](./README.md) | Hướng dẫn tích hợp (file này). |
| [`Jarvis.Modules.Setting/doc/SAD.md`](./Jarvis.Modules.Setting/doc/SAD.md) | Software Architecture Document. |
| [`Jarvis.Modules.Setting/doc/2026-07-30-adr-setting.md`](./Jarvis.Modules.Setting/doc/2026-07-30-adr-setting.md) | ADR. |
| [`Jarvis.Modules.Setting/doc/openapi.md`](./Jarvis.Modules.Setting/doc/openapi.md) | HTTP API cho Frontend. |
| [`frontend/README.md`](./frontend/README.md) | Package UI `@jarvis/setting`. |

### Sơ đồ thư mục

```text
modules/settings/
  README.md
  Jarvis.Modules.Setting/
    Definitions/
    Validation/
    Extensions/
    Models/
    Services/
    doc/
    SettingValueTypes.cs
    SettingErrorCode.cs
  Jarvis.Modules.Setting.EntityFramework/
    Configuration/
    Entities/
    EntityConfigurations/
    Extensions/
  Jarvis.Modules.Setting.API/
    Controllers/
    Extensions/
    Models/
  frontend/                    # @jarvis/setting
    src/features/settings/
```

## Kế hoạch phát triển

Chỉ các hạng mục thuộc **SettingManagement**. Audit / phân quyền / multitenancy **không** nằm trong kế hoạch module lõi (UI nằm ở package frontend riêng).

Trạng thái: ✅ xong · 📋 kế hoạch · ⏸️ backlog

### Phase 0 — Nền tảng (đã hoàn thành)

| | Hạng mục | Ghi chú |
|---|----------|---------|
| ✅ | `ISettingEntity` + package `Jarvis.Modules.Setting` | |
| ✅ | Code-first Library | Provider / Registry |
| ✅ | `ISettingManager` | Key + Group |
| ✅ | Encrypt at rest | AES-GCM |
| ✅ | Cache theo key | `Jarvis.Caching` |
| ✅ | Package EF / Redis / API | EF + API dùng ở Sample; Redis package giữ lại nhưng chưa dùng |
| ✅ | Type đầy đủ | Text…Image (kèm UI `@jarvis/setting`) |
| ✅ | Validate Type / Options / Image ≤ 2MB | `SettingValueValidator` + FE |
| ✅ | Sample + HTTP API + docs | API package + Sample demo email |

### Phase 1 — Củng cố (ưu tiên gần)

| | Hạng mục | Kết quả mong đợi |
|---|----------|------------------|
| 📋 | Unit test | Encrypt, cache invalidate, IsReadOnly, validate Type, Image limit |
| 📋 | Bulk GetOrCreate theo Group | Giảm round-trip runtime |
| 📋 | Rotation DataEncryptionKey | Re-encrypt / multi-key |
| 📋 | Image production path | Object storage thay data URL (nếu product cần) |

### Phase 2 — Backlog

| | Hạng mục | Kết quả mong đợi |
|---|----------|------------------|
| ⏸️ | Import/Export cấu hình | JSON theo group |
| ⏸️ | Event khi Setting đổi | Consumer tự xử lý |
| ⏸️ | Dynamic Group runtime | Chỉ nếu vẫn thuộc SettingManagement |
| ⏸️ | Skill scaffold | Agent workflow init/add |

### Thứ tự đề xuất

```text
Phase 0 ✅
   → Phase 1 (test + bulk + encrypt ops + image storage)
   → Phase 2 (khi product cần thêm thao tác trên cấu hình)
```
