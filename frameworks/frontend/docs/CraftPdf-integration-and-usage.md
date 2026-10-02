# Craft PDF

Editor template PDF kiểu CraftMyPDF: kéo text, table, QR, barcode; binding `{{data.field}}`; preview PDF.

---

## Export chính

```ts
import {
  CraftPdfTemplateListPage,
  CraftPdfEditorPage,
  CRAFT_PDF_ROUTES,
  configureCraftPdfNavigate,
  callGetPdfTemplateList,
  getPdfTemplatesMock,
  getCraftPdfDataFieldsMock,
} from '@platform/core'
```

---

## CSS

```css
@import "@platform/core/styles.css";
```

Peer: `react-pdf`, `pdfjs-dist`, `quill`, `react-rnd`.

Khởi tạo worker PDF (editor tự import side-effect):

```ts
import { configureCraftPdfWorker } from '@platform/core'
configureCraftPdfWorker('/pdf.worker.min.mjs')
```

---

## Tích hợp routes

```tsx
configureCraftPdfNavigate((to) => navigate(to))

<Route path={CRAFT_PDF_ROUTES.list} element={<CraftPdfTemplateListPage />} />
<Route path={CRAFT_PDF_ROUTES.editor} element={<CraftPdfEditorPage templateId={id} />} />
```

Paths: list, create, editor `:id`, preview.

---

## API vs memory mock

`callGetPdfTemplateList` / CRUD:

- Có `craftPdfHttp.defaults.baseURL` → gọi BE `/templates`
- Không có → dùng **memory store** (`createMockTemplates()`)

Seed JSON: `features/craftPdf/mocks/get-templates.json`, `get-data-fields.json`.

Data fields palette: `getFakeCraftPdfDataFields()`.

---

## Tùy biến

### Logic — `callback`

**CraftPdfTemplateListPage**

| Key | Mặc định |
|-----|----------|
| `create` | `callCreatePdfTemplate` / memory |
| `edit` | navigate editor |
| `delete` | `callDeletePdfTemplate` / memory |

**CraftPdfEditorPage**

| Key | Mặc định |
|-----|----------|
| `save` | lưu template |
| `generate` | `callGeneratePdf` — cần BE |
| `back` | navigate list |

```tsx
<CraftPdfEditorPage
  templateId={id}
  callback={{
    save: { onSubmit: (payload) => mySaveTemplate(payload) },
    generate: {
      onSubmit: ({ template, data }) => callGeneratePdf(template.id, data),
      success: ({ result }) => downloadBlob(result.pdf),
    },
  }}
/>
```

### Giao diện — `content`

List — context: `DefaultToolbar`, `DefaultGrid`, `DefaultContent`, `onCreate`, `requestDelete`, `reload`.

```tsx
<CraftPdfTemplateListPage
  content={(ctx) => (
    <>
      {ctx.DefaultToolbar}
      {ctx.DefaultGrid}
    </>
  )}
/>

<CraftPdfEditorPage
  templateId={id}
  content={(ctx) => ctx.DefaultEditor}
/>
```

---

## Props dự án khác phải custom

Host gắn CRUD template / generate PDF / điều hướng qua `callback`. Mỗi key là `ActionProps` (`before` / `onSubmit` / `success` / `error` / `complete`).

### `CraftPdfTemplateListPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.create.onSubmit` | Tạo template mới trên BE | `async ({ name }) => callCreatePdfTemplate({ name })` | User bấm Tạo → trả `PdfTemplate` (thường rồi navigate editor) |
| `callback.create.success` | Sau tạo OK | `({ result }) => navigate(\`/craft-pdf/${result.id}\`)` | Mở editor với id vừa tạo |
| `callback.edit.onSubmit` | Mở sửa (thường chỉ navigate) | `async (template) => { navigate(\`/craft-pdf/${template.id}\`) }` | Click sửa trên grid |
| `callback.delete.before` | Confirm trước khi xóa | `({ template }) => window.confirm(\`Xóa ${template.name}?\`) ? undefined : false` | `false` = hủy xóa |
| `callback.delete.onSubmit` | Xóa template trên BE | `async (template) => callDeletePdfTemplate(template.id)` | Sau confirm → gọi API xóa |
| `items` + `loading` | Controlled list | host tự fetch rồi truyền mảng | Page **không** tự load list |

