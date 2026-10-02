# Timesheet

Lưới log giờ theo **project** và **member**, group theo ngày/tuần, export CSV, popover chi tiết work item.

---

## Export chính

```ts
import {
  TimesheetPage,
  TIMESHEET_ROUTES,
  configureTimesheetNavigate,
  callGetTimesheetBoard,
  callGetTimesheetOptions,
  callUpdateTimesheetHours,
  callSaveTimesheet,
  getTimesheetBoardMock,
} from '@platform/core'
```

---

## CSS (bắt buộc)

```css
@import "@platform/core/timesheet.css";
```

Peer: `ag-grid-community`, `ag-grid-react`.

---

## Tích hợp

```tsx
import { TimesheetPage, TIMESHEET_ROUTES, configureTimesheetNavigate } from '@platform/core'

// Bridge navigate (giống Planner)
configureTimesheetNavigate((to) => navigate(to))

<Route path={TIMESHEET_ROUTES.page} element={<TimesheetPage locale="vi" />} />
```

Route mặc định: `/timesheet`.

---

## Tích hợp API HTTP thật (phương án chính)

`TimesheetPage` mặc định gọi `callGetTimesheetBoard`, `callSaveTimesheet`, … qua axios.

### Cấu hình Sample

```env
VITE_API_URL=/api/
VITE_USE_MOCK=false
VITE_API_PROXY_TARGET=http://127.0.0.1:5167
```

Chạy BE (`dotnet run` trong `Sample/`) + FE (`npm run dev`). Request `/api/v1/timesheet/*` được proxy sang BE.

```tsx
<Route path={TIMESHEET_ROUTES.page} element={<TimesheetPage locale="vi" />} />
```

Contract chi tiết: bảng API bên dưới. Seed: `features/timesheet/mocks/*.json`.

Xem [api-integration-and-usage.md](../../../docs/api-integration-and-usage.md).

---

## API

| Service | Method | Path |
|---------|--------|------|
| `callGetTimesheetBoard` | GET | `v1/timesheet/board` |
| `callGetTimesheetOptions` | GET | `v1/timesheet/options` |
| `callUpdateTimesheetHours` | PUT | `v1/timesheet/hours` |
| `callSaveTimesheet` | POST | `v1/timesheet/save` |

Query board: `from`, `to`, `projectIds`, `userIds`, `groupBy`, `grain`.

Options trả `{ options, hasMore }` cho filter dropdown.

---

## Giả lập API (dev / demo — Sample)

Plugin `Sample/clients/web/vite.plugins/timesheetApiMock.ts` — bật khi `VITE_USE_MOCK=true`:

Seed:
- `features/timesheet/mocks/get-board.json`
- `features/timesheet/mocks/get-options.json`

Middleware chạy trước Vite proxy; không cần MSW Service Worker.

Chuyển sang BE thật: `VITE_USE_MOCK=false`.

---

## Hành vi page

- Mặc định range = **tháng hiện tại** (`startOfMonth` → `endOfMonth`).
- Filter debounce 300ms trước khi gọi API.
- Toolbar: grain (day/week), group by project/user, export CSV, save.

---

## Tùy biến

### Logic — `callback`

| Key | Mặc định |
|-----|----------|
| `list` | `callGetTimesheetBoard` |
| `updateHours` | `callUpdateTimesheetHours` |
| `save` | `callSaveTimesheet` |

```tsx
<TimesheetPage
  callback={{
    list: { onSubmit: (query) => callGetTimesheetBoard(query) },
    updateHours: {
      onSubmit: (payload) => callUpdateTimesheetHours(payload),
      success: () => notify.success('Đã cập nhật giờ'),
    },
    save: {
      before: () => { if (!isDirty) return false },
      onSubmit: () => callSaveTimesheet(),
    },
  }}
/>
```

### Controlled mode

```tsx
<TimesheetPage logs={logs} loading={loading} />
```

