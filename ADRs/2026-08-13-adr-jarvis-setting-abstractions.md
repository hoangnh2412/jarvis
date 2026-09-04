# ADR — Tách `Jarvis.Modules.Setting.Abstractions`

> **Trạng thái:** 🟡 **Accepted (boundary)** · chưa implement code  
> **Ngày:** 2026-08-13 · Accept boundary: 2026-08-13  
> **Loại:** Package boundary / Module Atomic / SPI  
> **Liên quan:** [Setting ADR](../modules/settings/Jarvis.Modules.Setting/doc/2026-07-30-adr-setting.md), [modules/settings/README](../modules/settings/README.md), [architecture-software.md](./architecture-software.md) §0.2, [Realtime vs Inbox](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) (module notifications sẽ khai báo Setting), [adr-template.md](./adr-template.md)  
> **Phạm vi:** Tách SPI code-first Library (`ISettingDefinitionProvider` + cụm definition) sang package mỏng trong `modules/settings/`; ranh giới phụ thuộc module → Abstractions vs Setting core; **không** đưa SPI vào `Jarvis.DDD.Domain`.  
> **Ngoài phạm vi:** Đổi semantic Library/Registry/Manager; move `ISettingEntity` (giữ Domain); authorization; UI; rename `ISettingManager`.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm / chờ confirm chi tiết · 🔴 chưa làm

---

## 1. Bối cảnh

Setting đã có Library code-first:

| Thành phần | Package hiện tại | Vai trò |
|------------|------------------|---------|
| `ISettingDefinitionProvider` / Context / `SettingDefinition*` / `SettingValueTypes` | `Jarvis.Modules.Setting` | SPI khai báo Group/Key |
| `SettingDefinitionRegistry`, `ISettingManager`, validation, encrypt, DI | `Jarvis.Modules.Setting` | Runtime |
| `ISettingEntity` | `Jarvis.DDD.Domain` | Contract persistence |
| EF / HTTP | `.EntityFramework` / `.API` | Satellite |

Hướng sản phẩm: module nghiệp vụ (vd. Notifications inbox) **tự khai báo** setting của mình qua provider, rồi Host `AddProvider<T>()`.

Hôm nay mọi thứ SPI nằm trong core Setting → module đóng góp definition phải reference **cả** `Jarvis.Modules.Setting` (manager, cache, AspNetCore…).

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | SPI definition nằm cùng package runtime Setting | Module chỉ cần `Define()` vẫn kéo manager / validation / FrameworkReference |
| P2 | Cân nhắc đưa `ISettingDefinitionProvider` vào `DDD.Domain` vì “mọi module đều reference Domain” | Domain phình SPI feature; Setting không còn atomic; module không dùng Setting vẫn “nhận” khái niệm form/UI metadata |
| P3 | Chỉ chuyển một interface | Không đủ — `Provider` phụ thuộc `Context` → `SettingDefinition` / `SettingGroupDefinition` → `SettingValueTypes` |
| P4 | Folder `modules/settings/` vs package dependency dễ bị hiểu nhầm | Nhầm “phụ thuộc module Setting” với “phụ thuộc full `Jarvis.Modules.Setting`” |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Dependency một chiều; Host = composition root |
| C2 | Setting vẫn là **atomic business module** — không elevate toàn bộ Setting thành kernel Domain |
| C3 | `ISettingEntity` **giữ** `Jarvis.DDD.Domain` (persistence contract đã chốt) |
| C4 | Semantic Library (duplicate Key fail-fast, Password → `IsEncrypted`, Group in-memory) **không** đổi — chỉ đổi chỗ package |
| C5 | `Jarvis.Modules.Setting` (core) reference Abstractions; Abstractions **không** reference core / EF / API |
| C6 | Module nghiệp vụ đóng góp definition → chỉ được reference **Abstractions** (không bắt buộc core) |
| C7 | Không vòng: Setting ↛ Notifications; Notifications → Setting.Abstractions (OK) |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược | Kết luận |
|---|-----------|---------|----|-------|----------|
| O1 | Giữ SPI trong `Jarvis.Modules.Setting` | Không tách package | Ít churn | Module kéo runtime nặng | Từ chối (P1) |
| O2 | Đưa SPI vào `Jarvis.DDD.Domain` (/ Shared) | Mọi module đã phụ thuộc Domain | Một ProjectReference sẵn có | Domain ≠ feature SPI; ô nhiễm metadata UI; khó bỏ Setting | **Từ chối** |
| O3 | Host-owned providers only | Module chỉ publish const key; Host viết provider | Zero dep Setting từ module | Scale kém khi nhiều module; xa product intent “module tự Define” | Dự chọn vận hành, không phải boundary mặc định |
| O4 | Tách `Jarvis.Modules.Setting.Abstractions` trong `modules/settings/` | Package SPI mỏng | Opt-in mỏng; Domain sạch; Atomic rõ | Thêm package + cut-over using/csproj | **Chọn** |

