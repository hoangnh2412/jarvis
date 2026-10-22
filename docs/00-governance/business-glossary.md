# Business Glossary — Trạng thái & thuật ngữ

| Version | Date | Author | Status |
|---------|------|--------|--------|
| — | YYYY-MM-DD | | Draft |

> Quy tắc versioning: [`doc-versioning.md`](doc-versioning.md).

**Nguồn sự thật duy nhất** cho mọi **trạng thái** và **thuật ngữ** dùng trong `docs/` — DOC, registry, trace, DOC-16, stakeholder. Các file khác **tham chiếu** file này, không định nghĩa lại.

**Liên quan:** [`doc-registry.md`](../05-traceability/doc-registry.md) · [`doc-versioning.md`](doc-versioning.md) · [`DOC-02`](../01-project/DOC-02-stakeholder-analysis.md)

---

## Mục lục

| Mục | Nội dung |
|-----|----------|
| **Trạng thái** | |
| [1](#1-doc--status) | DOC — Status |
| [2](#2-sign-off-registry) | Sign-off (registry) |
| [3](#3-baseline) | Baseline |
| [4](#4-doc-16--trạng-thái-thực-thi-tc) | DOC-16 — Trạng thái thực thi TC |
| **Thuật ngữ** | |
| [5](#5-registry--author) | Registry — Author |
| [6](#6-req--priority-fr--ac) | REQ — Priority (FR / AC) |
| [7](#7-dự-án--tm--sh--raci) | Dự án — TM / SH, RACI |
| [7.2](#72-raci--ma-trận-phân-công) | RACI — ma trận phân công *(chi tiết)* |
| [8](#8-doc-16--cột-catalog) | DOC-16 — Cột catalog |
| [9](#9-doc-16--layer-path-priority) | DOC-16 — Layer, Path, Priority |
| [10](#10-doc-16--defect-severity) | DOC-16 — Defect severity |

---

## 1. DOC — Status

Áp dụng: header mọi DOC, cột **Status** trong [`doc-registry.md`](../05-traceability/doc-registry.md).

| Giá trị | Ý nghĩa | Khi nào dùng |
|---------|---------|--------------|
| **Draft** | Bản nháp — đang soạn hoặc chỉnh sửa; chưa đủ điều kiện baseline | Mặc định khi mở DOC mới hoặc đang cập nhật nội dung |
| **Review** | Đủ nội dung — REQ owner đang rà soát trước sign-off | Author chuyển sang Review khi sẵn sàng ký; đồng bộ header `Status` trong file DOC |
| **Baseline** | Đã sign-off REQ owner | Cột **Baseline** registry = ID baseline (vd. `v1.0`); snapshot [`02-baseline/`](../02-baseline/) |
| **Accepted** | Quyết định đã chốt — **ADR** | Team implement theo ADR; đổi qua ADR mới hoặc CR |
| **Superseded** | Đã thay thế — **ADR** / DOC arch cũ | Có ADR/DOC kế nhiệm; giữ trace lịch sử |
| **Deprecated** | Không dùng baseline mới — **legacy** | Chỉ tham chiếu; không đưa vào `02-baseline/` manifest |

**Luồng thông thường:** `Draft` → `Review` → *(REQ owner sign-off)* → `Baseline`

**ADR:** `Draft` → `Accepted` *(có thể baseline sau)* → `Superseded`

**Version:** chỉ ghi số sau sign-off — [`doc-versioning.md`](doc-versioning.md).

---

## 2. Sign-off (registry)

| Cột registry | Giá trị | Ý nghĩa |
|--------------|---------|---------|
| **Sign-off** | **☐** | Chưa ký — mục Approval chưa `☑` |
| | **☑** | Đã ký REQ owner |
| **Người Sign-off** | **{alias}**, … | REQ owner trong mục Approval — alias hoặc tên ngắn |
| **Ngày sign-off** | **—** | Chưa có ngày trong mục Approval |
| | **YYYY-MM-DD** | Ngày ký REQ owner |

**REQ owner:** người **Accountable** phê duyệt DOC → **Baseline** — xác định trong [DOC-02](../01-project/DOC-02-stakeholder-analysis.md) và [Luồng phê duyệt tài liệu](../01-project/DOC-02-stakeholder-analysis.md#23-luồng-phê-duyệt-tài-liệu) *(tùy dự án)*. Stakeholder cấp trên có thể chỉ **Informed** — không chặn baseline.

**Điều kiện `☐` → `☑`:** Nội dung đủ phạm vi baseline; trace FR↔UC↔AC (nếu DOC REQ); không blocker mở; mục Approval có ngày ký.

**Highlight:** ô **Status = Baseline** tô nền <mark>vàng</mark> trong `doc-registry.md` để quét nhanh.

---

## 3. Baseline

| Giá trị (cột **Baseline**) | Ý nghĩa |
|------------------------------|---------|
| **—** | Chưa đưa vào baseline package |
| **v1.0**, **v1.1**, … | ID baseline — khớp [`baseline-history.md`](baseline-history.md), [`02-baseline/vX.Y/manifest.yaml`](../02-baseline/v1.0/manifest.yaml) |

---

## 4. DOC-16 — Trạng thái thực thi TC

Áp dụng: cột **Trạng thái** catalog; mục chi tiết TC (**Status**). Cột catalog → [Cột catalog](#8-doc-16--cột-catalog).

| Giá trị | Ý nghĩa |
|---------|---------|
| *(trống)* | Chưa thực thi |
| **Pass** | Đạt kết quả mong muốn |
| **Fail** | Không đạt — tạo defect |
| **Blocked** | Không chạy được (môi trường, dependency, data) |
| **Skip** | Bỏ qua có chủ đích — ghi lý do trong DOC-16 / test report |

Trace matrix (DOC-16 mục trace): **Pass** / **Fail** trên từng dòng FR↔TC.

---

## 5. Registry — Author

| Thuật ngữ | Ý nghĩa |
|-----------|---------|
| **Người thực hiện tài liệu** | Khớp cột **Author** header DOC — người soạn hoặc cập nhật lần gần nhất |
| Tên / alias | Giá trị trong cột registry & header (vd. tên đầy đủ, **BA**, alias REQ owner) |

---

## 6. REQ — Priority (FR / AC)

Áp dụng: cột Priority trong DOC-06, DOC-07, trace matrix.

| Giá trị | Ý nghĩa |
|---------|---------|
| **Must** | Bắt buộc MVP / baseline; chặn UAT & go-live nếu chưa đáp ứng |
| **Should** | Quan trọng; có thể hoãn release nhỏ nếu trade-off |
| **Could** | Mong muốn; không chặn UAT / sign-off |

---

## 7. Dự án — TM / SH, RACI

| Ký hiệu | Ý nghĩa |
|---------|---------|
| **TM-xxx** | Thành viên dự án (nội bộ) — [Thành viên dự án](../01-project/DOC-02-stakeholder-analysis.md#21-thành-viên-tham-gia-dự-án) |
| **SH-xxx** | Stakeholder bên ngoài / persona vận hành — [Stakeholder](../01-project/DOC-02-stakeholder-analysis.md#22-stakeholder) |

### 7.2 RACI — ma trận phân công

Áp dụng: bảng RACI trong [DOC-02](../01-project/DOC-02-stakeholder-analysis.md#4-raci-sơ-bộ) và ma trận tương tự trong dự án.

| Ký hiệu | Tiếng Anh | Tiếng Việt | Vai trò |
|---------|-----------|------------|---------|
| **R** | Responsible | **Người thực hiện** | Làm trực tiếp công việc / deliverable; có thể nhiều **R** trên một hàng |
| **A** | Accountable | **Người chịu trách nhiệm** | **Một** người chốt kết quả; quyền quyết định cuối |
| **C** | Consulted | **Tham vấn** | Cho ý kiến **hai chiều** trước hoặc trong khi làm |
| **I** | Informed | **Được thông báo** | Nhận cập nhật **một chiều** sau khi có quyết định / tiến độ |

**Quy ước đọc bảng RACI:**

| Ghi chú | Ý nghĩa |
|---------|---------|
| **A/R** | Cùng một người vừa **chịu trách nhiệm** vừa **tự thực hiện** |
| Một hàng — một **A** | Mỗi deliverable chỉ **một** Accountable |
| **R** vs **C** | **R** = làm xong việc; **C** = góp ý, không owner công việc |
| **C** vs **I** | **C** = hỏi ý kiến trước khi chốt; **I** = báo cáo sau khi đã chốt |

**Ma trận stakeholder (Ảnh hưởng × Quan tâm):**

| Giá trị | Ý nghĩa |
|---------|---------|
| **Quản lý chặt** | Ảnh hưởng cao, quan tâm cao |
| **Giữ hài lòng** | Ảnh hưởng cao, quan tâm thấp hơn |
| **Thông báo** | Cần đồng bộ định kỳ |
| **Theo dõi** | Giám sát nhẹ |

**Mức Ảnh hưởng / Quan tâm:** Cao · Trung bình (TB) · Thấp.

---

## 8. DOC-16 — Cột catalog

Áp dụng: bảng catalog trong mọi [`DOC-16`](../03-modules/) (`03-modules/`).

```text
| TC ID | Mô tả | Kết quả mong muốn | Layer | Path | Priority | Trạng thái |
```

| Cột | Ý nghĩa |
|-----|---------|
| **TC ID** | `{MOD}-TC-{NNN}` |
| **Mô tả** | Tóm tắt kịch bản |
| **Kết quả mong muốn** | Expected outcome ngắn |
| **Layer** | [Layer, Path, Priority](#9-doc-16--layer-path-priority) |
| **Path** | [Layer, Path, Priority](#9-doc-16--layer-path-priority) |
| **Priority** | [Layer, Path, Priority](#9-doc-16--layer-path-priority) *(kế thừa FR/AC hoặc quy tắc gán)* |
| **Trạng thái** | Kết quả thực thi — [Trạng thái thực thi TC](#4-doc-16--trạng-thái-thực-thi-tc) |

---

## 9. DOC-16 — Layer, Path, Priority

### Layer — tầng kiểm thử chính

| Giá trị | Ý nghĩa |
|---------|---------|
| `E2E` | Kiểm thử qua UI portal (FE + BE) |
| `FE` | Client validation; không gọi API |
| `BE/API` | API / integration; không qua UI |
| `FE/BE` | Validation FE **và** BE — ghi rõ trong DOC-16 chi tiết nếu chỉ một tầng |

> Cột **Layer** = tầng **chính** khi thực thi catalog (một TC có thể chạy nhiều tầng).

### Path — loại luồng

| Giá trị | Ý nghĩa |
|---------|---------|
| `Happy` | Luồng thành công |
| `Unhappy` | Từ chối / lỗi nghiệp vụ / unauthorized |
| `Validation` | Input không hợp lệ (subset Unhappy) — **Priority** mặc định **Must** |

### Priority — catalog TC

| Giá trị | Ý nghĩa |
|---------|---------|
| `Must` | Map FR/AC **Must** — bắt buộc pass trước UAT / sign-off |
| `Should` | Map FR/AC **Should** hoặc TC edge bổ sung |
| `Could` | Map FR/AC/BR **Could** — không chặn UAT; chạy khi còn capacity |

**Quy tắc gán Priority (TC):**

1. TC có **FR / AC** → kế thừa priority DOC-06 / DOC-07.
2. **Path** = `Validation` → **Must** (trừ khi DOC-16 ghi khác).
3. TC chỉ **BR / UC** (edge) → mặc định **Should**.
4. TC không map requirement → **Could**.
5. Smoke / UAT: **Must** trước → **Should** regression → **Could** tùy chọn.

---

## 10. DOC-16 — Defect severity

| Severity | Ý nghĩa | SLA fix (tham chiếu) |
|----------|---------|------------------------|
| **Critical** | Chặn production / go-live | 24h |
| **High** | Tính năng chính hỏng | 3 ngày |
| **Medium** | Có workaround | Sprint kế |
| **Low** | Cosmetic | Backlog |

---

## Change Log

| Version | Thay đổi | Tác giả |
|---------|----------|---------|
| — | *(chưa có — chờ approve lần đầu)* | — |
