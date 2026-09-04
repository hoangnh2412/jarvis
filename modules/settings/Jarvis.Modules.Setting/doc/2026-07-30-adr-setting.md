# ADR — SettingManagement (Jarvis.Modules.Setting)

> **Trạng thái:** Phase 0 đã triển khai trong source hiện tại; kiến trúc cốt lõi đã chốt. Cache secret đã chuyển sang lưu ciphertext. Cần hoàn thiện authorization, kiểm thử, validation và rotation key trước khi xem là production-ready.
> **Phạm vi:** `Jarvis.Modules.Setting`, contract `Jarvis.DDD.Domain/Entities/ISettingEntity.cs` và phần tích hợp trong `Sample` — Library code-first, `ISettingManager`, persistence theo tenant, cache, mã hóa và API mẫu.
> **Ngoài phạm vi:** authorization, xác định tenant, audit log, UI quản trị và migration của host.
> **Liên quan:** [README.md](./README.md), [SAD.md](./SAD.md), [openapi.md](./openapi.md).

---

## 1. Mục tiêu nghiệp vụ

| # | Yêu cầu | Trạng thái |
|---|---------|------------|
| 1 | Quản lý danh mục cấu hình theo Group/Key, metadata được version cùng source code | **Đạt** — `ISettingDefinitionProvider` + `SettingDefinitionRegistry` |
| 2 | Mỗi tenant lưu một giá trị cho mỗi Key, không trùng Key trong tenant | **Đạt** — 1 Key = 1 row; host tạo unique index `(TenantId, Key)` |
| 3 | UI lấy được metadata, giá trị hiện tại hoặc default để dựng form | **Đạt** — `GetGroups`, `GetDefinitions`, `GetFormAsync` |
| 4 | Service nghiệp vụ đọc/ghi qua một facade, không query bảng Setting trực tiếp | **Đạt** — `ISettingManager` |
| 5 | Giá trị nhạy cảm được mã hóa khi lưu DB | **Đạt có điều kiện** — AES-GCM; host phải cấu hình key hợp lệ |
| 6 | Tăng tốc đọc lặp lại và làm mới cache sau khi ghi | **Đạt** — cache theo `TenantId + Key`; secret được cache dưới dạng ciphertext |
| 7 | Không persist secret rỗng (không mã hóa, không lưu) | **Đạt** — rỗng/whitespace → `98107`; client bỏ key khỏi payload nếu không đổi |
| 8 | `GetOrCreateAsync` hội tụ khi nhiều request cùng tạo một Key | **Đạt có điều kiện** — dựa vào unique index của host |
| 9 | Kiểm tra Value theo Type/Options trước khi persist | **Chưa đạt** — Phase 1 |
| 10 | Authorization, tenant resolution và audit do host kiểm soát | **Đúng ranh giới** — không thuộc package |

---

## 2. Critical Issues

### Plaintext secret trong distributed cache — đã xử lý

**Trước đây:** `GetAsync` chuyển entity sang `SettingModel` bằng `ToModel` trước khi cache; `ToModel` giải mã secret nên Redis có thể chứa plaintext.

**Hiện tại:** cache lưu `Value` đúng như persist (ciphertext cho secret); decrypt chỉ sau cache hit khi trả caller. Cache key dùng suffix `:persisted` để tránh đụng entry plaintext cũ.

**Vận hành:** entry Redis cũ có thể còn đến khi TTL hết — nên flush `setting:*` sau khi deploy bản sửa nếu môi trường đã chạy trước đó.

---

## 3. Suggestions

### Validation `Type` và `Options`

**Issue:** `Number`, `Combobox` và các type tương lai hiện chỉ là metadata cho UI; manager vẫn nhận và persist mọi chuỗi.

**Impact:** Dữ liệu sai chỉ được phát hiện ở consumer, ví dụ SMTP port phải tự `TryParse`.

**Suggested fix (Phase 1):** Thêm validator theo `SettingValueTypes`; kiểm tra trong `CreateAsync`, `UpdateAsync` và `SaveGroupAsync`, trả mã lỗi thống nhất.

---

### Snapshot metadata có thể lệch definition

**Issue:** Khi tạo row, `Name`, `Type`, `Options`, `Description`, `IsReadOnly` được copy từ definition. Sau deploy, thay đổi definition không cập nhật row cũ. `GetFormAsync` ưu tiên metadata Library, còn `GetByGroupAsync` trả snapshot từ DB.

**Impact:** Hai API có thể trả metadata khác nhau cho cùng một Key.

**Suggested fix:** Chốt một source of truth cho metadata cấp Key:

- Ưu tiên Library khi trả DTO và chỉ lưu tối thiểu `Group`, `Key`, `Value`; hoặc
- Giữ snapshot nhưng bổ sung version/migration/sync metadata.

