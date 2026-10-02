# CraftDoc (DOCX Template Builder)

Tạo mẫu DOCX với placeholder `{{field}}`, điền dữ liệu form, xem trước và xuất file đã điền.

---

## Export chính

```ts
import {
  CraftDocBuilderPage,
  CRAFT_DOC_ROUTES,
  configureCraftDocNavigate,
  callLoadCraftDocTemplateFromApi,
  callUploadCraftDocTemplate,
  callSaveCraftDocDocument,
  callExportCraftDocDocument,
  callGetCraftDocTemplateDetail,
  getCraftDocTemplatesMock,
  resolveCraftDocContent,
  toPlaceholderMap,
  applyValuesMapToFields,
  ensureFieldValues,
  useCraftDocBuilderState,
  buildCraftDocController,
  DocxTemplateBuilder,
  type DocumentField,
  type DocumentTemplate,
  type CraftDocBuilderPageContentContext,
  type CraftDocSavePayload,
} from '@platform/core'
```

---

## CSS (bắt buộc)

```css
@import "@platform/core/craftDoc.css";
```

Peer: `docx-preview`, `pizzip` (đã bundle trong `@platform/core`).

---

## Tích hợp

```tsx
import {
  CraftDocBuilderPage,
  CRAFT_DOC_ROUTES,
  configureCraftDocNavigate,
} from '@platform/core'

configureCraftDocNavigate((to) => navigate(to))

<Route path={CRAFT_DOC_ROUTES.builder} element={<CraftDocBuilderPage locale="vi" />} />
```

Route mặc định: `/craft-doc`.

---

## Mô hình dữ liệu — `fields[].value`

Dữ liệu form **không** còn prop `sampleData` riêng. Mọi giá trị người dùng điền nằm trên `**templates[].fields[].value`**.

```ts
type DocumentField = {
  id: string
  name: string              // key placeholder, vd: "staff"
  label: string             // nhãn form
  type: 'text' | 'number' | 'date' | 'currency' | 'textarea' | 'select' | 'checkbox'
  placeholder: string       // vd: "{{staff}}"
  required: boolean
  value?: string            // ← giá trị người dùng điền (nguồn chính)
  defaultValue?: string
  description?: string
  group?: string
  placeholderText?: string  // hint trong input
  options?: { label: string; value: string }[]
}

type DocumentTemplate = {
  id: string
  name: string
  fileName: string
  file: Blob
  fields: DocumentField[]
  updatedAt?: string
}
```

**Preview / export:** chỉ field có `value` không rỗng mới thay `{{placeholder}}`; field trống giữ nguyên placeholder trong preview.

**API legacy:** BE vẫn có thể trả `sampleData: Record<string, string>` — FE merge vào `fields[].value` qua `applyValuesMapToFields`.

---

## Hành vi page

- Mặc định có sẵn mẫu **Hợp đồng lao động** (`labor-contract`); form **bắt đầu trống** (không pre-fill dữ liệu mẫu).
- Layout **3:7** — form trái / preview phải.
- Upload `.docx`, chỉnh sửa placeholder (drawer), chọn mẫu (dialog).
- **Lưu** — stub `callSaveCraftDocDocument` (host override qua `callback.save`).
- **Xuất** — tải DOCX đã điền qua `callExportCraftDocDocument` (client-side).
- Mẫu đã biết (`labor-contract`, `lla-idas`) dùng metadata từ `fieldMeta.ts`; upload khác tự group field động.

---

## Tùy biến

### Logic — `callback`


| Key              | Mặc định                                |
| ---------------- | --------------------------------------- |
| `upload`         | `callUploadCraftDocTemplate`            |
| `save`           | `callSaveCraftDocDocument`              |
| `export`         | `callExportCraftDocDocument` (download) |
| `createTemplate` | Tạo mẫu trống cục bộ                    |


