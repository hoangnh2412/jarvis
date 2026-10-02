# Dashboard

Dashboard kéo-thả widget biểu đồ (**GridStack** + **Chart.js**). Catalog chart lấy từ API hoặc fake data.

---

## Export chính

```ts
import {
  DashboardPage,
  DASHBOARD_ROUTES,
  configureDashboardNavigate,
  callGetDashboardChartCatalog,
  getDashboardChartsMock,
} from '@platform/core'
```

---

## CSS (bắt buộc)

```css
@import "@platform/core/dashboard.css";
```

Peer: `gridstack`, `chart.js`, `react-chartjs-2`.

---

## Tích hợp

```tsx
import { DashboardPage, DASHBOARD_ROUTES } from '@platform/core'

<Route index element={<DashboardPage title="Tổng quan" />} />
<Route path={DASHBOARD_ROUTES.home} element={<DashboardPage />} />
```

`configureDashboardNavigate` — nếu feature có link nội bộ.

Layout lưu localStorage (`DASHBOARD_LAYOUT_STORAGE_KEY`).

---

## API HTTP thật (khuyến nghị)

| Service | Method | Path |
|---------|--------|------|
| `callGetDashboardChartCatalog` | GET | `v1/dashboard/charts` |

Response: `{ items: ChartCatalogItem[] }` — mỗi item có `data` (Chart.js), `settingsForm`, `settings`.

BE implement endpoint trên; seed contract: `features/dashboard/mocks/get-charts.json`.

---

## Fallback / giả lập

Nếu API lỗi hoặc rỗng → page tự dùng `getFakeDashboardChartCatalog()` từ mock JSON (dev/demo, không cần cấu hình thêm).

---

## Tùy biến

### Giao diện — `content`

Context `DashboardPageContentContext`: `charts`, `catalogLoading`, `openAddDialog`, `saveLayout`, `requestReset`, `reloadCatalog`, `DefaultHeader`, `DefaultCanvas`, `DefaultContent`.

```tsx
<DashboardPage
  title="Tổng quan"
  locale="vi"
  persistLayout={false}
  content={(ctx) => (
    <>
      {ctx.DefaultHeader}
      <KpiStrip charts={ctx.charts} />
      {ctx.DefaultCanvas}
    </>
  )}
/>
```

### Embed canvas riêng

Không dùng `DashboardPage` — ghép component lẻ:

```tsx
import { DashboardCanvas, useDashboardState, AddChartDialog } from '@platform/core'

const state = useDashboardState({ persistLayout: true })
<DashboardCanvas charts={state.charts} onChangeCharts={state.updateCharts} … />
```

| Prop | Tác dụng |
|------|----------|
| `persistLayout` | `false` = không lưu layout vào localStorage |
| `title`, `description`, `locale` | Header mặc định |

---

## Props dự án khác phải custom

`DashboardPage` **không** có `callback` API. Catalog chart load nội bộ (`callGetDashboardChartCatalog` / fake). Host chủ yếu custom UI + hành vi layout.

### `DashboardPage` — props thường custom

| Prop | Ý nghĩa dễ hiểu | Ví dụ truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|------------------|---------------------------|
| `persistLayout` | Có lưu vị trí chart xuống `localStorage` không | `false` | `false` = mỗi lần mở lại layout mặc định; `true` = nhớ layout user |
| `title` / `description` | Tiêu đề trang | `"Tổng quan vận hành"` | Hiện trên header mặc định |
| `locale` | Ngôn ngữ UI | `"vi"` | Copy header / dialog Thêm chart |
| `content` | Thay layout header + canvas | `(ctx) => (<>…</>)` | Bạn tự ghép toolbar / KPI / canvas |

`content` nhận context có **hàm hành động** (không phải prop gốc, nhưng host dùng khi custom UI):

| Hàm trong `ctx` | Ý nghĩa | Ví dụ gọi | Khi gọi thì làm gì |
|-----------------|---------|-----------|---------------------|
| `ctx.openAddDialog` | Mở dialog chọn chart từ catalog | `() => ctx.openAddDialog()` | User thêm widget mới |
| `ctx.saveLayout` | Ghi layout hiện tại (nếu `persistLayout`) | `() => ctx.saveLayout()` | Lưu vị trí/size xuống storage |
| `ctx.requestReset` | Reset layout về mặc định | `() => ctx.requestReset()` | Xóa layout đã lưu + reload |
| `ctx.reloadCatalog` | Tải lại danh sách chart từ API | `() => ctx.reloadCatalog()` | Refresh catalog khi BE đổi |

```tsx
function openAddChart(ctx: DashboardPageContentContext) {
  // Mở dialog chọn chart từ catalog đã load
  ctx.openAddDialog()
}

function saveCurrentLayout(ctx: DashboardPageContentContext) {
  // Persist grid positions vào localStorage (khi persistLayout=true)
  ctx.saveLayout()
}

<DashboardPage
  locale="vi"
  title="Tổng quan vận hành"
  description="Kéo thả chart trên canvas"
  persistLayout={false}
  content={(ctx) => (
    <>
      <button type="button" onClick={() => openAddChart(ctx)}>Thêm chart</button>
      <button type="button" onClick={() => saveCurrentLayout(ctx)}>Lưu layout</button>
      {ctx.DefaultHeader}
      <MyKpiStrip charts={ctx.charts} />
      {ctx.DefaultCanvas}
    </>
  )}
/>
```

### Muốn nguồn chart / API khác (không qua page props)

Page không nhận hàm fetch catalog. Embed component lẻ:

```tsx
import { DashboardCanvas, useDashboardState, AddChartDialog } from '@platform/core'

// Host tự load catalog rồi truyền charts vào canvas
const state = useDashboardState({ persistLayout: true })

<DashboardCanvas
  charts={state.charts}
  onChangeCharts={state.updateCharts}
/>
```

| Prop embed | Ý nghĩa | Ví dụ hàm | Khi truyền vào |
|------------|---------|-----------|----------------|
| `onChangeCharts` | User kéo/resize → cập nhật state | `(next) => setCharts(next)` | Mỗi lần đổi layout grid |

### Props UI tùy chọn

`className` — class wrapper.

---

## Ghi chú

- Animation CSS qua field `cssAnimation` trong settings.
- `ensureChartJsRegistered()` gọi trước khi render chart.
