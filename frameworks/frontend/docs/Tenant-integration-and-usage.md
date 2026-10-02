# Tenant

Quản lý **multi-tenant**: danh sách tenant, chi tiết, form tạo/sửa, **connections** (DB), **domains**.

Tích hợp **Query Builder** trên màn list để lọc nâng cao.

---

## Export chính

```ts
import {
  TenantListPage,
  TenantFormPage,
  TenantDetailPage,
  TenantConnectionsPage,
  TenantDomainsPage,
  configureTenantNavigate,
  TENANT_ROUTES,
  callGetTenantList,
  getTenantListMock,
} from '@platform/core'
```

---

## CSS

```css
@import "@platform/core/styles.css";
```

---

## Tích hợp routes

```tsx
import { useParams } from 'react-router-dom'
import {
  TENANT_ROUTES,
  getTenantDetailPath,
  TenantListPage,
  TenantFormPage,
  TenantDetailPage,
  TenantConnectionsPage,
  TenantDomainsPage,
  configureTenantNavigate,
} from '@platform/core'

configureTenantNavigate((to) => navigate(to))

function TenantDetailRoute() {
  const { id } = useParams()
  return <TenantDetailPage tenantId={id!} />
}

<Route path={TENANT_ROUTES.list} element={<TenantListPage locale="vi" />} />
<Route path={TENANT_ROUTES.create} element={<TenantFormPage mode="create" />} />
<Route path={TENANT_ROUTES.detail} element={<TenantDetailRoute />} />
<Route path={TENANT_ROUTES.edit} element={<TenantEditRoute />} />
<Route path={TENANT_ROUTES.connections} element={<TenantConnectionsRoute />} />
<Route path={TENANT_ROUTES.domains} element={<TenantDomainsRoute />} />
```

---

## API HTTP thật

Tenant **luôn** gọi `call*` qua axios — cần BE implement contract dưới đây. Không có mock middleware trong Sample.

```tsx
// Mặc định — không cần callback
<Route path={TENANT_ROUTES.list} element={<TenantListPage locale="vi" />} />
```

Cấu hình: `configurePlatformHttp` + Vite proxy — xem [api-integration-and-usage.md](../../../docs/api-integration-and-usage.md).

---

## API

| Service | Path |
|---------|------|
| `callGetTenantList` | GET `v1/tenants` |
| `callGetTenant` | GET `v1/tenants/:id` |
| `callCreateTenant` | POST `v1/tenants` |
| `callUpdateTenant` | PUT `v1/tenants/:id` |
| `callDeleteTenant` | DELETE `v1/tenants/:id` |
| `callSetTenantStatus` | PATCH `v1/tenants/:id/status` |
| `callGetTenantConnectionList` | GET `v1/tenants/:id/connections` |
| `callGetTenantDomainList` | GET `v1/tenants/:id/domains` |

Mock JSON: `get-tenant-list.json`, `get-connections.json`, `get-domains.json`.

---

## Tùy biến

### Logic — `callback` theo page

| Page | Keys | Mặc định |
|------|------|----------|
| TenantListPage | `delete`, `toggleStatus` | `callDeleteTenant`, `callSetTenantStatus` |
| TenantFormPage | `callback` (root) | `callCreateTenant` / `callUpdateTenant` |
| TenantDetailPage | `toggleStatus` | `callSetTenantStatus` |
| TenantConnectionsPage | `add`, `update`, `delete` | `call*` connections |
| TenantDomainsPage | `add`, `update`, `delete` | `call*` domains |

```tsx
<TenantFormPage
  mode="create"
  hideStatus
  showAttachments
  callback={{
    before: async () => { if (!hasPermission) return false },
    onSubmit: async (data) => {
      const res = await callCreateTenant(data)
      return res.data
    },
  }}
/>

<TenantListPage
  callback={{
    delete: {
      before: ({ tenant }) => { if (tenant.protected) return false },
      onSubmit: (tenant) => callDeleteTenant(tenant.id),
    },
    toggleStatus: { onSubmit: (tenant) => callSetTenantStatus(tenant.id, { status: … }) },
  }}
/>
```

### Controlled mode & điều hướng

```tsx
<TenantListPage
  items={tenants}
  total={total}
  loading={loading}
  onQueryChange={(query) => fetchFromHost(query)}
  useRoutes={false}
  onCreate={() => navigate('/orgs/new')}
  onView={(t) => navigate(`/orgs/${t.id}`)}
  onEdit={(t) => openDrawer(t)}
/>
```

### Giao diện — `content`