```tsx
<CraftDocBuilderPage
  locale="vi"
  callback={{
    upload: {
      onSubmit: async (file) => {
        const form = new FormData()
        form.append('file', file)
        const res = await api.post('/v1/craft-doc/templates/upload', form)
        // Trả DocumentTemplate — fields[] có thể kèm value
        return res.data
      },
      success: () => notify.success('Đã tải mẫu'),
    },
    save: {
      before: ({ template }) => (template ? undefined : false),
      onSubmit: async (payload) => {
        // payload.fields — nguồn chính
        await api.put(`/documents/${payload.template.id}`, {
          fields: payload.fields,
        })
        return callSaveCraftDocDocument(payload)
      },
    },
    export: {
      onSubmit: (payload) => callExportCraftDocDocument(payload),
    },
  }}
/>
```

Mỗi key trong `callback` là `ActionProps`: `before` → `onSubmit` → (page side-effect) → `success` → `complete`; lỗi: `error` → `complete`. `before` return `false` để hủy.

### Controlled mode

Host tự quản lý state mẫu và dữ liệu form qua `templates[].fields[].value`:

```tsx
const [templates, setTemplates] = useState<DocumentTemplate[]>([])
const [activeTemplateId, setActiveTemplateId] = useState<string | null>(null)

<CraftDocBuilderPage
  locale="vi"
  templates={templates}
  activeTemplateId={activeTemplateId}
  loading={loading}
  onTemplatesChange={setTemplates}
  onActiveTemplateChange={setActiveTemplateId}
/>
```

Khi user sửa form, `onTemplatesChange` nhận `DocumentTemplate[]` với `fields[].value` đã cập nhật.

Khi truyền `templates`, page **không** tự khởi tạo mẫu demo — host phải cung cấp `DocumentTemplate[]` (kèm `Blob` file).

**Load từ BE:**

```tsx
import { callLoadCraftDocTemplateFromApi } from '@platform/core'

useEffect(() => {
  callLoadCraftDocTemplateFromApi('labor-contract').then((template) => {
    setTemplates([template])
    setActiveTemplateId(template.id)
  })
}, [])
```

**Cập nhật một field từ host app:**

```tsx
function updateFieldValue(fieldName: string, value: string) {
  setTemplates((prev) =>
    prev.map((t) =>
      t.id !== activeTemplateId
        ? t
        : {
            ...t,
            fields: t.fields.map((f) =>
              f.name === fieldName ? { ...f, value } : f,
            ),
          },
    ),
  )
}
```

### Giao diện — `content`

`content` nhận `ReactNode` hoặc render function `(ctx) => ReactNode`.

Context `CraftDocBuilderPageContentContext`:


| Key                                | Mô tả                                                           |
| ---------------------------------- | --------------------------------------------------------------- |
| `templates`                        | Danh sách mẫu                                                   |
| `activeTemplate`                   | Mẫu đang chọn                                                   |
| `fields`                           | `**activeTemplate.fields**` — mỗi item có `value`               |
| `sampleData`                       | **Deprecated** — map placeholder derived; dùng `fields[].value` |
| `previewBlob`                      | Blob preview hiện tại                                           |
| `status`                           | `'idle' | 'loading' | 'parsing' | 'ready' | 'error'`            |
| `loading`, `saving`, `dirty`       | Trạng thái UI                                                   |
| `reload`, `save`, `exportDocument` | Actions                                                         |
| `DefaultLayout`                    | Layout builder mặc định (form + preview)                        |


**Lưu ý:** `content` **không** expose `setFieldValue`. Sửa value qua form mặc định (`DefaultLayout`) hoặc controlled `onTemplatesChange`.

#### Ví dụ — header + layout mặc định

