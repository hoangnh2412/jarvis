# Tài liệu Features — @platform/core

Package `@platform/core` (`frameworks/frontend`) là UI kit React trên **PrimeReact 11**, gồm nhiều **feature module** độc lập trong `src/features/`.

Mỗi feature export: **Page**, **routes**, **services**, **types**, **localization**, **permission**, và (nếu có) **mock JSON**.

---

## Danh sách tài liệu

| Phạm vi | Mô tả | Tài liệu |
|---------|-------|----------|
| **Tích hợp kit (chung)** | Quy trình bootstrap, routes, menu, Sample | [integration-and-usage.md](./integration-and-usage.md) |
| **API (HTTP & mock)** | Tích hợp axios, Sample, proxy | [api-integration-and-usage.md](./api-integration-and-usage.md) |
| **Cấu trúc module** | Chuẩn folder feature | [SAD-fe-module-structure.md](./SAD-fe-module-structure.md) |

### Tích hợp & sử dụng theo feature

Mỗi feature có tài liệu riêng tại `src/features/<feature>/docs/integration-and-usage.md`:

| Feature | Mô tả ngắn | Tài liệu |
|---------|------------|----------|
| Account | Đăng nhập, hồ sơ, đổi mật khẩu | [integration-and-usage.md](../src/features/account/docs/integration-and-usage.md) |
| Dashboard | Biểu đồ GridStack + Chart.js | [integration-and-usage.md](../src/features/dashboard/docs/integration-and-usage.md) |
| Planner | Kanban / Calendar / Timeline | [integration-and-usage.md](../src/features/planner/docs/integration-and-usage.md) |
| Timesheet | Lưới log giờ theo project/user | [integration-and-usage.md](../src/features/timesheet/docs/integration-and-usage.md) |
| Tenant | Quản lý tenant, domain, connection | [integration-and-usage.md](../src/features/tenant/docs/integration-and-usage.md) |
| Role | Vai trò & phân quyền | [integration-and-usage.md](../src/features/role/docs/integration-and-usage.md) |
| Craft PDF | Editor template PDF | [integration-and-usage.md](../src/features/craftPdf/docs/integration-and-usage.md) |
| Dynamic Form | Form builder + điền form | [integration-and-usage.md](../src/features/dynamicForm/docs/integration-and-usage.md) |
| File Manager | Quản lý file/folder (mock) | [integration-and-usage.md](../src/features/fileManager/docs/integration-and-usage.md) |
| Import | Import Excel + validate | [integration-and-usage.md](../src/features/import/docs/integration-and-usage.md) |
| Query Builder | Component lọc nâng cao (embed) | [integration-and-usage.md](../src/features/queryBuilder/docs/integration-and-usage.md) |

**App mẫu tích hợp đầy đủ:** `Sample/clients/web` (`src/App.tsx`).

---

## Tích hợp chung (mọi dự án)

### 1. Cài package

```bash
# Monorepo / local link
npm i file:../../../frameworks/frontend

# Hoặc từ registry (sau khi publish)
npm i @platform/core
```

Peer deps: xem [`README.md`](../README.md) ở root package.

### 2. Bootstrap HTTP (một lần) — **bắt buộc cho API thật**

```ts
// main.tsx hoặc constants/index.ts
import { configurePlatformHttp } from '@platform/core'

configurePlatformHttp({
  baseURL: import.meta.env.VITE_API_URL,      // vd: '/api/'
  apiKey: import.meta.env.VITE_API_KEY,
  apiKeyHeader: import.meta.env.VITE_API_KEY_NAME,
})
```

Mọi feature dùng axios instance chung → prefix `v1/...` tương đối `baseURL`.

**Hướng dẫn đầy đủ:** [api-integration-and-usage.md](./api-integration-and-usage.md) — HTTP thật (phương án chính) và giả lập API (dev/demo trong Sample).

### 3. Provider & Toast

```tsx
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@platform/core'

<PrimeReactProvider {...kitPrimeReactConfig}>
  <App />
  <Toaster />
</PrimeReactProvider>
```

### 4. CSS & Tailwind

```css
/* index.css */
@import "@platform/core/theme.css";
@import "@platform/core/styles.css";

/* Feature cần dùng — import theo module */
@import "@platform/core/dashboard.css";
@import "@platform/core/planner.css";
@import "@platform/core/timesheet.css";
@import "@platform/core/dynamicForm.css";
```

Tailwind v4: `@source` trỏ tới `node_modules/@platform/core/dist` (xem README package).