`content` là `(ctx) => ReactNode` — **không** phải object `{ toolbarExtra: … }`. Ghép `ctx.Default*`:

Context `TenantListPageContentContext`: `items`, `total`, `query`, `reload`, `requestDelete`, `confirmDelete`, `pendingDelete`, `DefaultToolbar`, `DefaultTable`, `DefaultPagination`, `DefaultContent`.

```tsx
<TenantListPage
  withQueryBuilder
  headerActions={<ExportButton />}
  content={(ctx) => (
    <>
      <Banner count={ctx.total} />
      {ctx.DefaultToolbar}
      {ctx.DefaultTable}
      {ctx.DefaultPagination}
    </>
  )}
/>
```

Dialog confirm riêng:

```tsx
<TenantListPage
  withDialogs={false}
  content={(ctx) => (
    <>
      {ctx.DefaultContent}
      {ctx.pendingDelete && (
        <MyConfirm onConfirm={ctx.confirmDelete} onCancel={ctx.cancelDelete} />
      )}
    </>
  )}
/>
```

Form page — context có `register`, `control`, `errors`, `submit`, `DefaultContent`.

| Prop | Tác dụng |
|------|----------|
| `withShell={false}` | Bỏ `TenantPageShell` |
| `withDialogs={false}` | Tắt dialog mặc định (list, connections, domains) |
| `withQueryBuilder` | Bật/tắt Query Builder trên list |
| `initialQuery`, `defaultPageSize` | Override query ban đầu |

---

## Props dự án khác phải custom

Tenant mặc định gọi `call*` qua axios. Custom khi cần điều hướng host, permission, toast, hoặc controlled fetch.

### `TenantListPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `onCreate` | Nút Tạo tenant | `() => navigate('/orgs/new')` | Thay route kit mặc định |
| `onView` | Click xem chi tiết | `(t) => navigate(\`/orgs/${t.id}\`)` | Mở detail theo path host |
| `onEdit` | Click sửa | `(t) => openDrawer(t)` | Mở drawer/page edit host |
| `onQueryChange` | Sync search/page/filter ra host | `(query) => fetchTenants(query)` | Controlled: mỗi lần đổi query → host refetch |
| `callback.delete.onSubmit` | Xóa tenant | `async (tenant) => callDeleteTenant(tenant.id)` | Sau confirm xóa |
| `callback.delete.before` | Confirm / chặn xóa protected | `({ tenant }) => tenant.protected ? false : undefined` | `false` = hủy |
| `callback.toggleStatus.onSubmit` | Bật/tắt trạng thái | `async (tenant) => callSetTenantStatus(tenant.id, { status })` | Toggle active/inactive |
| `useRoutes` | Có dùng navigate mặc định của kit không | `false` | `false` → bắt buộc tự gắn `onCreate`/`onView`/`onEdit` |
| `items` + `total` + `loading` | Controlled list | từ React Query | Page không tự list |

```tsx
function goCreate() {
  navigate('/orgs/new')
}

function goView(tenant: Tenant) {
  navigate(`/orgs/${tenant.id}`)
}

function goEdit(tenant: Tenant) {
  openDrawer(tenant)
}

function syncQuery(query: GetTenantListParams) {
  // Host refetch theo search/page/status mới
  void fetchTenants(query)
}

async function deleteTenant(tenant: Tenant) {
  await callDeleteTenant(tenant.id)
}

async function toggleTenantStatus(tenant: Tenant) {
  const next = tenant.status === 'active' ? 'inactive' : 'active'
  await callSetTenantStatus(tenant.id, { status: next })
}

<TenantListPage
  locale="vi"
  useRoutes={false}
  onCreate={goCreate}
  onView={goView}
  onEdit={goEdit}
  onQueryChange={syncQuery}
  items={tenants}
  total={total}
  loading={loading}
  callback={{
    delete: {
      before: ({ tenant }) =>
        window.confirm(`Xóa ${tenant.name}?`) ? undefined : false,
      onSubmit: deleteTenant,
      success: () => notify.success('Đã xóa'),
    },
    toggleStatus: {
      onSubmit: toggleTenantStatus,
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
  }}
/>
```

### `TenantFormPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `mode` | Tạo hay sửa | `"edit"` | Chọn API create vs update |
| `tenantId` | Id khi edit | `params.id` | Load + update đúng tenant |
| `callback.onSubmit` | Submit form | `async (data) => { await callUpdateTenant(id, data); return data }` | Bấm Lưu |
| `callback.before` | Validate / quyền trước save | `({ data, mode }) => mode === 'edit' && !data.code ? false : undefined` | `false` = hủy |
| `callback.success` | Sau save OK | `() => navigate(\`/tenants/${id}\`)` | Điều hướng / toast |
| `onCancel` | Nút Hủy | `() => navigate('/tenants')` | Thoát form |