```tsx
<CraftDocBuilderPage
  locale="vi"
  title="Soạn hợp đồng"
  withShell
  content={(ctx) => (
    <>
      <div className="mb-2 flex gap-4 text-sm text-muted-foreground">
        <span>Mẫu: {ctx.activeTemplate?.name ?? '—'}</span>
        <span>{ctx.fields.length} trường</span>
        <span>
          Điền:{' '}
          {ctx.fields.filter((f) => (f.value ?? '').trim()).length}/{ctx.fields.length}
        </span>
        {ctx.dirty && <span className="text-amber-600">Chưa lưu</span>}
      </div>
      {ctx.DefaultLayout}
    </>
  )}
/>
```

#### Ví dụ — sidebar đọc `ctx.fields`

```tsx
<CraftDocBuilderPage
  locale="vi"
  content={(ctx) => (
    <div className="flex h-full min-h-0 gap-4">
      <aside className="w-64 shrink-0 overflow-auto border-r p-3">
        <h3 className="mb-2 font-medium">Dữ liệu form</h3>
        <ul className="space-y-2 text-sm">
          {ctx.fields.map((field) => (
            <li key={field.id}>
              <div className="font-medium">{field.label}</div>
              <div className="text-muted-foreground">
                {field.value?.trim() || '(trống)'}
              </div>
            </li>
          ))}
        </ul>
      </aside>
      <div className="min-w-0 flex-1">{ctx.DefaultLayout}</div>
    </div>
  )}
/>
```

#### Ví dụ — toolbar custom + save/export

```tsx
<CraftDocBuilderPage
  locale="vi"
  callback={{ save: { onSubmit: async (payload) => { /* persist payload.fields */ } } }}
  content={(ctx) => (
    <>
      <div className="flex justify-end gap-2 border-b p-2">
        <button type="button" disabled={ctx.saving} onClick={() => void ctx.save()}>
          {ctx.saving ? 'Đang lưu...' : 'Lưu'}
        </button>
        <button type="button" onClick={() => void ctx.exportDocument()}>
          Xuất DOCX
        </button>
      </div>
      {ctx.DefaultLayout}
    </>
  )}
/>
```

#### Chỉ layout mặc định

```tsx
<CraftDocBuilderPage content={(ctx) => ctx.DefaultLayout} />
```


| Prop                                | Tác dụng                                                   |
| ----------------------------------- | ---------------------------------------------------------- |
| `withShell={false}`                 | Bỏ `CraftDocPageShell` (mặc định — dùng trong AdminLayout) |
| `withShell`                         | Hiện title/description qua shell                           |
| `title`, `description`, `className` | Tuỳ chỉnh shell / wrapper                                  |
| `locale`                            | `'vi'` | `'en'`                                            |
| `initialTemplateId`                 | Mẫu mặc định khi không controlled (`labor-contract`)       |


---

## Props dự án khác phải custom

### `CraftDocBuilderPage`


| Prop                               | Ý nghĩa                        | Ví dụ                                              |
| ---------------------------------- | ------------------------------ | -------------------------------------------------- |
| `callback.upload.onSubmit`         | Upload mẫu lên BE              | `(file) => callUploadCraftDocTemplate(file)`       |
| `callback.save.onSubmit`           | Persist fields + mẫu           | `(payload) => api.save(payload.fields)`            |
| `callback.export.onSubmit`         | Xuất file (hoặc trả URL)       | `(payload) => callExportCraftDocDocument(payload)` |
| `callback.createTemplate.onSubmit` | Tạo mẫu mới trên BE            | `({ name }) => api.createTemplate(name)`           |
| `templates` + `onTemplatesChange`  | Controlled — host sở hữu state | React Query / Redux                                |
| `content`                          | Tuỳ biến UI; đọc `ctx.fields`  | Xem ví dụ trên                                     |


### Payload types

```ts
type CraftDocSavePayload = {
  template: DocumentTemplate
  /** Fields kèm value — nguồn dữ liệu form chính. */
  fields: DocumentField[]
  /** @deprecated Dùng fields[].value — derived khi save/export. */
  sampleData?: Record<string, string>
}

type CraftDocExportPayload = CraftDocSavePayload
```