---

## 5. Quyết định

Chúng ta tách SPI Library sang package Abstractions; Domain không chứa definition provider.

| # | Quyết định | Chi tiết | Status |
|---|------------|----------|--------|
| D1 | Package | **`Jarvis.Modules.Setting.Abstractions`** — `PackageId` / `AssemblyName` / `RootNamespace` cùng tên | 🟡 Accepted |
| D2 | Vị trí | `modules/settings/Jarvis.Modules.Setting.Abstractions/` (cạnh core / EF / API) | 🟡 Accepted |
| D3 | Không đưa SPI vào Domain | “Mọi module reference Domain” **không** biện minh nhét Setting SPI vào Domain | 🟡 Accepted |
| D4 | Ai reference gì | Module đóng góp definition → **chỉ Abstractions**. Host / Setting core / EF / API → core (+ Abstractions transitive). Đọc/ghi runtime → core (`ISettingManager`) | 🟡 Accepted |
| D5 | Inventory Abstractions (tối thiểu) | Xem §5.2 — SPI + metadata types + `SettingValueTypes` | 🟡 Accepted |
| D6 | Giữ trong core | Registry impl, `ISettingDefinitionRegistry`, `ISettingManager`, `SettingValueValidator`, encrypt, `AddCoreSetting` / `AddProvider` extension | 🟡 Accepted |
| D7 | `SettingTypeOptions` | **Chuyển sang Abstractions** (provider Sample/module dùng helper build `Options`) — validator vẫn core | 🟡 Accepted (Q2) |
| D8 | Namespace | `Jarvis.Modules.Setting.Abstractions` (+ subfolder `Definitions` / `Validation` nếu cần). Breaking using — chấp nhận khi cut-over cùng PR | 🟡 Accepted (Q3) |
| D9 | Docs / skill | Cập nhật `modules/settings/README`, Setting ADR/SAD, skill setting-dotnet (nếu có) khi implement | 🔴 sau code |

### 5.1 Dependency graph (đích)

```text
  [Module Notifications]                [Module khác…]
           │                                    │
           └──────────────┬─────────────────────┘
                          ▼
           Jarvis.Modules.Setting.Abstractions
              ISettingDefinitionProvider
              ISettingDefinitionContext
              SettingDefinition / SettingGroupDefinition
              SettingValueTypes
              SettingTypeOptions
                          ▲
                          │
           Jarvis.Modules.Setting  (core)
              Registry, ISettingManager, validator, DI
                    ▲           ▲
                    │           │
         Setting.EntityFramework   Setting.API
                    │
                    ▼
                  Host
         AddCoreSetting().AddProvider<T>()…
```

**Folder** `modules/settings/` = khu vực sản phẩm Setting.  
**Package** Abstractions ≠ phụ thuộc full runtime Setting.

### 5.2 Inventory move

| Từ (core hiện tại) | Sang Abstractions | Ghi chú |
|--------------------|--------------|---------|
| `Definitions/ISettingDefinitionProvider.cs` | ✅ | SPI |
| `Definitions/ISettingDefinitionContext.cs` | ✅ | SPI |
| `Definitions/SettingDefinition.cs` | ✅ | Metadata |
| `Definitions/SettingGroupDefinition.cs` | ✅ | Metadata |
| `SettingValueTypes.cs` | ✅ | Hằng Type |
| `Validation/SettingTypeOptions.cs` | ✅ (D7) | Helper Options cho provider |
| `Definitions/ISettingDefinitionRegistry.cs` | ❌ giữ core | Port runtime lookup |
| `Definitions/SettingDefinitionRegistry.cs` | ❌ giữ core | Impl |
| `Services/*`, encrypt, Extensions DI | ❌ giữ core | Runtime |
| `Jarvis.DDD.Domain/.../ISettingEntity.cs` | ❌ không đụng | Persistence |

### 5.3 Target layout

```text
modules/settings/
  Jarvis.Modules.Setting.Abstractions/
    Definitions/
      ISettingDefinitionProvider.cs
      ISettingDefinitionContext.cs
      SettingDefinition.cs
      SettingGroupDefinition.cs
    Validation/
      SettingTypeOptions.cs
    SettingValueTypes.cs
    Jarvis.Modules.Setting.Abstractions.csproj   # tối thiểu deps ( BCL / không AspNetCore nếu không cần )

  Jarvis.Modules.Setting/
    → ProjectReference Abstractions
    Definitions/SettingDefinitionRegistry.cs
    Services/…
    Extensions/…

  Jarvis.Modules.Setting.EntityFramework/
  Jarvis.Modules.Setting.API/
  frontend/
```

