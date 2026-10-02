# File Manager

Quản lý file/folder dạng tree: upload, tạo folder, rename, move, delete. **Mặc định dùng in-memory mock** — phù hợp demo không cần BE.

---

## Export chính

```ts
import {
  FileManagerPage,
  FILE_MANAGER_ROUTES,
  configureFileManagerNavigate,
  mockListFiles,
  mockUploadFiles,
  mockCreateFolder,
  mockDeleteEntry,
  mockRenameEntry,
  mockMoveEntry,
  getFilesMock,
  FAKE_FILE_ENTRIES,
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
configureFileManagerNavigate((to) => navigate(to))

<Route path={FILE_MANAGER_ROUTES.list} element={<FileManagerPage locale="vi" />} />
```

Route mặc định: `/files`.

---

## Mock mặc định

Page gọi các service `mock*`:

| Service | Chức năng |
|---------|-----------|
| `mockListFiles` | Liệt kê file/folder theo path |
| `mockUploadFiles` | Upload vào folder hiện tại |
| `mockCreateFolder` | Tạo folder mới |
| `mockRenameEntry` | Đổi tên |
| `mockMoveEntry` | Di chuyển |
| `mockDeleteEntry` | Xóa |
| `mockResetFiles` | Reset về seed ban đầu |

Seed JSON: `features/fileManager/mocks/get-files.json` → `getFilesMock()`.

Storage quota demo: `FAKE_STORAGE`.

---

## Tùy biến

### Logic — `callback`

Mặc định dùng `mock*`. Override khi có BE (`modules/files`):

| Key | Mặc định |
|-----|----------|
| `list` | `mockListFiles` |
| `upload` | `mockUploadFiles` |
| `createFolder` | `mockCreateFolder` |
| `delete` | `mockDeleteEntry` |
| `rename` | `mockRenameEntry` |
| `move` | `mockMoveEntry` |

```tsx
<FileManagerPage
  callback={{
    list: {
      onSubmit: async ({ path, filter, sortField, sortOrder }) => {
        const res = await axios.get('/api/v1/files', {
          params: { path, filter, sortField, sortOrder },
        })
        return res.data
      },
    },
    upload: { onSubmit: (payload) => myUploadApi(payload) },
    delete: { onSubmit: ({ entry }) => myDeleteApi(entry.id) },
  }}
/>
```

### Controlled mode

```tsx
<FileManagerPage
  items={files}
  folders={folders}
  entries={allEntries}
  storage={storageInfo}
  loading={loading}
/>
```

### Giao diện — `content`

Context `FileManagerPageContentContext`: `items`, `folders`, `currentPath`, `storage`, `navigate`, `reload`, `openUpload`, `openCreateFolder`, `DefaultLayout`.

```tsx
<FileManagerPage
  locale="vi"
  content={(ctx) => (
    <>
      <StorageAlert used={ctx.storage.used} quota={ctx.storage.quota} />
      {ctx.DefaultLayout}
    </>
  )}
/>
```

---

## Components lẻ

`FileManagerSidebar`, `FileManagerToolbar`, `FileManagerTable`, `FileManagerStorageBar`, `FileRowActions`.

---

## Utils

`formatFileSize`, `getBreadcrumbSegments`, `getChildren`, `normalizePath`, …

---

## Props dự án khác phải custom

Mặc định page dùng `mock*`. Production **bắt buộc** override `callback.*.onSubmit` bằng API files thật. Mỗi key là `ActionProps`.

### `FileManagerPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.list.onSubmit` | Liệt kê file/folder theo path | `async ({ path, filter, sortField, sortOrder }) => api.list(…)` | Đổi folder / sort / filter → load bảng |
| `callback.upload.onSubmit` | Upload file vào folder hiện tại | `async ({ path, files }) => api.upload(path, files)` | Chọn file → gửi lên storage |
| `callback.createFolder.onSubmit` | Tạo thư mục mới | `async ({ path, name }) => api.mkdir(path, name)` | Dialog tạo folder |
| `callback.delete.onSubmit` | Xóa file/folder | `async (entry) => api.remove(entry.id)` | Confirm xóa |
| `callback.rename.onSubmit` | Đổi tên | `async ({ entry, name }) => api.rename(entry.id, name)` | Inline / dialog rename |
| `callback.move.onSubmit` | Di chuyển vào path khác | `async ({ entry, targetPath }) => api.move(entry.id, targetPath)` | Drag / move dialog |
| `items` + `folders` + `entries` + `storage` + `loading` | Controlled mode | host fetch rồi truyền | Page **không** gọi `list` mặc định |

```tsx
async function listFiles(payload: {
  path: string
  filter: string
  sortField: 'name' | 'size'
  sortOrder: 'asc' | 'desc'
}) {
  // Lấy danh sách theo thư mục hiện tại + sort/filter
  const res = await axios.get('/api/v1/files', { params: payload })
  return res.data // ListFilesResult: items, folders, storage, …
}

async function uploadFiles({ path, files }: { path: string; files: File[] }) {
  // Upload nhiều file vào path
  const form = new FormData()
  files.forEach((f) => form.append('files', f))
  form.append('path', path)
  const res = await axios.post('/api/v1/files/upload', form)
  return res.data // FileEntry[]
}

async function createFolder({ path, name }: { path: string; name: string }) {
  const res = await axios.post('/api/v1/files/folders', { path, name })
  return res.data // FileEntry
}

async function deleteEntry(entry: FileEntry) {
  await axios.delete(`/api/v1/files/${entry.id}`)
}

async function renameEntry({ entry, name }: { entry: FileEntry; name: string }) {
  const res = await axios.put(`/api/v1/files/${entry.id}/rename`, { name })
  return res.data
}

async function moveEntry({
  entry,
  targetPath,
}: {
  entry: FileEntry
  targetPath: string
}) {
  const res = await axios.put(`/api/v1/files/${entry.id}/move`, { targetPath })
  return res.data
}

<FileManagerPage
  locale="vi"
  callback={{
    list: { onSubmit: listFiles },
    upload: {
      onSubmit: uploadFiles,
      success: () => notify.success('Đã upload'),
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
    createFolder: { onSubmit: createFolder },
    delete: {
      before: ({ entry }) =>
        window.confirm(`Xóa ${entry.name}?`) ? undefined : false,
      onSubmit: deleteEntry,
    },
    rename: { onSubmit: renameEntry },
    move: { onSubmit: moveEntry },
  }}
/>
```

### Controlled mode

```tsx
<FileManagerPage
  items={files}
  folders={folders}
  entries={allEntries}
  storage={storageInfo}
  loading={loading}
  // Vẫn nên truyền callback cho upload/delete/… khi user thao tác
/>
```

### Props UI tùy chọn

`locale`, `className`, `content` (`DefaultLayout`, `navigate`, `reload`, `openUpload`, …), `total`.

### Giao diện — `content`

```tsx
<FileManagerPage
  locale="vi"
  content={(ctx) => (
    <>
      <StorageAlert used={ctx.storage.used} quota={ctx.storage.quota} />
      {ctx.DefaultLayout}
    </>
  )}
/>
```