### 5. Routes + Navigate bridge

Mỗi feature có `*_ROUTES` và `configure*Navigate`. Host app **bắt buộc** gọi bridge để link nội bộ feature hoạt động:

```tsx
import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { configurePlannerNavigate, PlannerPage, PLANNER_ROUTES } from '@platform/core'

function PlannerNavigateBridge() {
  const navigate = useNavigate()
  useEffect(() => {
    configurePlannerNavigate((to) => navigate(to))
  }, [navigate])
  return null
}

// Trong Routes:
<Route path={PLANNER_ROUTES.page} element={<PlannerPage locale="vi" />} />
```

### 6. AdminLayout (tuỳ chọn)

```tsx
import { AdminLayout, DEFAULT_ADMIN_MAIN_NAV } from '@platform/core'

<Route element={<AdminLayout />}>
  {/* child routes */}
</Route>
```

`DEFAULT_ADMIN_MAIN_NAV` đã có sẵn link Planner, Timesheet, Tenant, … — có thể override qua prop `mainNav`.

---

## Gọi API — HTTP thật vs giả lập

**Phương án chính (production):** Page kit gọi `call*` qua axios → `VITE_API_URL` + Vite proxy (dev) hoặc URL production → **backend thật**. Sample: `VITE_USE_MOCK=false`, chạy `dotnet run` trong `Sample/`.

**Phương án thay thế (dev/demo):** Giả lập response khi chưa có BE — xem [api-integration-and-usage.md](./api-integration-and-usage.md).

| Feature | Mặc định Sample | API thật (`call*`) | Giả lập (dev) |
|---------|-----------------|-------------------|---------------|
| Account | `callLogin` → BE | Có — cần BE + JWT interceptor | `mockLogin` tài khoản demo (Sample) |
| Tenant | `call*` | Có — cần BE | Không mock sẵn |
| Planner | `call*` | Có — `VITE_USE_MOCK=false` | Vite middleware khi `VITE_USE_MOCK=true` |
| Timesheet | `call*` | Có — `VITE_USE_MOCK=false` | Vite middleware khi `VITE_USE_MOCK=true` |
| Dashboard | `callGetDashboardChartCatalog` | Có — BE trả catalog | Fallback fake khi API lỗi/rỗng |
| Role | In-memory `mock*` | `callGetRoleList` + callback | Mặc định page |
| File Manager / Import | In-memory mock | Override `callback` | Mặc định page |
| Dynamic Form | localStorage | Implement `call*` giữ chữ ký | Mặc định services |
| Craft PDF | Memory | BE + `configureCraftPdfHttp` | Khi chưa set baseURL |

Seed JSON contract: `src/features/<feature>/mocks/*.json`.

---

## Pattern callback (ActionProps)

Nhiều page hỗ trợ `callback` để host app **chèn logic trước/sau API** (giống jQuery ajax):

```tsx
<PlannerPage
  callback={{
    list: { onSubmit: (query) => callGetPlannerBoard(query) },
    create: {
      before: () => { /* return false để hủy */ },
      onSubmit: (data) => callCreatePlannerItem(data),
      success: ({ result }) => console.log(result.data),
    },
  }}
/>
```

Nếu không truyền `callback`, page gọi **service mặc định** (`callGet*`, `mock*`). Chi tiết `callback` / `content` theo từng feature — xem tài liệu feature tương ứng (mục **Tùy biến**).

---

## Biến môi trường thường dùng

| Biến | Ý nghĩa |
|------|---------|
| `VITE_API_URL` | Base axios, vd `/api/` |
| `VITE_API_KEY` | API key header |
| `VITE_API_KEY_NAME` | Tên header key |
| `VITE_USE_MOCK` | Sample: bật Vite mock Planner/Timesheet |
| `VITE_API_PROXY_TARGET` | Sample: proxy `/api` → BE (vd `:5167`) |

---

## Cấu trúc thư mục feature

```
src/features/<feature>/
├── docs/           # Tích hợp & sử dụng (integration-and-usage.md)
├── pages/          # Page entry (PlannerPage, …)
├── components/     # UI con
├── services/       # callGet*, mock*
├── routes/         # *_ROUTES, configure*Navigate
├── mocks/          # JSON seed (dev / middleware)
├── types/          # TypeScript types
├── localization/   # vi / en messages
├── permission/     # permission keys
└── index.ts        # Public exports
```

Đọc tiếp tài liệu tích hợp & sử dụng từng feature ở bảng **Tích hợp & sử dụng theo feature** phía trên.
