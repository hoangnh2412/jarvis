# ADR — Catalog Org events (đồng bộ / integration)

> **Trạng thái:** 🟡 **Accepted (boundary)** · **chưa implement code** (nợ phase sau)  
> **Ngày:** 2026-10-21 · Accept boundary: 2026-10-21  
> **Loại:** Module Atomic / contract / integration boundary  
> **Liên quan:** [Module Orgs](./2026-10-17_platform_orgs-module.md) (D4, D5, hooks), [OrgStructure](./2026-10-17_platform_orgstructure-module.md), [ImportExport](./2026-08-15-adr-platform-importexport-module.md), [architecture-rules.md](./architecture-rules.md) §0.2, [adr-template.md](./adr-template.md)  
> **Phạm vi:** Định nghĩa **catalog events** khi Create / Update / Delete Org qua `IOrgManager`; vị trí type + port publish trong `Platform.Orgs.*`; thời điểm phát trong lifecycle hooks hiện có.  
> **Ngoài phạm vi:** Message bus cụ thể (Kafka, RabbitMQ), transactional outbox schema, handler đồng bộ sang hệ thứ ba, MediatR wiring ở Host — **host-owned** hoặc package riêng sau.  
> **Chú thích icon:** 🟢 xong · 🟡 accepted / chờ code · 🔴 chưa làm

---

## 1. Bối cảnh

Module **`Platform.Orgs`** đã có catalog + lifecycle qua **`IOrgManager`**, entity host-owned (`IOrgCatalogEntity`), extension **`IOrgCatalogHooks<TOrg>`** với pipeline:

```text
Validate → invariant platform → Prepare → SaveChanges
  → Success (nếu OK)
  → Error + rethrow (nếu lỗi)
  → Finally (luôn)
```

Sản phẩm sẽ cần **đồng bộ dữ liệu Org** sang search index, data warehouse, module phụ (OrgStructure master, billing, …) **sau khi** bản ghi catalog đã persist. Hôm nay chưa có type event hay dispatcher trong repo Orgs; team muốn **chốt hướng DDD-friendly** và implement **later**.

| Thành phần | Hiện tại |
|------------|----------|
| `IOrgCatalog` / `OrgDescriptor` | 🟢 Abstractions — đọc / validate assignment |
| `IOrgManager` + hooks | 🟡 catalog slice |
| Domain event / outbox platform | 🔴 chưa có pattern chung |

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không contract event chuẩn cho thay đổi catalog Org | Mỗi host tự bắn message ad-hoc; module consumer không share type |
| P2 | Nhầm hook `Prepare` / `Success` với integration event | Publish trước commit hoặc payload không ổn định |
| P3 | Đưa Kafka/outbox vào `Platform.Orgs` core | Phá Module Atomic; khó test; lock-in transport |
| P4 | Entity `Org` không nằm trong package (D5) | Khó áp classic “aggregate raises domain event” **trong** platform — lifecycle do `OrgManager` điều phối |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Event **chỉ** phát sau **`SaveChanges` thành công** (cùng nhánh `Success*`), không phát trong `Prepare` / `Validate` |
| C2 | Payload event **không** expose type entity host — dùng **`OrgDescriptor`** hoặc snapshot tối thiểu (`TenantId`, `OrgId`, field chuẩn) |
| C3 | Type event + port publish đặt **`Platform.Orgs.Abstractions`** (consumer chỉ Abstractions) hoặc subfolder `Events/` cùng package — **không** reference EF / bus |
| C4 | `Platform.Orgs` core có thể gọi port tùy chọn; **mặc định no-op** (giống `DefaultOrgCatalogHooks`) |
| C5 | Host có thể vẫn dùng **`Success*`** hook cho logic riêng; event platform **không thay** hook — bổ sung contract cho nhiều subscriber |
| C6 | Delete: event chỉ khi **`DeleteAsync`** (hoặc tương đương) được implement trên `IOrgManager` — hiện 🔴 chưa có API delete |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược | Kết luận |
|---|-----------|---------|----|-------|----------|
| O1 | Chỉ hook `Success*` — host tự định nghĩa message | Không thêm type platform | Linh hoạt | Không share contract giữa module | Từ chối làm **duy nhất** (OK bổ sung tại host) |
| O2 | `CreateOrgEvent` imperative naming | Tên theo command | Dễ đọc cho dev mới | Lệch DDD/event sourcing convention | Từ chối tên mặc định |
| O3 | Rich aggregate host `RaiseDomainEvent` | Pure DDD trong Domain product | Invariant trong entity | Platform không sở hữu aggregate; bypass `IOrgManager` gãy catalog | **Host option** — không phải default platform |
| O4 | **Catalog events + `IOrgCatalogEventPublisher`** sau commit, type ở Abstractions | Module atomic + SPI | Đồng bộ sau này; test no-op | Thêm package surface | **Chọn** |
| O5 | Gộp publish vào `Platform.OpenTelemetry` / logging only | Quan sát, không contract | Nhanh | Không đủ cho integration | Từ chối làm thay O4 |

---

## 5. Quyết định

