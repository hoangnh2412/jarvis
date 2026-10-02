# ADR — Family `Platform.ORM.*` (rename EF + tạo Dapper)

> **Trạng thái:** 🟢 **Accepted + Implemented** (Accept §7 2026-08-10 · Phase B–D 2026-08-10).  
> **Ngày:** 2026-08-07 · Accept §7 2026-08-10 · Implement 2026-08-10  
> **Loại:** Package boundary / naming / foundation Persistence  
> **Liên quan:** [Multitenancy.EF](./2026-08-06-adr-platform-multitenancy-entityframework.md) (🟢 Done — satellite **không** gộp vào ORM), [Tenants module](./2026-08-07-adr-platform-tenants-module.md) (🔴), [architecture-rules.md](./architecture-rules.md) (§0.2 Core + satellite), [README](../README.md).  
> **Phạm vi:** đổi tên **`Platform.EntityFramework` → `Platform.ORM.EntityFramework`**; tạo package nền **`Platform.ORM.Dapper`**; chốt ranh giới **ORM foundation ≠ module Persistence satellite**.  
> **Ngoài phạm vi:** implement đầy đủ Dapper UoW/multitenancy (chỉ skeleton MVP); Multitenancy.Dapper; `Platform.Tenants` / Setting CRUD; đổi semantic R2 / query filter.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Trước implement:

| Package | Vai trò thực tế |
|---------|-----------------|
| `frameworks/Platform.EntityFramework` | Infra EF chung: repos, `BaseUnitOfWork`, `BaseStorageContext`, helpers |
| Module satellites | `Platform.Modules.Setting.EntityFramework`, `Platform.Multitenancy.EntityFramework` 🟢 |
| Dapper | Chưa có package |

Sau implement (2026-08-10): family `Platform.ORM.*` dưới `frameworks/`.

---

## 2. Vấn đề

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Tên `Platform.EntityFramework` không mirror được Dapper | Khó diễn đạt “chọn ORM foundation” trên Host |
| D2 | Chưa có `Platform.ORM.Dapper` | Host/report phải tự wire Dapper; lệch Atomic |
| D3 | Rủi ro hiểu nhầm “ORM = mọi thứ đụng DB” | God-package; phá opt-in satellite module |
| D4 | Rename PackageId / namespace = breaking | Cần kế hoạch cut-over Sample, skill, consumers |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | `Platform.ORM.EntityFramework` / `Platform.ORM.Dapper` = **foundation only** |
| C2 | **Không** chứa Tenants CRUD, Setting persistence, Multitenancy interceptor |
| C3 | Module satellite giữ tên `Platform.{Module}.{Orm}` / `Platform.Modules.{Module}.{Orm}` |
| C4 | ORM.* **không** reference module nghiệp vụ |
| C5 | Domain chỉ ports — **không** reference `Platform.ORM.*` |
| C6 | Host composition opt-in EF và/hoặc Dapper |
| C7 | Semantic EF (filter `ITenantEntity`, repo lifetimes) **không** đổi chỉ vì rename |
| C8 | Breaking PackageId: cut-over một lần — không dual-package |

---

## 4. Quyết định (Accepted 2026-08-10)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| O1 | Family | **`Platform.ORM.*`** dưới `frameworks/` |
| O2 | Rename | **`Platform.EntityFramework` → `Platform.ORM.EntityFramework`** |
| O3 | Tạo | **`Platform.ORM.Dapper`** — skeleton MVP |
| O4 | **Không** umbrella `Platform.ORM` | Host reference trực tiếp từng ORM package |
| O5 | Vai trò EF | Repos, UoW, `BaseStorageContext`, helpers; tenant wiring = Multitenancy.EF |
| O6 | Vai trò Dapper | `ISqlConnectionFactory` + `AddOrmDapper()` — không clone EF API |
| O7 | Module satellites | **Không** move vào `Platform.ORM.*` |
| O8 | DI EF | **Giữ `AddEntityFramework()`** |
| O9 | Skill | Cập nhật PackageId khi implement; skill Dapper khi API ổn định |
| O10 | Thứ tự | Multitenancy.EF **trước** rename — đã Done 🟢 trước Phase B |

### 4.1 Layout (implemented)