### Contract BE (khuyến nghị)

**GET** `v1/craft-doc/templates/:id`:

```json
{
  "template": { "id": "labor-contract", "name": "...", "fileName": "..." },
  "fields": [
    {
      "id": "f1",
      "name": "staff",
      "label": "Họ và tên",
      "type": "text",
      "placeholder": "{{staff}}",
      "required": true,
      "group": "employee",
      "value": ""
    }
  ],
  "sampleData": { "staff": "Nguyễn Văn A" }
}
```

Ưu tiên gửi `value` trên từng field. `sampleData` vẫn được merge nếu BE chưa migrate.

**POST** `v1/craft-doc/templates/upload` — response:

```json
{
  "id": "upload-123",
  "name": "Hợp đồng",
  "fileName": "hop-dong.docx",
  "fileBase64": "...",
  "fields": [{ "id": "...", "name": "...", "value": "" }],
  "sampleData": {}
}
```

DOCX không có placeholder `{{}}` vẫn dùng được nếu BE trả `fields[]` mô tả vùng điền.

---

## Migration từ model cũ


| Trước                           | Bây giờ                               |
| ------------------------------- | ------------------------------------- |
| `sampleData={{ staff: '...' }}` | `templates[].fields[].value`          |
| `onSampleDataChange`            | `onTemplatesChange`                   |
| Save gửi `sampleData`           | Save gửi `payload.fields`             |
| BE trả `sampleData` map         | BE trả `fields[].value` (hoặc cả hai) |
| `ctx.sampleData` trong content  | `ctx.fields`                          |


---

## Dùng builder độc lập (không page)

Khi cần form custom + `setFieldValue` trực tiếp:

```tsx
import {
  useCraftDocBuilderState,
  buildCraftDocController,
  DocxTemplateBuilder,
} from '@platform/core'

function MyCraftDoc() {
  const state = useCraftDocBuilderState({ initialTemplateId: 'labor-contract' })
  const controller = buildCraftDocController({
    state,
    onSave: () => state.saveTemplate(),
    onExport: () => void state.exportDocument(),
    onUploadFile: (file) => state.uploadTemplate(file),
    onCreateTemplate: (name) => state.createBlankTemplate(name),
  })

  // state.activeTemplate?.fields — read
  // controller.setFieldValue(name, value) — write

  return <DocxTemplateBuilder locale="vi" controller={controller} />
}
```

Component con nhận props rõ ràng (không phụ thuộc hook nội bộ):

- `DocumentDataForm` — `CraftDocFormPanelProps` (đọc `field.value`)
- `DocumentPreviewPane` — `CraftDocPreviewPanelProps`
- `TemplateDesignDrawer` — `CraftDocTemplateDesignDrawerProps`

---

## Utils


| Hàm                                             | Mục đích                                                |
| ----------------------------------------------- | ------------------------------------------------------- |
| `toPlaceholderMap(fields)`                      | Map `name → value` cho engine DOCX (bỏ qua field trống) |
| `applyValuesMapToFields(fields, sampleData?)`   | Merge API legacy `sampleData` vào `fields[].value`      |
| `ensureFieldValues(fields, defaults?)`          | Gán value mặc định cho từng field                       |
| `mergeParsedFieldsWithValues(parsed, previous)` | Giữ value cũ sau khi parse lại template                 |
| `resolveCraftDocContent`                        | Render slot `content`                                   |
| `generateDocument`                              | Thay placeholder trong buffer DOCX                      |
| `extractFields`                                 | Parse field từ XML                                      |
| `createSampleDocxBlob`                          | Tạo mẫu demo                                            |


Mock metadata: `getCraftDocTemplatesMock()` — chỉ id/name/fileName, không có Blob.

---

## API HTTP