| # | Quyết định | Chi tiết | Status |
|---|------------|----------|--------|
| D1 | Tên event (quá khứ) | **`OrgCreatedEvent`**, **`OrgUpdatedEvent`**, **`OrgDeletedEvent`** — không dùng `CreateOrgEvent` làm tên platform mặc định | 🟡 Accepted |
| D2 | Base / metadata | Mọi event có tối thiểu **`TenantId`**, **`OrgId`**, **`OccurredAt`** (UTC); payload chính: **`OrgDescriptor`** (sau create/update) hoặc descriptor cuối trước delete | 🟡 Accepted |
| D3 | Vị trí type | **`Platform.Orgs.Abstractions`** — namespace gợi ý `Platform.Orgs.Abstractions.Events` | 🟡 Accepted |
| D4 | Port publish | **`IOrgCatalogEventPublisher`** (hoặc tên tương đương) — `PublishAsync(OrgCatalogEventBase, CancellationToken)` hoặc overload theo loại; **default `NullOrgCatalogEventPublisher`** no-op | 🟡 Accepted |
| D5 | Ai gọi publish | **`OrgManager`** (hoặc hook nội bộ đăng ký mặc định khi bật events) **sau** `SaveChanges` + trước hoặc sau **`Success*`** host — **đề xuất:** publish **trước** `Success*` để handler host vẫn chạy sau event platform | 🟡 Accepted (chi tiết implement Q2) |
| D6 | DDD mapping | Coi event là **integration / catalog notification** phát từ **application boundary** (`OrgManager`), không bắt entity host implement `IDomainEvent` | 🟡 Accepted |
| D7 | Transport | Outbox / bus / retry = **Host Infrastructure**; platform chỉ contract + gọi port | 🟡 Accepted |
| D8 | DI | Opt-in: `AddPlatformOrgs` giữ no-op publisher; extension kiểu **`.UseOrgCatalogEventPublisher<T>()`** hoặc đăng ký handler collection — chốt khi implement | 🔴 implement |
| D9 | Liên kết hooks | `Prepare*` = map cột host; `Success*` = side effect tùy host; **event** = contract cross-module — không trùng trách nhiệm | 🟡 Accepted |

### 5.1 Luồng (mục tiêu khi implement)

```text
… → SaveChanges OK
  → map OrgDescriptor
  → IOrgCatalogEventPublisher.PublishAsync(OrgCreatedEvent | Updated | Deleted)
  → Success* (host hooks)
  → return
```

Lỗi: **không** publish created/updated; có thể dùng `Error*` / `Finally*` cho telemetry (đã có hooks).

### 5.2 Phân tách Domain event vs integration (tham chiếu)

| Khái niệm | Trong sản phẩm host | Trong platform Orgs |
|-----------|---------------------|---------------------|
| Domain event trên aggregate `Org` | Host có thể thêm nếu entity rich + UoW dispatch | Không bắt buộc; ngoài phạm vi package |
| Catalog event (ADR này) | Handler → outbox → bus | Type + publish port sau manager commit |

---

## 6. Hệ quả

- Consumer (`OrgStructure`, ImportExport, …) reference **`Platform.Orgs.Abstractions`** + subscribe qua host DI hoặc wrapper publisher.
- **`OrgManagerTests`** khi implement: publisher fake, assert **không** publish khi conflict/validation fail; assert **có** publish khi create OK.
- ADR Orgs §9 thêm phase events (xem [Orgs ADR](./2026-10-17_platform_orgs-module.md) phase 3b).

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D7 boundary (implement later)? | _chờ_ |
| Q2 | Publish **trước** hay **sau** `Success*` host? | _đề xuất: trước Success* (D5)_ |
| Q3 | `OrgUpdatedEvent` có kèm diff field hay chỉ full descriptor? | _đề xuất: full `OrgDescriptor` MVP; diff Later_ |
| Q4 | `SetActiveAsync` phát `OrgUpdatedEvent`? | _đề xuất: Yes — cùng Updated_ |

- [ ] Accept boundary D1–D7
- [ ] Chốt thứ tự vs `Success*`
- [ ] Chốt payload Updated / SetActive

---

## 8. Test cases *(khi implement)*

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | Create OK + publisher đăng ký | `OrgCreatedEvent` 1 lần; `TenantId`/`OrgId` khớp | 🔴 |
| T2 | Create duplicate code | Không publish | 🔴 |

### 8.2 Regression

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T3 | Host không đăng ký publisher | No-op; T1–T2 Orgs hiện có vẫn xanh | 🔴 |
| T4 | `Success*` hook host | Vẫn chạy sau publish (nếu D5 = trước Success) | 🔴 |

### 8.3 Mới

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T5 | `SetActiveAsync` | `OrgUpdatedEvent` | 🔴 |
| T6 | `DeleteAsync` (khi có) | `OrgDeletedEvent` | 🔴 |

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| E0 | Confirm §7 | Accepted | — |
| E1 | Types + `IOrgCatalogEventPublisher` trong Abstractions | Build | E0; Orgs phase 3 🟡 |
| E2 | Wire `OrgManager` + unit test T1–T4 | Tests 🟢 | E1 |
| E3 | Sample host: log hoặc fake handler (optional) | Doc | E2 |
| E4 | Host product: outbox + bus (không trong Orgs core) | Sản phẩm | E2 |

**Ghi chú:** Phase E* **không chặn** catalog slice hiện tại; làm sau khi cần đồng bộ.

---

## 10. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟡 **Accepted (boundary)** — chờ confirm §7 |
| Implementation | 🔴 **Deferred** — nợ code |
| Blocked by | Không (có thể implement độc lập sau catalog) |
| Unblocks | Integration OrgStructure / DWH / search |
| Owner | Platform |