```tsx
async function createTemplate({ name }: { name: string }) {
  // Tạo template trên BE, trả entity để page/success dùng
  return callCreatePdfTemplate({ name })
}

async function openEditor(template: PdfTemplate) {
  // Không gọi API — chỉ điều hướng sang editor
  navigate(`/craft-pdf/${template.id}`)
}

async function deleteTemplate(template: PdfTemplate) {
  await callDeletePdfTemplate(template.id)
}

<CraftPdfTemplateListPage
  locale="vi"
  callback={{
    create: {
      onSubmit: createTemplate,
      success: ({ result }) => navigate(`/craft-pdf/${result.id}`),
    },
    edit: { onSubmit: openEditor },
    delete: {
      before: ({ template }) =>
        window.confirm(`Xóa ${template.name}?`) ? undefined : false,
      onSubmit: deleteTemplate,
    },
  }}
/>
```

### `CraftPdfEditorPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `templateId` | Id template cần load | `params.id` từ route | Page load template nếu không truyền `template` |
| `callback.save.onSubmit` | Lưu layout/elements lên BE | `async (template) => callUpdatePdfTemplate(template.id, template)` | Bấm Lưu |
| `callback.save.success` | Toast sau lưu | `() => notify.success('Đã lưu')` | Sau API save OK |
| `callback.generate.onSubmit` | Xuất PDF từ template + data | `async (request) => callGeneratePdf(id, request)` | Bấm Generate / Preview PDF |
| `callback.generate.success` | Tải / mở file PDF | `({ result }) => window.open(result.pdfUrl)` | Nhận blob/url từ `onSubmit` |
| `callback.back.onSubmit` | Quay danh sách | `async () => navigate('/craft-pdf')` | Bấm Back |
| `dataFields` | Schema field bind `{{data.x}}` | mảng từ API metadata | Palette field bên editor |
| `previewFileUrl` | URL PDF xem trước | `URL.createObjectURL(blob)` | Panel preview react-pdf |

```tsx
async function saveTemplate(template: PdfTemplate) {
  // Persist toàn bộ template (elements, page size, …)
  await callUpdatePdfTemplate(template.id, template)
}

async function generatePdf(request: GeneratePdfRequest) {
  // BE render PDF; trả url hoặc blob tùy contract
  return callGeneratePdf(id, request)
}

async function goBackToList() {
  navigate('/craft-pdf')
}

<CraftPdfEditorPage
  templateId={id}
  dataFields={schemaFromApi}
  previewFileUrl={blobUrl}
  callback={{
    save: {
      onSubmit: saveTemplate,
      success: () => notify.success('Đã lưu'),
    },
    generate: {
      onSubmit: generatePdf,
      success: ({ result }) => {
        if (result && 'pdfUrl' in result) window.open(result.pdfUrl)
      },
    },
    back: { onSubmit: goBackToList },
  }}
/>
```

### Props UI tùy chọn

`locale`, `title`, `description`, `className`, `content`, `template` (controlled) — không bắt buộc để gắn API.

### HTTP riêng

```ts
import { configureCraftPdfHttp } from '@platform/core'
configureCraftPdfHttp({ baseURL: import.meta.env.VITE_API_URL_CRAFT_PDF })
```

Không set baseURL → memory store + mock JSON.

---

## Ghi chú

- Template status: `draft` | `published`
- Page size mặc định A4 (`PDF_PAGE_A4`)
- Side-effect `ensureBrowserProcess` trong package exports
