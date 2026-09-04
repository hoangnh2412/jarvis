# ADR Template — Jarvis

> Template chuẩn cho Architecture Decision Record trong repo này.
> Tham chiếu: [Nygard ADR](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions), [MADR](https://adr.github.io/madr/), Fowler [ADR](https://martinfowler.com/bliki/ArchitectureDecisionRecord.html).
> Copy file này → `YYYY-MM-DD-adr-<slug>.md`, xóa mục hướng dẫn, điền nội dung.

---

## Quy ước (bắt buộc)

### Tên file

```text
YYYY-MM-DD-adr-<slug-kebab-case>.md
```

Ví dụ: `2026-08-07-adr-jarvis-orm-packages.md`

- `YYYY-MM-DD` = ngày **tạo** ADR (không đổi khi accept/implement).
- `slug` = ngắn, ổn định; không đổi sau khi đã link từ ADR khác / README.

### Trạng thái

| Giá trị | Khi nào |
|---------|---------|
| 🔴 Proposed | Đang thảo luận / chờ confirm |
| 🟡 Accepted | Đã chốt, chưa implement (hoặc đang làm dở) |
| 🟢 Accepted + Implemented | Quyết định đã phản ánh trong code + docs |
| ⚫ Deprecated | Không còn áp dụng |
| 🔄 Superseded by `<link ADR>` | Bị thay bởi ADR khác |

Một ADR **Accepted** không sửa nội dung quyết định để “đổi ý”. Muốn đổi → ADR mới + đánh dấu Superseded.

### Mã mục (ID) — thống nhất theo **vai trò**, không theo chủ đề

Trong **một** ADR, mọi hàng bảng dùng prefix cố định:

| Prefix | Vai trò | Ví dụ |
|--------|---------|--------|
| **P** | Problem / force (nỗi đau, lực kéo) | P1, P2 |
| **C** | Constraint (ràng buộc đã chốt / không đàm phán trong ADR này) | C1, C2 |
| **O** | Option (phương án đang cân nhắc — trước khi chọn) | O1, O2 |
| **D** | Decision (quyết định đã chọn / đề xuất chọn) | D1, D2 |
| **Q** | Confirm question (câu hỏi chốt với team) | Q1, Q2 |
| **T** | Test case (smoke / regression / mới) | T1, T2 |

**Không** dùng prefix theo tên chủ đề (`E` = EF, `G` = generic, `M` = Multitenancy…). Cross-ref giữa ADR dùng **link file + mã**:

```markdown
[D5 home vs current](./2026-08-01-adr-current-user-tenant.md) · [O2 R2 resolve](./2026-08-01-adr-current-user-tenant.md)
```

Phase triển khai đánh số riêng: `0`, `1`, `2`… hoặc `A`, `B`, `C` — **không** trùng nghĩa với D/C/O/T.

### Test (bắt buộc khi ADR có thay đổi hành vi / API / DI)

Mọi quyết định **Accepted** mà kéo theo code **phải** có mục **Test cases** trước khi 🟢 Implemented:

| Loại | Mục đích | Khi nào ghi |
|------|----------|-------------|
| **Smoke** | Luồng chính vẫn chạy sau sửa (build + happy path tối thiểu) | **Luôn** — ít nhất 1 case |
| **Regression** | Hành vi cũ không gãy (suite / test đã có liên quan) | **Luôn** — liệt kê test hiện có cần giữ xanh; nếu chưa có thì ghi rõ sẽ thêm |
| **Mới** | Case cho semantic / API mới của ADR | Chỉ khi cần (breaking, nhánh mới, validation mới) |

- Không bắt buộc viết suite contract/reflection riêng nếu smoke + regression EF/Multitenancy (hoặc tương đương) đã cover quyết định.
- **Done khi:** mọi hàng T* smoke + regression 🟢; case mới (nếu có) 🟢.
- Icon: 🟢 xong · 🟡 đã có, cần giữ xanh · 🔴 chưa có / cần thêm.

### Icon tiến độ (trong kế hoạch / checklist)

- 🟢 xong · 🟡 đang làm / còn việc · 🔴 chưa làm

### Một ADR = một quyết định kiến trúc chính

Nếu cần slice package / tech debt / follow-up API → ADR riêng, link qua metadata **Liên quan**. Không nhồi nhiều quyết định độc lập vào một file.

---

# ADR — {Tiêu đề ngắn: quyết định + phạm vi}

> **Trạng thái:** 🔴 Proposed · 🟡 Accepted · 🟢 Accepted + Implemented · ⚫ Deprecated · 🔄 Superseded by […]  
> **Ngày:** YYYY-MM-DD · Accept: YYYY-MM-DD · Implement: YYYY-MM-DD  
> **Loại:** {API / package boundary / Module Atomic / tech debt / naming / …}  
> **Liên quan:** [ADR liên quan](./….md), [architecture-software.md](./architecture-software.md)  
> **Phạm vi:** {packages / contracts / DI / Host — cụ thể những gì ADR này quyết}  
> **Ngoài phạm vi:** {cố ý không làm ở đây; trỏ ADR khác nếu đã có}  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

`Jarvis.EntityFramework` vừa chứa infra EF chung vừa đăng ký wiring multitenancy. Host gọi `AddEntityFramework()` là kéo luôn tenant adapter — không opt-in. Cần tách satellite Persistence theo Module Atomic.

| Thành phần | Hiện tại |
|------------|----------|
| `Jarvis.Multitenancy` | Ambient `CurrentTenant*` + `AddCurrentTenant` |
| `Jarvis.EntityFramework` | UoW/repos **và** tenant interceptor |

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Tenant–EF nằm trong package EF chung | Không bật Multitenancy persistence độc lập |
| P2 | `AddEntityFramework` gọi `AddMultitenancy` | App “chỉ EF” vẫn kéo resolvers tenant |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | `Jarvis.Multitenancy` **không** reference EF |
| C2 | Semantic resolve / interceptor **không** đổi — chỉ đổi chỗ package + DI |

Chỉ ghi điều **không** mở lại trong ADR này. Tham chiếu quyết định cũ bằng link, không đổi mã cũ.

---

## 4. Phương án đã cân nhắc *(tuỳ chọn — khuyến nghị khi có trade-off)*

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Giữ wiring trong EF | Ít churn | Nhanh | Không opt-in; phình EF |
| O2 | Satellite `*.EntityFramework` | Core + satellite | Rõ Atomic; Host compose | Thêm package + cut-over DI |

---

## 5. Quyết định

Chúng ta sẽ tách adapter tenant–EF sang package satellite; Host opt-in.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tên package | `Jarvis.Multitenancy.EntityFramework` |
| D2 | DI Host | `AddEntityFramework()` không gọi `AddMultitenancy`; opt-in `AddMultitenancyEntityFramework()` |

### 5.1 Chi tiết / sơ đồ *(tuỳ chọn)*

```text
Host → AddCurrentTenant + AddEntityFramework + AddMultitenancyEntityFramework
Multitenancy.EF → Multitenancy + EntityFramework
EntityFramework → Domain  (không reference Multitenancy)
```

### 5.2 Inventory / mapping *(tuỳ chọn — khi move code / rename package)*

```text
Jarvis.EntityFramework/TenantDbConnectionInterceptor.cs
  → Jarvis.Multitenancy.EntityFramework/...
```

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Host chọn được “EF không tenant” hoặc “EF + Multitenancy.EF” |
| Xấu / chi phí | Breaking DI Sample/tests; thêm ProjectReference |
| Trung lập | Hành vi interceptor giữ nguyên nếu Host đăng ký đủ |

---

## 7. Confirm *(bắt buộc trước khi Accepted)*

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D2? | OK |
| Q2 | Chọn phương án? | O2 |
| Q3 | Tên extension DI cuối? | `AddMultitenancyEntityFramework()` |

Sau khi chốt: cập nhật **Trạng thái** → 🟡 Accepted (hoặc 🟢 nếu đã implement cùng lúc), ghi ngày Accept.

---

## 8. Test cases *(bắt buộc nếu ADR đổi code / hành vi)*

Liệt kê **smoke + regression** (bắt buộc) và **test mới** (chỉ khi cần). Trỏ file/test method cụ thể khi đã có.

### 8.1 Smoke *(bắt buộc — ≥ 1)*

| # | Case | Expect | Nơi (file / filter) | Trạng thái |
|---|------|--------|---------------------|------------|
| T1 | Host compose DI + tạo UoW / DbContext cơ bản | Không throw; resolve được service chính | Sample / UnitTest smoke | 🔴 |

### 8.2 Regression — test cũ cần giữ xanh *(bắt buộc)*

| # | Case | Expect | Nơi (test hiện có) | Trạng thái |
|---|------|--------|--------------------|------------|
| T2 | Hành vi cũ liên quan phạm vi ADR vẫn đúng | Suite liên quan xanh | ví dụ `MultitenancyEfTests.*` | 🟡 |

Nếu chưa có regression phù hợp: ghi 🔴 và thêm ở §8.3 (hoặc mở rộng suite cũ) — **không** 🟢 Implemented khi thiếu.

### 8.3 Test mới *(tuỳ chọn — chỉ khi semantic/API mới cần cover)*

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T3 | … | … | UnitTest / … | 🔴 |

### 8.4 Done khi

- Mọi hàng **Smoke** + **Regression** 🟢.
- Hàng **Test mới** (nếu có) 🟢.
- Không bắt buộc suite phản chiếu API thuần nếu smoke/regression đã chứng minh quyết định.

---

## 9. Kế hoạch triển khai *(tuỳ chọn — nên có nếu ADR kéo theo code)*

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Status → Accepted | — |
| 1 | Tạo package + move interceptor | Build xanh | 0 |
| 2 | Cut-over Sample + §8 smoke/regression | T* xanh | 1 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Package + DI opt-in trên Sample | 🔴 |
| 2 | §8 Smoke + Regression xanh | 🔴 |
| 3 | ADR + [README index](./README.md) cập nhật | 🔴 |

---

## 11. Tham chiếu thêm *(tuỳ chọn)*

- [architecture-software.md](./architecture-software.md) §0.2 Core + satellite
- Commit / PR implement (điền khi xong)

---

<!--
Hướng dẫn nhanh khi viết ADR mới:
1. Copy template → đổi tên file theo quy ước.
2. Điền metadata + §1–§3 trước; §4 nếu có trade-off; §5 là lõi.
3. Không invent prefix theo chủ đề. Chỉ P / C / O / D / Q / T.
4. Cross-ref ADR khác bằng link file, không dựa vào chữ cái chủ đề.
5. Nếu đổi code: §8 bắt buộc — smoke + regression; test mới chỉ khi cần.
6. Không 🟢 Implemented khi smoke/regression chưa xanh.
7. Sau Accept: không rewrite quyết định — chỉ bổ sung ngày implement / checklist / note “đã làm”.
8. Thêm dòng vào ADRs/README.md.
-->
