# Role

Quản lý **vai trò** và **cây phân quyền** (permission catalog dạng nhóm).

---

## Export chính

```ts
import {
  RoleListPage,
  ROLE_ROUTES,
  configureRoleNavigate,
  mockGetRoleList,
  callGetRoleList,
  getRoleListMock,
  PERMISSION_CATALOG,
} from '@platform/core'
```

---

## Tích hợp

```tsx
configureRoleNavigate((to) => navigate(to))

<Route path={ROLE_ROUTES.list} element={<RoleListPage locale="vi" />} />
```

Route mặc định: `/roles`.

---

## API HTTP thật (khuyến nghị)

Service axios sẵn sàng — override page qua `callback`:

| Service | Path |
|---------|------|
| `callGetRoleList` | GET `v1/roles` |
| `callGetRole` | GET `v1/roles/:id` |
| `callCreateRole` | POST `v1/roles` |
| `callUpdateRolePermissions` | PUT `v1/roles/:id/permissions` |

```tsx
<RoleListPage
  locale="vi"
  callback={{
    list: {
      onSubmit: async ({ search, page, size }) => {
        const res = await callGetRoleList({ search, page, size })
        return res.data
      },
    },
  }}
/>
```

Cấu hình HTTP: [api-integration-and-usage.md](../../../docs/api-integration-and-usage.md).

---

## Giả lập in-memory (mặc định page)

**Mặc định** `RoleListPage` dùng mock trong FE (không gọi HTTP):

- `mockGetRoleList`, `mockCreateRole`, `mockUpdateRole`, …
- Seed từ `features/role/mocks/get-role-list.json` → `FAKE_ROLES`

Phù hợp demo UI. Production: dùng `callback` với `call*` như trên.

---

## Tùy biến

### Logic — `callback`

Mặc định page dùng `mock*`. Production override:

| Key | Mặc định | API thật |
|-----|----------|----------|
| `create` | `mockCreateRole` | `callCreateRole` |
| `update` | `mockUpdateRole` | `callUpdateRole` |
| `delete` | `mockDeleteRole` | `callDeleteRole` |
| `updatePermissions` | `mockUpdateRolePermissions` | `callUpdateRolePermissions` |

```tsx
<RoleListPage
  callback={{
    create: { onSubmit: (data) => callCreateRole(data).then((r) => r.data) },
    update: {
      onSubmit: ({ id, data }) => callUpdateRole(id, data).then((r) => r.data),
    },
    delete: { onSubmit: (role) => callDeleteRole(role.id) },
    updatePermissions: {
      onSubmit: ({ id, permissions }) =>
        callUpdateRolePermissions(id, { permissions }).then((r) => r.data),
    },
  }}
/>
```

List fetch mặc định qua `mockGetRoleList` nội bộ page — muốn API list, dùng **controlled mode**: truyền `items`, `total`, `loading`.

### Giao diện — `content`

Context `RoleListPageContentContext`: `items`, `openCreate`, `openEdit`, `openView`, `requestDelete`, `confirmDelete`, `pendingDelete`, `reload`, `DefaultTable`, `DefaultPagination`, `DefaultDialogs`, `DefaultContent`.

```tsx
<RoleListPage
  headerActions={<SyncPermissionsButton />}
  content={(ctx) => (
    <>
      {ctx.DefaultTable}
      {ctx.DefaultPagination}
    </>
  )}
/>
```

Dialog tự build:

```tsx
<RoleListPage
  withDialogs={false}
  content={(ctx) => (
    <>
      {ctx.DefaultContent}
      {ctx.pendingDelete && <MyConfirm onConfirm={ctx.confirmDelete} />}
    </>
  )}
/>
```

| Prop | Tác dụng |
|------|----------|
| `withShell={false}` | Bỏ `RolePageShell` |
| `withDialogs={false}` | Tắt dialog create/edit/permission mặc định |
| `headerActions` | Nút thêm ở header |

---

## Props dự án khác phải custom

Mặc định page dùng **mock**. Production override `callback` bằng `call*`. List fetch: hoặc để mock, hoặc **controlled** (`items`/`total`/`loading`) + host gọi `callGetRoleList`.

### `RoleListPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.create.onSubmit` | Tạo role mới | `async (data) => (await callCreateRole(data)).data` | Lưu dialog tạo |
| `callback.create.before` | Validate / quyền trước tạo | `({ data }) => data.name.trim() ? undefined : false` | `false` = không gọi API |
| `callback.update.onSubmit` | Sửa tên/mô tả role | `async ({ id, data }) => (await callUpdateRole(id, data)).data` | Lưu dialog sửa |
| `callback.delete.onSubmit` | Xóa role | `async (role) => callDeleteRole(role.id)` | Sau confirm xóa |
| `callback.updatePermissions.onSubmit` | Lưu cây permission | `async ({ id, permissions }) => (await callUpdateRolePermissions(id, { permissions })).data` | Lưu PermissionEditor |
| `items` + `total` + `loading` | Controlled list từ BE | host `callGetRoleList` rồi truyền | Page **không** mock-fetch list |
| `headerActions` | Nút phụ header (sync IAM…) | `<SyncButton />` | Slot cạnh tiêu đề |

```tsx
async function createRole(data: RoleFormData) {
  const res = await callCreateRole(data)
  return res.data
}

async function updateRole({ id, data }: { id: string; data: RoleFormData }) {
  const res = await callUpdateRole(id, data)
  return res.data
}

async function deleteRole(role: Role) {
  await callDeleteRole(role.id)
}

async function savePermissions({
  id,
  permissions,
}: {
  id: string
  permissions: string[]
}) {
  const res = await callUpdateRolePermissions(id, { permissions })
  return res.data
}

<RoleListPage
  locale="vi"
  headerActions={<button type="button">Sync IAM</button>}
  callback={{
    create: {
      before: ({ data }) => (data.name.trim() ? undefined : false),
      onSubmit: createRole,
      success: () => notify.success('Đã tạo role'),
    },
    update: { onSubmit: updateRole },
    delete: {
      onSubmit: deleteRole,
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
    updatePermissions: {
      onSubmit: savePermissions,
      complete: () => console.log('permissions saved'),
    },
  }}
/>
```

**Controlled list** + CRUD qua callback:

```tsx
async function fetchRoles(query: { search?: string; page: number; size: number }) {
  const res = await callGetRoleList(query)
  setRoles(res.data.items)
  setTotal(res.data.total)
}

<RoleListPage
  items={roles}
  total={total}
  loading={loading}
  callback={{ /* create/update/delete/updatePermissions như trên */ }}
/>
```

### Props UI tùy chọn

`title`, `description`, `className`, `content`, `withShell`, `withDialogs`, `locale`.

---

### Components tái sử dụng

`RoleTable`, `RoleForm`, `PermissionEditor`, `RoleDetailView` — ghép layout riêng không qua `RoleListPage`.

---

## Permission catalog

`PERMISSION_CATALOG` — cấu trúc nhóm `identity`, `saas`, `account`, …

Utility: `collectAllPermissionIds`, `togglePermissionNode`, …
