# Planner

Kanban board + Calendar + Timeline cho quản lý công việc. Gọi API REST chuẩn `v1/planner/*`.

---

## Khi nào dùng

- Cần UI planner đầy đủ (kéo thả cột, lịch, timeline) mà không viết lại từ đầu.
- BE (hoặc Vite mock) implement cùng contract API.

---

## Export chính

```ts
import {
  PlannerPage,
  PLANNER_ROUTES,
  configurePlannerNavigate,
  callGetPlannerBoard,
  callCreatePlannerItem,
  callMovePlannerItem,
  getBoardMock,
} from '@platform/core'
```

---

## CSS (bắt buộc)

```css
@import "@platform/core/planner.css";
```

Peer: `@fullcalendar/*`, `@atlaskit/pragmatic-drag-and-drop`.

---

## Tích hợp

```tsx
import { useEffect } from 'react'
import { useNavigate, Route } from 'react-router-dom'
import { PlannerPage, PLANNER_ROUTES, configurePlannerNavigate } from '@platform/core'

function PlannerBridge() {
  const navigate = useNavigate()
  useEffect(() => configurePlannerNavigate((to) => navigate(to)), [navigate])
  return null
}

<PlannerBridge />
<Route path={PLANNER_ROUTES.page} element={<PlannerPage locale="vi" />} />
```

Route mặc định: `/planner`.

---

## Tích hợp API HTTP thật (phương án chính)

`PlannerPage` mặc định gọi `call*` qua axios. Host app chỉ cần bootstrap HTTP + proxy BE.

### Cấu hình Sample

```env
# .env.development
VITE_API_URL=/api/
VITE_USE_MOCK=false
VITE_API_PROXY_TARGET=http://127.0.0.1:5167
```

```bash
# Terminal 1
cd Sample && dotnet run

# Terminal 2
cd Sample/clients/web && npm run dev
```

```tsx
// Không cần callback — page tự gọi callGetPlannerBoard, callCreatePlannerItem, ...
<Route path={PLANNER_ROUTES.page} element={<PlannerPage locale="vi" />} />
```

BE phải implement contract bảng API bên dưới. Seed JSON tham chiếu: `features/planner/mocks/get-board.json`.

Chi tiết luồng HTTP: [api-integration-and-usage.md](../../../docs/api-integration-and-usage.md).

---

## API

| Service | Method | Path |
|---------|--------|------|
| `callGetPlannerBoard` | GET | `v1/planner/board` |
| `callCreatePlannerItem` | POST | `v1/planner/items` |
| `callUpdatePlannerItem` | PUT | `v1/planner/items/:id` |
| `callMovePlannerItem` | PUT | `v1/planner/items/:id/move` |
| `callReschedulePlannerItem` | PUT | `v1/planner/items/:id/reschedule` |
| `callDeletePlannerItem` | DELETE | `v1/planner/items/:id` |

Query board: `?search=&priority=all|low|medium|high`.

---

## Giả lập API (dev / demo — Sample)

Khi **chưa có** endpoint Planner trên BE, Sample dùng Vite middleware thay vì proxy:

```env
VITE_API_URL=/api/
VITE_USE_MOCK=true
```

Plugin `Sample/clients/web/vite.plugins/plannerApiMock.ts` intercept `/api/v1/planner/*` **trước** proxy. Axios vẫn gọi HTTP bình thường — Network tab hiện XHR, response từ JSON seed.

Tắt mock → chuyển sang API thật: `VITE_USE_MOCK=false` (xem mục trên).

---

## Tùy biến

### Logic — `callback`

| Key | Mặc định |
|-----|----------|
| `list` | `callGetPlannerBoard` |
| `create` | `callCreatePlannerItem` |
| `update` | `callUpdatePlannerItem` |
| `move` | `callMovePlannerItem` |
| `reschedule` | `callReschedulePlannerItem` |
| `delete` | `callDeletePlannerItem` |

```tsx
<PlannerPage
  locale="vi"
  initialView="calendar"
  callback={{
    list: {
      onSubmit: (query) => callGetPlannerBoard(query),
      success: ({ result }) => console.log(result.data),
    },
    create: {
      before: () => { if (!canCreate) return false },
      onSubmit: (data) => callCreatePlannerItem(data),
    },
    delete: {
      onSubmit: (id) => callDeletePlannerItem(id),
      error: ({ error }) => logError(error),
    },
  }}
/>
```

### Controlled mode