```text
frameworks/
  Platform.ORM.EntityFramework/     ← rename từ Platform.EntityFramework
    └─ AddEntityFramework()
  Platform.ORM.Dapper/              ← mới
    ├─ ISqlConnectionFactory / ConfigSqlConnectionFactory
    └─ AddOrmDapper()
  Platform.Multitenancy.EntityFramework/   ← satellite 🟢
modules/settings/
  Platform.Modules.Setting.EntityFramework/
```

### 4.2 `Platform.ORM.Dapper` — MVP (Done)

| Có trong MVP | Chưa (Later) |
|--------------|--------------|
| Project + `PackageId` + sln | Full parity repo/UoW với EF |
| `AddOrmDapper()` + connection factory từ config | Dynamic filter / Multitenancy.Dapper |
| Host cung cấp `CreateConnection` (ADO.NET provider) | Bulk / transaction abstraction lớn |
| Sample `SampleDapperReadSample` (`SELECT 1`) | — |

### 4.3 Breaking change (EF rename) — Done

| Hạng mục | Việc |
|----------|------|
| `PackageId` / ProjectReference | → `Platform.ORM.EntityFramework` |
| Namespace | → `Platform.ORM.EntityFramework.*` (cắt sạch) |
| DI | Giữ `AddEntityFramework()` |
| Sample / UnitTest / README | Cập nhật cùng cut-over |

---

## 5. Kế hoạch triển khai

| Phase | Việc | Done khi | Status |
|-------|------|----------|--------|
| 0 | Confirm §7 (O1–O10) | Status → Accepted | 🟢 2026-08-10 |
| A | Multitenancy.EF tách tenant wiring | EF foundation sạch concern | 🟢 |
| B | Rename → `Platform.ORM.EntityFramework`; giữ `AddEntityFramework()` | Build + test xanh | 🟢 2026-08-10 |
| C | `Platform.ORM.Dapper` skeleton + Sample thin | Host opt-in được | 🟢 2026-08-10 |
| D | README + ADR index Done | Docs đồng bộ | 🟢 2026-08-10 |
| E | (Later) Multitenancy.Dapper / filter động | ADR riêng khi cần | 🔴 |

**Note implement:** `TenantScopedContextValidation` chỉ yêu cầu ambient `TenantId` khi change tracker có thay đổi `ITenantEntity` — cần vì `MasterDbContext` map `Setting` (`ISettingEntity : ITenantEntity`) cùng catalog tenant; không đổi global query filter.

---

## 6. Hệ quả

### Tích cực

- Family ORM rõ; Dapper có chỗ đặt package.
- Foundation ORM ≠ module Persistence.
- Giữ `AddEntityFramework()` → Host churn chủ yếu PackageId/namespace.

### Chi phí

- Breaking rename monorepo.
- Hai ORM foundation cần bảo trì (Dapper giữ mỏng).

---

## 7. Checklist confirm

### Confirm §7 (2026-08-10)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept O1–O10, C1–C8? | **OK** |
| 2 | Tên sau rename | **`Platform.ORM.EntityFramework`** — **OK** |
| 3 | Package Dapper | **`Platform.ORM.Dapper`** — **OK** |
| 4 | Không umbrella | **OK** |
| 5 | Satellites không move vào ORM.* | **OK** |
| 6 | DI EF | **Giữ `AddEntityFramework()`** |
| 7 | Namespace | Cắt sạch — **OK** |
| 8 | Thứ tự vs Multitenancy.EF | **OK** (đã Done) |
| 9 | Versioning | Bump + release note; không dual-package |
| 10 | Dapper MVP | Skeleton + Sample thin — **OK** |

- [x] Accept O1–O10, C1–C8
- [x] Rename → `Platform.ORM.EntityFramework`
- [x] Tạo `Platform.ORM.Dapper`
- [x] Không umbrella / không nuốt satellites
- [x] Giữ `AddEntityFramework()`; namespace cắt sạch
- [x] Phase B–D implemented

---

## 8. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-10) |
| Implementation | 🟢 **Done** Phase B–D (2026-08-10) |
| Blocked by | — |
| Next | Phase E Later — Multitenancy.Dapper khi cần; skill `orm-dapper-dotnet` khi API ổn định |
| Không thay thế | Multitenancy.EF, Tenants module, Setting.EF |
| Owner | Platform |