Trong kiến trúc hiện tại, **Library nên là source of truth khi dựng form**; snapshot DB chỉ phục vụ tương thích và truy vấn độc lập.

---

### Encryption key chưa hỗ trợ rotation

**Issue:** Payload có prefix version `enc.v1.` nhưng encryptor chỉ đọc một `DataEncryptionKey`.

**Impact:** Thay key trực tiếp làm các secret cũ không thể giải mã; mất key đồng nghĩa mất dữ liệu.

**Suggested fix (Phase 1):** Hỗ trợ key identifier/multi-key decrypt, công cụ re-encrypt và runbook backup/rotation.

---

### API mẫu chưa minh họa authorization

**Issue:** `SettingController` trả plaintext, bao gồm secret, nhưng không có `[Authorize]`. Tenant thực tế do host thiết lập qua `ICurrentTenant` (middleware / R2) trước khi vào manager.

**Impact:** Consumer có thể sao chép Sample sang production và vô tình expose cấu hình nhạy cảm hoặc hiểu nhầm header action tự thiết lập tenant context.

**Suggested fix:** Tài liệu hóa middleware tenant bắt buộc, thêm policy authorization trong host mẫu, và mask secret ở HTTP response nếu caller không thực sự cần plaintext.

---

### Tạo entity bypass constructor

**Issue:** `RuntimeHelpers.GetUninitializedObject` tạo `TSetting` mà không chạy constructor.

**Impact:** Invariant/default value trong entity concrete có thể bị bỏ qua.

**Suggested fix:** Yêu cầu `new()` trên `TSetting`, dùng factory do host cung cấp, hoặc bổ sung abstraction `ISettingEntityFactory<TSetting>`.

---

## 4. Best Practices & Improvements

- Bảo vệ endpoint Setting bằng policy riêng; không chỉ dựa vào việc caller đã đăng nhập.
- Không commit `Encryption:DataEncryptionKey`; dùng secret store hoặc environment variable.
- Bắt buộc unique index `(TenantId, Key)` và index `(TenantId, Group)` trong mọi host.
- Không dùng trực tiếp repository/`DbSet<Setting>` ở UI hoặc service nghiệp vụ; luôn qua `ISettingManager`.
- Không trả password hiện tại về form quản trị nếu UI không cần; masking thuộc HTTP/UI host, không thay đổi contract nội bộ của manager.
- Cấu hình cache key phải chứa cả `{tenantId}` và `{key}`.
- Thêm metric cho cache hit/miss, decrypt failure, latency và số lần conflict khi `GetOrCreateAsync`.
- Thêm integration test với DB và distributed cache thật cho các invariant về tenant, unique race và secret.

---

## 5. Đã triển khai

| Hạng mục | Trạng thái |
|----------|------------|
| Atomic package `Jarvis.Modules.Setting` | ✅ |
| Contract persistence `ISettingEntity` | ✅ |
| Library code-first theo Provider/Registry | ✅ |
| Metadata Group chỉ nằm trong memory | ✅ |
| Facade `ISettingManager` | ✅ |
| CRUD theo Key | ✅ |
| Đọc/form/upsert theo Group | ✅ |
| UUID v7 khi tạo row | ✅ |
| AES-GCM, payload `enc.v1.*` | ✅ |
| Blank secret giữ giá trị hiện tại | ✅ |
| Cache key theo Tenant + Key, invalidate sau ghi | ✅ |
| `IsReadOnly` chặn update/delete | ✅ |
| Xử lý race trong `GetOrCreateAsync` | ✅ |
| Host Sample: entity, EF mapping, migration, Email provider, API | ✅ |
| Validate Value theo Type/Options | ❌ Phase 1 |
| Encryption key rotation | ❌ Phase 1 |
| Unit/integration test chuyên biệt cho Setting | ❌ Phase 1 |

---

## 6. Summary

| Khu vực | Đánh giá |
|---------|----------|
| Definition Library | Registry code-first gọn, deterministic, phát hiện duplicate Key lúc startup |
| `ISettingManager` | Facade rõ ràng; bao phủ metadata, CRUD, form, group upsert và race cơ bản |
| Persistence | Host-owned, phù hợp Atomic Module; phụ thuộc host map đúng index và tenant context |
| Encryption | AES-GCM đúng mục tiêu at-rest; cache giữ ciphertext; còn thiếu rotation |
| Cache | Key tenant-safe, invalidate đúng luồng ghi và không lưu plaintext secret |
| Sample | Đủ minh họa tích hợp và SMTP consumer; chưa phải mẫu security production |
| Tài liệu | README/SAD/OpenAPI đã mô tả chi tiết; ADR này là bản ghi quyết định và trạng thái triển khai |