Truyền **cả** `columns` + `items` → page không fetch board:

```tsx
<PlannerPage columns={columns} items={items} loading={loading} />
```

### Giao diện — `content`

Context: `PlannerPageContentContext` — `columns`, `items`, `visibleItems`, `view`, `setView`, `openCreate`, `reload`, `DefaultLayout`.

```tsx
<PlannerPage
  title="Công việc"
  withShell={false}
  content={(ctx) => (
    <>
      <MyPlannerHeader onAdd={ctx.openCreate} />
      {ctx.DefaultLayout}
    </>
  )}
/>
```

| Prop | Tác dụng |
|------|----------|
| `initialView` | `"kanban"` / `"calendar"` / `"timeline"` |
| `title`, `description`, `className` | Shell / wrapper |
| `withShell={false}` | Bỏ `PlannerPageShell` |

---

## Props dự án khác phải custom

Khi BE khác contract mặc định, hoặc cần permission / toast / confirm — override `callback.*`. Mỗi key là `ActionProps`. Không truyền `columns`+`items` thì page tự `list`.

### `PlannerPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.list.onSubmit` | Load board (cột + item) theo filter | `async (query) => callGetPlannerBoard(query)` | Mở page / đổi search/priority |
| `callback.create.before` | Chặn tạo nếu thiếu quyền / title | `(payload) => payload.title.trim() ? undefined : false` | `false` = không gọi create |
| `callback.create.onSubmit` | Tạo item mới | `async (payload) => callCreatePlannerItem(payload)` | Submit form tạo |
| `callback.update.onSubmit` | Sửa item trong panel | `async (payload) => callUpdatePlannerItem(payload)` | Lưu panel chi tiết |
| `callback.move.onSubmit` | Kéo thả sang cột Kanban khác | `async (payload) => callMovePlannerItem(payload)` | Drop card sang cột |
| `callback.reschedule.onSubmit` | Đổi ngày trên calendar/timeline | `async (payload) => callReschedulePlannerItem(payload)` | Drag event đổi start/end |
| `callback.delete.before` | Confirm xóa | `() => window.confirm('Xóa?') ? undefined : false` | Hủy nếu user không đồng ý |
| `callback.delete.onSubmit` | Xóa item | `async (id) => callDeletePlannerItem(id)` | Sau confirm |
| `columns` + `items` + `loading` | Controlled — host tự fetch | mảng từ React Query | Page **không** gọi `list` |
| `initialView` | View mở đầu | `"calendar"` | `kanban` / `calendar` / `timeline` |

```tsx
async function loadBoard(query?: PlannerBoardQuery) {
  // Lấy columns + items theo search/priority
  return callGetPlannerBoard(query)
}

async function createItem(payload: CreatePlannerItemPayload) {
  return callCreatePlannerItem(payload)
}

async function updateItem(payload: UpdatePlannerItemPayload) {
  return callUpdatePlannerItem(payload)
}

async function moveItem(payload: MovePlannerItemPayload) {
  // Đổi statusId / thứ tự khi kéo Kanban
  return callMovePlannerItem(payload)
}

async function rescheduleItem(payload: ReschedulePlannerItemPayload) {
  // Đổi start/end khi kéo trên lịch
  return callReschedulePlannerItem(payload)
}

async function deleteItem(id: string) {
  return callDeletePlannerItem(id)
}

<PlannerPage
  locale="vi"
  initialView="kanban"
  callback={{
    list: { onSubmit: loadBoard },
    create: {
      before: (payload) => (payload.title.trim() ? undefined : false),
      onSubmit: createItem,
      success: () => notify.success('Đã tạo'),
    },
    update: { onSubmit: updateItem },
    move: {
      onSubmit: moveItem,
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
    reschedule: { onSubmit: rescheduleItem },
    delete: {
      before: () => (window.confirm('Xóa?') ? undefined : false),
      onSubmit: deleteItem,
    },
  }}
  content={(ctx) => (
    <>
      <button type="button" onClick={ctx.openCreate}>Thêm</button>
      {ctx.DefaultLayout}
    </>
  )}
/>
```

**Controlled** (không gọi `list`):

```tsx
<PlannerPage columns={columns} items={items} loading={loading} />
```

### Props UI tùy chọn

`title`, `description`, `className`, `withShell`, `content`, `locale`.

---

## Permission & menu

```ts
import { PLANNER_PERMISSIONS, hasPlannerPermission, plannerMenuItems } from '@platform/core'
```