Core csproj: bỏ `FrameworkReference` khỏi đường đi của module chỉ cần Abstractions (Abstractions **không** reference `Microsoft.AspNetCore.App` trừ khi bắt buộc — mặc định không).

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Module đóng góp setting chỉ kéo SPI mỏng; Domain không phình; ranh giới Atomic rõ |
| Tốt | Host vẫn một chỗ `AddCoreSetting().AddProvider<>()`; semantic Registry giữ nguyên |
| Xấu / chi phí | Thêm package; breaking namespace/using; cập nhật Sample + docs + solution |
| Trung lập | `ISettingEntity` vẫn Domain; đọc giá trị vẫn qua `ISettingManager` (core) |
| Rủi ro | Nhét nhầm Registry/Manager vào Abstractions → Abstractions phình lại như core — tránh bằng D6 |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D6, D8–D9 (tách Abstractions, không Domain)? | **OK** (2026-08-13) |
| Q2 | `SettingTypeOptions` vào Abstractions (D7)? | **OK** — provider cần helper; validator ở core |
| Q3 | Namespace `Jarvis.Modules.Setting.Abstractions` (breaking using) vs giữ `Jarvis.Modules.Setting.Definitions`? | **`…Setting.Abstractions`** — ranh giới package rõ |
| Q4 | Có type-forward / obsolete shim trong core một version? | **Không** (MVP) — cut-over một PR; Sample + module cùng sửa |
| Q5 | `ISettingDefinitionRegistry` có public cho module khác không? | **Không qua Abstractions** — module Define thôi; đọc metadata/value qua Host/`ISettingManager` |

Sau confirm: status boundary giữ 🟡 Accepted; implement → cập nhật checklist + §8 → 🟢.

---

## 8. Test cases

### 8.1 Smoke *(bắt buộc)*

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T1 | Build solution: Abstractions + Setting + EF + API + Sample | Compile xanh; ProjectReference đúng chiều | `dotnet build` | 🔴 |
| T2 | Host `AddCoreSetting().AddProvider<DemoSettingDefinition>()` (hoặc Email) | Registry nạp Group/Key như trước | Sample startup / smoke | 🔴 |

### 8.2 Regression

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T3 | Suite Setting / Sample liên quan definition + manager | Xanh; duplicate key / Password encrypt mặc định không đổi | UnitTest (nếu có) + smoke Sample | 🟡 / 🔴 bổ sung nếu thiếu |
| T4 | Provider Sample (`*SettingDefinition`) sau đổi namespace | `Define` + `SettingTypeOptions` / `SettingValueTypes` resolve từ Abstractions | `Sample/Settings/*` | 🔴 |

### 8.3 Test mới *(tuỳ chọn)*

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T5 | (Optional) Project reference graph: mock “module” chỉ ref Abstractions, implement provider, đăng ký từ Host | Compile không cần reference Setting core | UnitTest hoặc project smoke nhỏ | 🔴 nếu cần chứng minh P1 |

### 8.4 Done khi

- T1–T2 🟢; T3–T4 🟢 (hoặc ghi rõ chưa có suite rồi thêm).
- T5 không bắt buộc nếu T1 + Sample đã chứng minh cut-over.

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 (đã OK Q1–Q5) | Boundary Accepted | — |
| 1 | Tạo csproj Abstractions; move inventory §5.2; namespace mới | Build Abstractions + Setting | 0 |
| 2 | Cập nhật Setting / EF / API / Sample usings + solution | T1–T4 | 1 |
| 3 | README Setting + SAD/ADR Setting + ADRs index; skill nếu có | Docs khớp layout | 2 |
| 4 | (Sau) Module Notifications reference Abstractions cho inbox settings | Theo [Realtime vs Inbox](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) | 2 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Package `Jarvis.Modules.Setting.Abstractions` + move types | 🔴 |
| 2 | Core/EF/API/Sample cut-over; dependency chiều đúng | 🔴 |
| 3 | §8 Smoke + Regression xanh | 🔴 |
| 4 | Docs Setting + ADR index | 🔴 |
| 5 | ADR status → 🟢 Accepted + Implemented | 🔴 |

---

## 11. Tham chiếu thêm

- [architecture-software.md](./architecture-software.md) — Core + satellite / không vòng module  
- Thảo luận: SPI ≠ Domain chỉ vì mọi module đã reference Domain; folder Setting ≠ package runtime Setting  
- Commit / PR implement *(điền khi xong)*