**Overall:** kiến trúc Phase 0 phù hợp để tiếp tục phát triển. Cache secret đã được xử lý; chưa nên tuyên bố production-ready cho tới khi bổ sung kiểm thử, authorization và quy trình rotation key.

---

## 7. Hướng dẫn host

### Entity và EF mapping

Host cung cấp entity concrete implement `ISettingEntity` và quản lý migration:

```csharp
public sealed class Setting : BaseEntity<Guid>, ISettingEntity
{
    public Guid TenantId { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string? Description { get; set; }
    public bool IsReadOnly { get; set; }
}
```

Ràng buộc tối thiểu:

```csharp
builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
builder.HasIndex(x => new { x.TenantId, x.Group });
```

Module không phát hành migration vì schema và DbContext thuộc host.

### Definition provider

```csharp
public sealed class EmailSettingDefinition : ISettingDefinitionProvider
{
    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup("Email", group =>
        {
            group.DisplayName = "Email";
            group.Order = 10;
        });

        context.AddSetting("Email", "Email.Host", setting =>
        {
            setting.Name = "Host";
            setting.Type = SettingValueTypes.Text;
        });

        context.AddSetting("Email", "Email.Password", setting =>
        {
            setting.Name = "Password";
            setting.Type = SettingValueTypes.Password;
        });
    }
}
```

`Password` tự mặc định `IsEncrypted = true`.

### Registration

```csharp
builder.AddJarvisCaching();

builder.AddCurrentTenant<CurrentTenantInfo>();
builder.AddCoreSetting()
    .UseEntityFramework<IMasterUnitOfWork, CurrentTenantInfo>()
    .AddProvider<EmailSettingDefinition>();
```

`AddJarvisCaching()` phải được đăng ký trước hoặc có sẵn trong host.

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
        "DistributedGroup": "Default"
      }
    }
  }
}
```

Không lưu khóa thật trong file cấu hình được commit. Distributed cache chỉ lưu ciphertext; sau lần deploy đầu tiên có thay đổi này cần xóa các key `setting:*` cũ để loại bỏ entry plaintext còn TTL.

### Luồng frontend khuyến nghị

```text
GET /settings/groups
  → chọn Group
GET /settings/form?group=Email
  → render metadata + Value/default
PUT /settings/group/Email
  → partial upsert các Key được gửi
```

API HTTP là trách nhiệm của host. Host phải xác thực, authorize, thiết lập tenant context và audit trước khi gọi manager.

---

## 8. Kiến trúc hiện tại

### Luồng startup và runtime

```text
ISettingDefinitionProvider
        │ Define
        ▼
SettingDefinitionRegistry (singleton, in-memory)
        │ metadata
        ▼
ISettingManager / SettingManager
   ├── ICacheService       key: TenantId + Key
   ├── AesGcmStringEncryptionHelper  AES-GCM (Jarvis.Common), enc.v1.*
   └── IRepository<TSetting> qua TUnitOfWork
                            │
                            ▼
                     SQL Setting table
```

### Cấu trúc project

```text
Jarvis.Modules.Setting/
├── Definitions/       Provider, Context, Registry, Group/Setting metadata
├── (encryptor)        Jarvis.Common.Encryption — AesGcmStringEncryptionHelper
├── Extensions/        AddCoreSetting, JarvisSettingBuilder
├── Models/            SettingModel, SettingFormItemModel
├── Services/          ISettingManager, SettingManager
├── doc/               README, SAD, OpenAPI, ADR
├── SettingErrorCode.cs
└── SettingValueTypes.cs
```

### Contract `ISettingManager`

```csharp
IReadOnlyList<SettingGroupDefinition> GetGroups();
IReadOnlyList<SettingDefinition> GetDefinitions(string? group = null);
Task<SettingModel?> GetAsync(string key, CancellationToken cancellationToken = default);
Task<IReadOnlyList<SettingModel>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);
Task<IReadOnlyList<SettingFormItemModel>> GetFormAsync(string group, CancellationToken cancellationToken = default);
Task<SettingModel> CreateAsync(string key, string? value = null, CancellationToken cancellationToken = default);
Task<SettingModel> GetOrCreateAsync(string key, CancellationToken cancellationToken = default);
Task<SettingModel> UpdateAsync(string key, string value, CancellationToken cancellationToken = default);
Task<IReadOnlyList<SettingModel>> SaveGroupAsync(
    string group,
    IReadOnlyDictionary<string, string> values,
    CancellationToken cancellationToken = default);