| Service                             | Method | Path                                     |
| ----------------------------------- | ------ | ---------------------------------------- |
| `callGetCraftDocTemplates`          | GET    | `v1/craft-doc/templates`                 |
| `callGetCraftDocTemplateDetail`     | GET    | `v1/craft-doc/templates/:id`             |
| `callGetCraftDocTemplateSampleData` | GET    | `v1/craft-doc/templates/:id/sample-data` |
| `callLoadCraftDocTemplateFromApi`   | GET    | detail + `.../file` (parallel)           |
| `callUploadCraftDocTemplate`        | POST   | `v1/craft-doc/templates/upload`          |
| `callSaveCraftDocDocument`          | —      | Stub (host override)                     |
| `callExportCraftDocDocument`        | —      | Client-side download                     |


```ts
// Load đầy đủ template + fields (value merged) + Blob file
const loaded = await callLoadCraftDocTemplateFromApi('lla-idas')

// Chỉ metadata + fields (không tải file)
const detail = await callGetCraftDocTemplateDetail('lla-idas')

// Offline / unit test
const mock = getLlaIdasTemplateDetailMock()
```

---

## Giả lập API (dev / demo — Sample)

Plugin `Sample/clients/web/vite.plugins/craftDocApiMock.ts` — bật khi `VITE_USE_MOCK=true`:


| Method | Path                                          | Response seed                         |
| ------ | --------------------------------------------- | ------------------------------------- |
| GET    | `v1/craft-doc/templates`                      | `mocks/get-templates-list.json`       |
| GET    | `v1/craft-doc/templates/lla-idas`             | `mocks/get-lla-idas-template.json`    |
| GET    | `v1/craft-doc/templates/lla-idas/sample-data` | `{ sampleData }` từ cùng file         |
| GET    | `v1/craft-doc/templates/lla-idas/file`        | `mocks/assets/LLA-IDAS.docx`          |
| POST   | `v1/craft-doc/templates/upload`               | Trả id + fileBase64 + fields (~700ms) |


**Kích thước trang LLA-IDAS:** US Letter `12240×16840` twips, lề `1440` twips (1 inch) → preview ~`816×1056`px @ 96 DPI. Hằng số:`CRAFT_DOC_PAGE_LETTER`.

**Lưu ý:** File `LLA-IDAS.docx` gốc không có placeholder `{{field}}` — các field trong mock mô tả nội dung tài liệu để form/API demo; preview filled không thay thế text trừ khi thêm placeholder vào DOCX.

---

## Ví dụ end-to-end (controlled + BE)

```tsx
import { useEffect, useState } from 'react'
import {
  CraftDocBuilderPage,
  callLoadCraftDocTemplateFromApi,
  type DocumentTemplate,
} from '@platform/core'

export function ContractEditorPage({ contractId }: { contractId: string }) {
  const [templates, setTemplates] = useState<DocumentTemplate[]>([])
  const [activeId, setActiveId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    callLoadCraftDocTemplateFromApi(contractId)
      .then((t) => {
        setTemplates([t])
        setActiveId(t.id)
      })
      .finally(() => setLoading(false))
  }, [contractId])

  return (
    <CraftDocBuilderPage
      locale="vi"
      title="Soạn hợp đồng"
      templates={templates}
      activeTemplateId={activeId}
      loading={loading}
      onTemplatesChange={setTemplates}
      onActiveTemplateChange={setActiveId}
      content={(ctx) => (
        <>
          <p className="text-sm text-muted-foreground">
            {ctx.fields.filter((f) => f.required && !(f.value ?? '').trim()).length}{' '}
            trường bắt buộc còn trống
          </p>
          {ctx.DefaultLayout}
        </>
      )}
      callback={{
        save: {
          onSubmit: async ({ template, fields }) => {
            await fetch(`/api/contracts/${template.id}`, {
              method: 'PUT',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ fields }),
            })
            return { saved: true, templateId: template.id }
          },
        },
      }}
    />
  )
}
```

