# @platform/importexport

Reusable React UI for Platform ImportExport (template CRUD + import/export).

## Structure

- `src/lib` — library (`@platform/importexport`)
- `src/app` — Vite demo shell (port **5174**, proxies `/ie-api` → `http://localhost:5251`)

## Dev shell

```bash
cd modules/importexport/frontend
npm install
npm run dev
```

Run `Platform.Modules.ImportExport.Api` on `:5251`. Optional `.env.local`:

- `VITE_IMPORT_EXPORT_API_KEY` → header `X-Excel-Engine-Key`
- `VITE_TENANT_ID` → header `X-Tenant-Id` (host → `ICurrentTenant`)
  - Interim default in `.env`: `00000000-0000-0000-0000-000000000001`

## Consume in Sample / host

```ts
import {
  configureImportExportHttp,
  configureImportExportNavigate,
  TemplateListPage,
  ImportWorkflowPage,
  ExcelImportPage,
  KitFileUpload,
  IMPORT_EXPORT_ROUTES,
} from '@platform/importexport'
```

Import workflow dùng `KitFileUpload` + `ImportPageShell` từ `@platform/core` (re-export qua module). Kết quả validate/import hiển thị bằng `ImportResultPanel` (schema động từ API).

- `ImportWorkflowPage` — trang generic: truyền `callbacks.validate` / `callbacks.commit` trả `ImportResult`
- `ExcelImportPage` — bọc sẵn `validateExcelImport` / `importExcel` theo `templateCode`

```json
"@platform/importexport": "file:../../../modules/importexport/frontend"
```

Alias Vite to `src/lib` for HMR (see Sample `vite.config.ts`).