Host tự fetch và truyền `logs` — page không gọi `callGetTimesheetBoard`.

### Giao diện — `content`

Context `TimesheetPageContentContext`: `logs`, `rows`, `from`, `to`, `grain`, `groupBy`, `filterIds`, `reload`, `DefaultLayout`.

```tsx
<TimesheetPage
  locale="vi"
  title="Chấm công"
  content={(ctx) => (
    <>
      <TimesheetSummary rows={ctx.rows} />
      {ctx.DefaultLayout}
    </>
  )}
/>
```

| Prop | Tác dụng |
|------|----------|
| `withShell={false}` | Bỏ `TimesheetPageShell` |
| `title`, `description`, `className` | Tuỳ chỉnh shell |

---

## Props dự án khác phải custom

Mỗi key trong `callback` là `ActionProps`: `before` → `onSubmit` → (page reload) → `success` → `complete`; lỗi: `error` → `complete`. `before` return `false` để hủy.

### `TimesheetPage` — props phải custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ hàm truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|----------------------|---------------------------|
| `callback.list.onSubmit` | Load lưới theo khoảng ngày / filter | `async (query) => callGetTimesheetBoard(query)` | Đổi from/to, project, user, grain |
| `callback.list.before` | Chặn load nếu range invalid | `(query) => query.from && query.to ? undefined : false` | Không gọi API khi thiếu ngày |
| `callback.updateHours.onSubmit` | Sửa một ô giờ trên lưới | `async (payload) => callUpdateTimesheetHours(payload)` | User edit cell → cập nhật log |
| `callback.updateHours.success` | Side-effect sau sửa giờ | `({ result }) => console.log(result.data.id)` | Sau khi page cập nhật state local |
| `callback.save.before` | Confirm trước khi lưu cả bảng | `() => window.confirm('Lưu?') ? undefined : false` | Hủy save nếu user không đồng ý |
| `callback.save.onSubmit` | Lưu / chốt timesheet | `async () => callSaveTimesheet()` | Bấm nút Lưu trên toolbar |
| `logs` + `loading` | Controlled — host tự fetch | `logs` từ React Query | Page **không** gọi `list` |

```tsx
async function loadTimesheetBoard(query: TimesheetQuery) {
  // Lấy logs theo from/to, projectIds, userIds, groupBy, grain
  return callGetTimesheetBoard(query)
}

async function updateCellHours(payload: UpdateTimesheetHoursPayload) {
  // Cập nhật 1 ô giờ (project + member + ngày)
  return callUpdateTimesheetHours(payload)
}

async function saveTimesheet() {
  // Chốt / persist toàn bộ thay đổi pending
  return callSaveTimesheet()
}

<TimesheetPage
  locale="vi"
  title="Chấm công tháng này"
  callback={{
    list: {
      before: (query) => (query.from && query.to ? undefined : false),
      onSubmit: loadTimesheetBoard,
      success: () => notify.success('Đã tải bảng công'),
      error: ({ error }) => notify.error(getErrorMessage(error)),
    },
    updateHours: {
      onSubmit: updateCellHours,
      success: ({ result }) => {
        console.log('log updated', result.data.id)
      },
    },
    save: {
      before: () => (window.confirm('Lưu timesheet?') ? undefined : false),
      onSubmit: saveTimesheet,
      complete: () => console.log('save finished'),
    },
  }}
  content={(ctx) => (
    <>
      <p>
        Từ {ctx.from} → {ctx.to}
      </p>
      {ctx.DefaultLayout}
    </>
  )}
/>
```

**Controlled mode** (host tự fetch):

```tsx
const [logs, setLogs] = useState<TimesheetLog[]>([])

useEffect(() => {
  void callGetTimesheetBoard(query).then((res) => setLogs(res.data.logs))
}, [query])

<TimesheetPage logs={logs} loading={false} withShell title="Chấm công" />
```

### Props UI tùy chọn

`title`, `description`, `className`, `withShell`, `content`, `locale`.
