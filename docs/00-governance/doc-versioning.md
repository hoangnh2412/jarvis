# DOC Versioning — Quy tắc version & Change Log

| Version | Date | Author | Status |
|---------|------|--------|--------|
| — | YYYY-MM-DD | | Draft |

**Áp dụng cho:** mọi DOC-01–18, governance doc và artifact trong `docs/` (trừ snapshot `02-baseline/` — read-only).

**Liên quan:** [`doc-registry.md`](../05-traceability/doc-registry.md) · [`business-glossary.md`](business-glossary.md) · [`baseline-history.md`](baseline-history.md) (baseline ID).

---

## 1. Nguyên tắc

| Quy tắc | Mô tả |
|---------|--------|
| **Version chỉ sau approve** | Cột **Version** trong header và **Change Log** chỉ ghi số version **sau khi REQ owner sign-off** (§ Approval). |
| **Trước approve = Draft** | Mọi chỉnh sửa nháp: **Status = Draft**, **Version = `—`**. Không tăng version, không thêm dòng `Updated` trong header. |
| **Một dòng header** | Bảng version đầu file luôn **một dòng** phản ánh trạng thái **hiện tại** — không liệt kê lịch sử chỉnh sửa nháp. |
| **Change Log = đã approve** | § Change Log (hoặc tương đương) chỉ ghi các mốc **đã sign-off**. Chỉnh sửa nháp không ghi vào Change Log. |

---

## 2. Header (đầu mỗi DOC)

```markdown
| Version | Date | Author | Status |
|---------|------|--------|--------|
| — | YYYY-MM-DD | {Author} | Draft |
```

| Cột | Trước sign-off | Sau sign-off |
|-----|----------------|--------------|
| **Version** | `—` | `0.1`, `0.2`, … *(theo baseline / lần ký)* |
| **Date** | Ngày cập nhật gần nhất | Ngày sign-off |
| **Author** | Người soạn / cập nhật | Người trình / cập nhật lần ký |
| **Status** | `Draft` hoặc `Review` | `Baseline` *(hoặc `Accepted` với ADR)* — [DOC — Status](business-glossary.md#1-doc--status) |

> **Review:** Author chuyển sang `Review` khi sẵn sàng REQ owner ký — vẫn giữ **Version = `—`** cho đến khi sign-off.

**Sau sign-off**, cập nhật một dòng header (vd. `| 0.1 | 2026-06-20 | BA | Baseline |`). Chỉnh sửa tiếp theo **chưa** sign-off lại → quay về `— | … | Draft`.

**Lịch sử version đã ký** nằm tại: Change Log trong DOC, [`doc-registry.md`](../05-traceability/doc-registry.md), [`baseline-history.md`](baseline-history.md), snapshot `02-baseline/vX.Y/` — không nhân bản nhiều dòng trong header.

---

## 3. Change Log

**Trước approve lần đầu:**

```markdown
> Quy tắc versioning: [`doc-versioning.md`](../../00-governance/doc-versioning.md).

| Version | Thay đổi | Tác giả |
|---------|----------|---------|
| — | *(chưa có — chờ approve lần đầu)* | — |
```

**Sau mỗi lần sign-off:** thêm một dòng với **Version** tương ứng, tóm tắt thay đổi đã được phê duyệt, **Tác giả**.

Không ghi từng chỉnh sửa nháp (vd. `0.3 Updated`, `0.4 Updated`…) khi chưa có sign-off.

---

## 4. Đồng bộ `doc-registry.md`

| Trường registry | Quy tắc |
|-----------------|---------|
| **Ver** | `—` khi DOC đang Draft/Review chưa ký; số version sau sign-off |
| **Người thực hiện tài liệu** | Khớp cột **Author** header DOC — [Registry — Author](business-glossary.md#5-registry--author) |
| **Status** | Khớp header DOC — [DOC — Status](business-glossary.md#1-doc--status) |
| **Sign-off** | `☑` chỉ khi mục Approval REQ owner đã ký |
| **Người Sign-off** | REQ owner trong mục Approval — [Sign-off (registry)](business-glossary.md#2-sign-off-registry) |
| **Ngày sign-off** | Ngày trong mục Approval; `—` nếu chưa ký |

---

## 5. Tham chiếu version DOC khác

Khi trích dẫn nội dung DOC dependency (vd. *"theo DOC-06 v0.3"*) — dùng **version đã baseline** của DOC đó, không dùng số version nháp chưa ký.

---

## 6. Change Log (tài liệu này)

| Version | Thay đổi | Tác giả |
|---------|----------|---------|
| — | *(chưa có — chờ approve lần đầu)* | — |