```tsx
async function submitTenantForm(data: TenantFormData) {
  if (mode === 'create') {
    const res = await callCreateTenant(data)
    return res.data
  }
  await callUpdateTenant(id, data)
  return data
}

<TenantFormPage
  mode="edit"
  tenantId={id}
  defaultValues={tenant}
  hideStatus
  onCancel={() => navigate('/tenants')}
  callback={{
    before: ({ data, mode }) =>
      mode === 'edit' && !data.code ? false : undefined,
    onSubmit: submitTenantForm,
    success: () => navigate(`/tenants/${id}`),
  }}
/>
```

### `TenantDetailPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `tenantId` / `tenant` | Load theo id hoặc controlled entity | `id` hoặc object sẵn | Hiện chi tiết |
| `onEdit` | Nút Sửa | `() => navigate(\`/tenants/${id}/edit\`)` | Vào form edit |
| `onManageConnections` | Quản lý DB connection | `() => navigate(\`/tenants/${id}/connections\`)` | Sub-page connections |
| `onManageDomains` | Quản lý domain | `() => navigate(\`/tenants/${id}/domains\`)` | Sub-page domains |
| `callback.toggleStatus.onSubmit` | Đổi trạng thái trên detail | `async (tenant) => callSetTenantStatus(tenant.id, {…})` | Nút active/inactive |

```tsx
async function toggleStatus(tenant: Tenant) {
  const next = tenant.status === 'active' ? 'inactive' : 'active'
  await callSetTenantStatus(tenant.id, { status: next })
}

<TenantDetailPage
  tenantId={id}
  useRoutes={false}
  onEdit={() => navigate(`/tenants/${id}/edit`)}
  onManageConnections={() => navigate(`/tenants/${id}/connections`)}
  onManageDomains={() => navigate(`/tenants/${id}/domains`)}
  callback={{ toggleStatus: { onSubmit: toggleStatus } }}
/>
```

### `TenantConnectionsPage` / `TenantDomainsPage` — props phải custom

Cùng shape (đổi type Connection ↔ Domain).

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `tenantId` | **Bắt buộc** — tenant đang quản lý | `id` | Scope CRUD theo tenant |
| `callback.add.onSubmit` | Thêm connection/domain | `async (data) => callAddTenantConnection(id, data)` | Submit dialog thêm |
| `callback.update.onSubmit` | Sửa | `async ({ connectionId, data }) => callUpdateTenantConnection(…)` | Submit dialog sửa |
| `callback.delete.onSubmit` | Xóa | `async (item) => callDeleteTenantConnection(id, item.id)` | Sau confirm |
| `callback.delete.before` | Confirm | `({ item }) => window.confirm(…) ? undefined : false` | Hủy nếu không đồng ý |

```tsx
async function addConnection(data: TenantConnectionFormData) {
  return callAddTenantConnection(id, data)
}

async function updateConnection({
  connectionId,
  data,
}: {
  connectionId: string
  data: TenantConnectionFormData
}) {
  return callUpdateTenantConnection(id, connectionId, data)
}

async function deleteConnection(item: TenantConnection) {
  await callDeleteTenantConnection(id, item.id)
}

<TenantConnectionsPage
  tenantId={id}
  tenantName={tenant.name}
  callback={{
    add: {
      onSubmit: addConnection,
      success: () => notify.success('Đã thêm kết nối'),
    },
    update: { onSubmit: updateConnection },
    delete: {
      before: ({ item }) =>
        window.confirm(`Xóa ${item.name}?`) ? undefined : false,
      onSubmit: deleteConnection,
    },
  }}
/>
```

`TenantDomainsPage` tương tự — đổi tên hàm service domain.

### Props UI tùy chọn

`title`, `description`, `className`, `headerActions`, `content`, `withShell`, `withDialogs`, `withQueryBuilder`, `initialQuery`, `defaultPageSize`, `hideStatus`, `showAttachments`, `locale`.


---

## Query Builder trên list

```tsx
<TenantListPage withQueryBuilder locale="vi" />
```

Query Builder gọi `callGetCompanyEmployees` (demo) — **không** filter trực tiếp tenant list. Xem [queryBuilder — tích hợp & sử dụng](../../queryBuilder/docs/integration-and-usage.md).

---

## Permission

```ts
import { TENANT_PERMISSIONS, hasTenantPermission } from '@platform/core'
```
