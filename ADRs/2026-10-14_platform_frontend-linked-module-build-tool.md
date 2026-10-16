# ADR — Script build linked-frontend module theo dependency graph (dùng chung cho mọi Host)

> **Trạng thái:** 🔴 Proposed
> **Ngày:** 2026-10-14
> **Loại:** build tooling / dev infra
> **Liên quan:** ADR sản phẩm Lexora (repo `lexora`, submodule `documents`, không link trực tiếp được vì khác repo): `documents/staging/ADRs/proposed_2026-10-14_lexora-host-build-linked-frontend-modules.md`
> **Phạm vi:** thư mục mới `platform/tools/frontend/` — script CLI độc lập, mọi Host (Lexora, tương lai khác) gọi qua path tương đối.
> **Ngoài phạm vi:** không đổi `vite.config.ts` của bất kỳ Host nào (Lexora, Sample); không đổi `Sample.csproj`/`EnsureLinkedFrontendDeps`; không viết test runner JS riêng cho `tools/`; không publish script thành npm package.
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Sản phẩm dùng Platform qua `file:` dependency (vd. `@platform/core` build bằng `tsup` → `dist/`). Khi Host build SPA chính, nó dùng `dist/` có sẵn của package liên kết — **không tự rebuild** nếu source Platform vừa đổi. `Sample` không gặp vấn đề này vì alias Vite trỏ thẳng source (`EnsureLinkedFrontendDeps` chỉ `npm install`, không build).

Lexora (ADR liên quan ở trên) muốn: trong `.csproj` chỉ khai **danh sách** module liên kết, tool tự tính thứ tự build theo quan hệ phụ thuộc `file:` giữa các module đó, build trước khi build SPA chính — không tự sắp thứ tự tay. Vì vấn đề "dist cũ không tự rebuild" không riêng gì Lexora (Host nào dùng `file:` dependency tới Platform cũng gặp), script nên đặt dùng chung ở `platform` thay vì copy riêng từng sản phẩm.

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | `file:` dependency không tự rebuild khi source Platform đổi | Host dùng nhầm `dist` cũ, không báo lỗi |
| P2 | Nhiều module liên kết có quan hệ phụ thuộc chéo (A phụ thuộc B) | Người viết `.csproj` phải tự biết thứ tự đúng — dễ sai, không scale |
| P3 | Logic build-theo-thứ-tự-phụ-thuộc là generic, không riêng Lexora | Nếu viết trong `lexora/`, Host khác dùng Platform sau này phải copy lại |

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Không đổi cách bất kỳ Host nào hiện khai `file:` dependency hay `vite.config.ts` — script chỉ rebuild `dist/`, không đổi cơ chế resolve |
| C2 | Script không phụ thuộc MSBuild/.NET cụ thể — chạy độc lập bằng `node`, Host nào cũng gọi được qua `Exec`/shell |
| C3 | Không tự ý đổi `Sample.csproj`/`EnsureLinkedFrontendDeps` trong ADR này — muốn Sample dùng lại script thì làm ở ADR/slice riêng |

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Mỗi Host tự copy script riêng (trong `lexora/`) | Script sống cùng sản phẩm dùng nó | Nhanh, không đụng repo `platform` | Trùng lặp logic nếu Host khác (dùng Platform) cần y hệt |
| O2 | Đặt script dùng chung trong `platform/tools/` | Một chỗ maintain, mọi Host gọi qua path tương đối | Không trùng lặp; đúng vai trò "framework dùng chung" của Platform | Đổi phải qua 3-repo chain (platform → bump erp_core → bump lexora) |

## 5. Quyết định

Thêm `platform/tools/frontend/build-linked-frontend.mjs` (ESM, CLI nhận danh sách path qua argv).

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Vị trí | `platform/tools/frontend/build-linked-frontend.mjs` — thư mục `tools/` mới ở top-level `platform/`, ngang hàng `frameworks/`, `modules/`, `Sample/` |
| D2 | Input | Danh sách path (tương đối hoặc tuyệt đối) tới các thư mục module — mỗi thư mục có `package.json` — truyền qua argv: `node build-linked-frontend.mjs <path1> <path2> ...` |
| D3 | Thuật toán | Đọc `package.json` từng path → gộp `dependencies`+`devDependencies`+`peerDependencies` → nếu **key trùng `name`** của module khác trong danh sách **và** value bắt đầu `file:` → cạnh phụ thuộc. Kahn's algorithm topo-sort; phát hiện cycle → in rõ chu trình, `process.exit(1)` |
| D4 | Build | Theo thứ tự đã sort: `npm install` (chỉ nếu thiếu `node_modules`) rồi `npm run build` (script `build` của chính module đó — **không** giả định có `build:nocache`) |
| D5 | Lỗi | Bất kỳ bước nào lỗi (thiếu `package.json`, cycle, install/build fail) → `process.exit(1)` — Host gọi qua `Exec` sẽ fail rõ, không âm thầm dùng `dist` cũ |
| D6 | Phân phối | Không publish thành npm package riêng — chỉ là file CLI nội bộ repo `platform`; Host khác tham chiếu qua path tương đối xuyên submodule (giống cách `.csproj` đã reference lẫn nhau hiện nay) |