Task DeleteAsync(string key, CancellationToken cancellationToken = default);
```

### Library và Database

| Library — dùng chung mọi tenant | Database — theo tenant |
|---------------------------------|-------------------------|
| Group, DisplayName, Order | `TenantId`, mã `Group` |
| Key, Name, Type, Options | Key, Value |
| DefaultValue | Snapshot metadata cấp Key |
| IsEncrypted, IsReadOnly | `IsReadOnly` snapshot |
| Rebuild từ provider lúc startup | Tạo row theo nhu cầu runtime |

Library là source of truth để dựng form. Database là source of truth cho Value runtime.

### Quy ước secret

```text
Setting thường + value=""                   → lưu chuỗi rỗng
Secret + value="" hoặc whitespace           → 98107; không encrypt, không ghi
Secret + value non-empty                    → encrypt và persist
SaveGroup bỏ key secret khỏi payload        → không đụng secret đã lưu (partial upsert)
```

Xóa secret: dùng `DeleteAsync` (nếu không `IsReadOnly`); không dùng chuỗi rỗng.

---

## 9. Đối chiếu Atomic Module Pattern

| Jarvis.Modules.Setting cung cấp | Host sở hữu |
|---------------------------|-------------|
| `ISettingEntity` contract | Entity concrete, DbContext, EF mapping, migration |
| Definition provider/registry | Các Group/Key nghiệp vụ |
| `ISettingManager` | Authorization policy và HTTP API |
| AES-GCM encryptor | Secret store, backup và rotation runbook |
| Cache integration | Cache backend, topology và policy cho secret |
| Tenant-aware cache key | Tenant resolution/middleware |
| Error codes và DTO | API envelope, localization, masking |
| `IsReadOnly` enforcement | Audit log và approval workflow |

Quy tắc sử dụng: host xử lý authentication, authorization và tenant context trước; sau đó mới gọi `ISettingManager`.

---

## 10. Kế hoạch phases

### Phase 0 — Nền tảng SettingManagement ✅

- [x] Package atomic + `ISettingEntity`
- [x] Code-first Definition Library
- [x] `ISettingManager` theo Key/Group
- [x] Encrypt at rest AES-GCM
- [x] Cache Tenant + Key và invalidate
- [x] Secret blank semantics
- [x] Concurrent `GetOrCreateAsync`
- [x] Sample entity/provider/controller/consumer

### Phase 1 — Production hardening

- [x] Không đưa plaintext secret vào distributed cache
- [ ] Unit test encrypt roundtrip, cache invalidate, read-only, duplicate Key
- [ ] Integration test concurrent `GetOrCreateAsync` với unique index thật
- [ ] Test blank secret trong `UpdateAsync` và `SaveGroupAsync`
- [ ] Validate `Text`, `Number`, `Combobox`, `Password`
- [ ] Thêm `MultiSelect`, `DateTime`, `Date` và quy ước serialize
- [ ] Rotation/multi-key cho encryption
- [ ] Chốt source of truth và chiến lược đồng bộ metadata snapshot
- [ ] Bổ sung authorization + tenant middleware minh họa trong Sample
- [ ] Thay `GetUninitializedObject` bằng entity factory an toàn

### Phase 2 — Khi có nhu cầu nghiệp vụ

- [ ] Bulk GetOrCreate theo Group
- [ ] Import/Export JSON theo Group
- [ ] Event khi Setting thay đổi
- [ ] Metric/log vận hành
- [ ] API explicit để clear secret
- [ ] Dynamic Group/Key runtime nếu product không còn phù hợp code-first

---

## 11. Breaking changes dự kiến

Phase 0 chưa công bố breaking change so với package Setting trước đó vì đây là module mới. Các thay đổi Phase 1 sau đây có thể ảnh hưởng consumer và phải được version/document:

| Thay đổi | Ảnh hưởng |
|----------|-----------|
| Thêm validation Type/Options | Value từng được chấp nhận có thể bị từ chối |
| Thay đổi cách cache secret | Cache config/TTL và hiệu năng đọc có thể đổi |
| Bỏ snapshot metadata hoặc thêm metadata version | Migration/schema/DTO có thể đổi |
| Thêm key identifier vào ciphertext | Payload mới phải vẫn đọc được `enc.v1.*` cũ |
| Mask secret ở HTTP API | Không ảnh hưởng `ISettingManager`; ảnh hưởng response của host |
| Thêm entity factory/new constraint | Host entity registration có thể cần cập nhật |

---

## 12. Tiến độ

| Phase | Ngày | Ghi chú |
|-------|------|---------|
| Phase 0 | 2026-07-29 | Module, Sample integration và migration đã có trong working tree |
| ADR review | 2026-07-30 | Đối chiếu toàn bộ source `Jarvis.Modules.Setting`, Sample, README/SAD/OpenAPI |
| Phase 1 | Chưa lên lịch | Test, validation, key rotation, authorization sample |
| Phase 2 | Backlog | Chỉ thực hiện khi có nhu cầu product |

---

*Cập nhật: 2026-07-30 — ADR phản ánh source hiện tại trong workspace; trạng thái test chưa được dùng làm bằng chứng vì module chưa có test chuyên biệt được ghi nhận.*
