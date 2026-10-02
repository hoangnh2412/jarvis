# Import

Wizard import dữ liệu từ Excel/CSV: upload → validate → xem preview lỗi/hợp lệ → commit.

**Mặc định dùng in-memory mock** (`mockValidateImport`, `mockCommitImport`).

---

## Export chính

```ts
import {
  ImportPage,
  ImportStudentPage,
  IMPORT_ROUTES,
  configureImportNavigate,
  mockValidateImport,
  mockCommitImport,
  getImportValidateResultMock,
} from '@platform/core'
```

---

## CSS

```css
@import "@platform/core/styles.css";
```

---

## Tích hợp

```tsx
configureImportNavigate((to) => navigate(to))

<Route path={IMPORT_ROUTES.page} element={<ImportPage locale="vi" />} />
```

Route mặc định: `/import`.

Có thêm `ImportStudentPage` cho flow import sinh viên (mock riêng: `mockValidateStudentImport`, `mockImportStudents`).

---

## Luồng sử dụng

1. **Chọn file** — `.csv`, `.xlsx`, `.xls` (tối đa 100MB mặc định)
2. **Validate** — gọi `mockValidateImport` hoặc callback
3. **Xem kết quả** — `ImportValidationPanel` hiển thị dòng hợp lệ/lỗi
4. **Confirm** — commit khi `invalidCount === 0` và có dòng hợp lệ

---

## Tùy biến

### Logic — `callback`

| Key | Mặc định |
|-----|----------|
| `validate` | `mockValidateImport` |
| `commit` | `mockCommitImport` |

```tsx
<ImportPage
  callback={{
    validate: {
      before: () => { if (!hasFile) return false },
      onSubmit: async ({ file }) => {
        const form = new FormData()
        form.append('file', file)
        const res = await axios.post('/api/v1/import/validate', form)
        return res.data
      },
    },
    commit: {
      onSubmit: async ({ file }) => {
        const form = new FormData()
        form.append('file', file)
        return (await axios.post('/api/v1/import/commit', form)).data
      },
      success: ({ result }) => notify.success(`Đã import ${result.importedCount} dòng`),
    },
  }}
  onConfirmed={(result) => refetchList()}
/>
```

### Giao diện

Context `ImportPageContentContext` — dùng với prop `content`:

```tsx
<ImportPage
  title="Nhập dữ liệu"
  accept=".xlsx,.csv"
  maxFileSize={50_000_000}
  backPath="/admin"
  withShell={false}
  content={(ctx) => ctx.DefaultContent}
/>
```

| Prop | Tác dụng |
|------|----------|
| `title`, `description` | Tiêu đề trang |
| `accept` | MIME/extension FileUpload |
| `maxFileSize` | Giới hạn dung lượng |
| `backPath` | Nút quay lại |
| `withShell` | Bọc `ImportPageShell` (mặc định `true`) |
| `content` | Thay / ghép nội dung wizard |

---

## Props dự án khác phải custom

Production **bắt buộc** override `callback.validate` / `callback.commit` (mặc định là mock). `onConfirmed` dùng để host refetch list sau khi import xong.

### `ImportPage` / `ImportStudentPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.validate.onSubmit` | Parse + validate file trước khi nhập | `async ({ file }) => (await callValidateImport({ file })).data` | Sau chọn file → hiện bảng dòng OK / lỗi |
| `callback.validate.before` | Chặn validate nếu file không hợp lệ | `({ file }) => /\.(csv\|xlsx)$/i.test(file.name) ? undefined : false` | `false` = không gọi API |
| `callback.commit.onSubmit` | Commit nhập thật vào hệ thống | `async ({ file }) => (await callCommitImport({ file })).data` | Bấm Xác nhận khi không còn dòng lỗi |
| `callback.commit.success` | Toast số dòng đã nhập | `({ result }) => notify.success(\`Đã nhập ${result.importedCount}\`)` | Sau commit OK |
| `onConfirmed` | Host làm việc sau import (refetch, đóng drawer) | `(result) => { refetchStudents(); closeDrawer() }` | Chạy sau commit thành công (ngoài `callback.success`) |
| `onCancelled` | User hủy / xóa file đã chọn | `() => setDraft(null)` | Reset state host nếu cần |
| `accept` | Loại file cho phép | `".xlsx,.csv"` | Attribute `accept` của FileUpload |
| `maxFileSize` | Giới hạn dung lượng (byte) | `50_000_000` | File lớn hơn → reject trước validate |
| `backPath` | Path nút quay lại | `"/students"` | `navigateImport(backPath)` |

```tsx
async function validateImportFile({ file }: { file: File }) {
  // Gửi file lên BE parse → trả validRows / invalidRows
  const form = new FormData()
  form.append('file', file)
  const res = await axios.post('/api/v1/import/validate', form)
  return res.data // ImportValidationResult
}

async function commitImportFile({ file }: { file: File }) {
  // Ghi dữ liệu hợp lệ vào DB
  const form = new FormData()
  form.append('file', file)
  const res = await axios.post('/api/v1/import/commit', form)
  return res.data // ImportCommitResult
}

function afterImportConfirmed(result: ImportCommitResult) {
  // Host: refresh danh sách nghiệp vụ + đóng wizard
  refetchStudents()
  notify.success(result.message ?? `Đã nhập ${result.importedCount}`)
}

function onImportCancelled() {
  // User bỏ chọn file — reset draft phía host nếu có
  console.log('cleared file')
}

<ImportPage
  locale="vi"
  title="Nhập học viên"
  accept=".xlsx,.csv"
  maxFileSize={50_000_000}
  backPath="/students"
  callback={{
    validate: {
      before: ({ file }) =>
        /\.(csv|xlsx)$/i.test(file.name) ? undefined : false,
      onSubmit: validateImportFile,
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
    commit: {
      onSubmit: commitImportFile,
      success: ({ result }) => {
        notify.success(result.message ?? `Đã nhập ${result.importedCount}`)
      },
    },
  }}
  onConfirmed={afterImportConfirmed}
  onCancelled={onImportCancelled}
/>
```

### Props UI tùy chọn

`title`, `description`, `className`, `backLabel`, `content`, `withShell`, `showBackButton`, `locale`.

---

## Types

```ts
ImportValidationResult  // validCount, invalidCount, validRows, invalidRows
ImportCommitResult        // importedCount, message
ImportValidatePayload     // { file: File }
ImportCommitPayload       // { file: File }
```

Mock seed: `features/import/mocks/get-validate-result.json`.

---

## Components lẻ

`ImportPageShell`, `ImportPreviewTable`, `ImportValidationPanel`.