### 5.1 Chi tiết

```text
platform/tools/frontend/build-linked-frontend.mjs
  node build-linked-frontend.mjs <modulePath1> <modulePath2> ... <modulePathN>
  → đọc package.json mỗi path, dựng graph theo "file:" dependency trùng name
  → topo-sort, build lần lượt theo thứ tự đã tính (không theo thứ tự argv)
```

Cách gọi từ phía Host (ví dụ Lexora — chi tiết đầy đủ nằm ở ADR sản phẩm liên quan, không lặp lại ở đây):

```xml
<Target Name="BuildLinkedFrontendModules" BeforeTargets="DebugBuildFrontend;PublishRunWebpack" Condition="'$(SkipFrontendBuild)' != 'true'">
  <Exec Command="node &quot;...\platform\tools\frontend\build-linked-frontend.mjs&quot; @(LinkedFrontendModule->'&quot;%(Identity)&quot;', ' ')" />
</Target>
```

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Một chỗ maintain cho mọi Host dùng Platform qua `file:` dependency; tự tính thứ tự build theo dependency graph, không cần sắp tay; lỗi rõ ràng khi cycle hoặc build fail thay vì build sai thứ tự âm thầm |
| Xấu / chi phí | Thay đổi script phải đi qua 3 repo (`platform` → bump con trỏ `erp_core` → bump con trỏ `lexora`) để Host cuối thấy được; thêm 1 file CLI cần maintain, review riêng trong repo `platform` |
| Trung lập | `Sample` chưa dùng script này (vẫn giữ `EnsureLinkedFrontendDeps` cũ) — có thể chuyển sau nếu muốn, ở ADR riêng |

---

## 7. Confirm *(bắt buộc trước khi Accepted)*

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D6? | *(chờ anh Hoàng)* |
| Q2 | Chọn phương án? | O2 |
| Q3 | Sample có chuyển sang dùng script này luôn không, hay giữ nguyên `EnsureLinkedFrontendDeps`? | *(mặc định: giữ nguyên, ngoài phạm vi ADR này — chờ xác nhận)* |

Sau khi chốt: cập nhật **Trạng thái** → 🟡 Accepted, ghi ngày Accept.

---

## 8. Test cases *(bắt buộc trước khi 🟢 Implemented)*

### 8.1 Smoke *(bắt buộc ≥ 1)*

| # | Case | Expect | Nơi (file / filter) | Trạng thái |
|---|------|--------|---------------------|------------|
| T1 | Gọi `node build-linked-frontend.mjs <path @platform/core>` trực tiếp (không qua .NET) | Build thành công, `dist/` cập nhật timestamp mới | `platform/tools/frontend/` (chạy thủ công hoặc smoke script) | 🔴 |

### 8.2 Regression — test cũ cần giữ xanh *(bắt buộc)*

| # | Case | Expect | Nơi (test hiện có) | Trạng thái |
|---|------|--------|--------------------|------------|
| T2 | `Sample.csproj` build vẫn xanh, không bị ảnh hưởng | `EnsureLinkedFrontendDeps` không đổi, Sample build thành công như cũ | `erp_core/platform/Sample` | 🔴 |

### 8.3 Test mới

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T3 | 2 thư mục giả (moduleA phụ thuộc moduleB qua `file:`), truyền argv theo thứ tự **ngược** (B, A) | Script build B trước A (chứng minh dựa vào graph, không dựa argv order) | thư mục test tạm / fixture | 🔴 |
| T4 | Cycle giả (A↔B qua `file:`) | Script `exit(1)`, in rõ chu trình, **không** build module nào | thư mục test tạm / fixture | 🔴 |

### 8.4 Done khi

- T1, T2 🟢 (bắt buộc).
- T3, T4 🟢 (case mới cho semantic topo-sort/cycle).

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Status → Accepted | — |
| 1 | Viết `build-linked-frontend.mjs` + T1, T3, T4 xanh | §8.1, §8.3 🟢 | 0 |
| 2 | Xác nhận Sample không bị ảnh hưởng (T2) | §8.2 🟢 | 1 |
| 3 | Host (Lexora) gọi script — chi tiết ở ADR sản phẩm riêng (repo `lexora`) | ADR sản phẩm Accepted + Implemented | 1 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Script + T1, T3, T4 xanh | 🔴 |
| 2 | Sample không bị ảnh hưởng (T2) | 🔴 |
| 3 | ADR + [README index](./README.md) cập nhật | 🔴 |

---

## 11. Tham chiếu thêm

- ADR sản phẩm Lexora (repo `lexora`, submodule `documents`): `documents/staging/ADRs/proposed_2026-10-14_lexora-host-build-linked-frontend-modules.md`
- `erp_core/platform/Sample/Sample.csproj` — target `EnsureLinkedFrontendDeps`, pattern tham khảo (chỉ `npm install`, không build)
