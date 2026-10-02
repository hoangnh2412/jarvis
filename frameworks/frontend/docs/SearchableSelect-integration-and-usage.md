# SearchableSelect & SearchableMultiSelect

Combobox chọn option **async**: search + debounce + phân trang infinite scroll. Filter do **BE** xử lý qua `loadOptions` — không filter local trên FE.

| Component | Chọn | Giá trị |
|-----------|------|---------|
| `SearchableSelect` | 1 option | `string \| number \| null` |
| `SearchableMultiSelect` | Nhiều option (chip) | `string[]` |

Hai component dùng chung contract `loadOptions` / `SearchableSelectLoadParams`.

---

## Export chính

```ts
import {
  SearchableSelect,
  SearchableMultiSelect,
  type SearchableSelectOption,
  type SearchableSelectProps,
  type SearchableSelectLoadParams,
  type SearchableSelectLoadResult,
  type SearchableSelectLoadOptions,
  type SearchableMultiSelectOption,
  type SearchableMultiSelectProps,
  type SearchableMultiSelectLoadOptions,
} from '@platform/core'
```

Source: `src/common/SearchableSelect.tsx`, `src/common/SearchableMultiSelect.tsx`. Hook nội bộ: `useSearchableAsyncOptions`.

---

## CSS (bắt buộc)

Style nằm trong kit theme:

```css
@import "@platform/core/styles.css";
```

Class chính: `.kit-searchable-select*`, `.kit-searchable-multi-select*`.

---

## Khi nào dùng

- Dropdown cần **search remote** (tenant, user, project, …) — danh sách lớn, không nhét hết vào FE.
- Cần **phân trang** khi cuộn list (`page` / `pageSize`, mặc định 10).
- Multi: filter nhiều id cùng lúc (vd. Timesheet toolbar).

**Không** dùng khi chỉ có vài option tĩnh — `SearchableSelect` có mode `searchable={false}` (Prime Select, list local).

---

## Contract `loadOptions` (chung)

```ts
type SearchableSelectLoadParams = {
  search: string
  signal?: AbortSignal
  context?: Record<string, unknown>
  page: number      // bắt đầu từ 1
  pageSize: number  // mặc định 10
}

type SearchableSelectLoadResult =
  | SearchableSelectOption[]
  | { options: SearchableSelectOption[]; hasMore?: boolean }

type SearchableSelectOption = {
  label: string
  value: string | number
}
```

`loadOptions` được gọi khi mở dropdown / đổi search (debounce) / cuộn gần cuối list. Component truyền `AbortSignal` — hủy request cũ khi search đổi.

`hasMore`:

- Có truyền → dùng trực tiếp.
- Không truyền → suy ra `options.length >= pageSize`.

```tsx
async function loadUsers(
  params: SearchableSelectLoadParams,
): Promise<SearchableSelectLoadResult> {
  const res = await axios.get('/api/v1/users', {
    params: {
      q: params.search,
      page: params.page,
      pageSize: params.pageSize,
    },
    signal: params.signal,
  })
  return {
    options: res.data.items.map((u: { id: string; name: string }) => ({
      label: u.name,
      value: u.id,
    })),
    hasMore: res.data.hasMore,
  }
}
```

`context` — extra query theo trang (tenantId, role, …) map vào API nếu cần:

```tsx
<SearchableSelect
  value={userId}
  context={{ tenantId }}
  loadOptions={async ({ search, page, pageSize, context, signal }) => {
    return fetchUsers({
      search,
      page,
      pageSize,
      tenantId: context?.tenantId as string,
      signal,
    })
  }}
  onValueChange={setUserId}
/>
```

---

## Embed — `SearchableSelect` (chọn 1)

```tsx
import { useState } from 'react'
import { SearchableSelect, type SearchableSelectLoadParams } from '@platform/core'

async function loadOptions(params: SearchableSelectLoadParams) {
  const res = await callGetUsers(params)
  return { options: res.data.options, hasMore: res.data.hasMore }
}

function UserPicker() {
  const [userId, setUserId] = useState<string | number | ''>('')

  return (
    <SearchableSelect
      value={userId}
      loadOptions={loadOptions}
      placeholder="Chọn người dùng…"
      emptyMessage="Không có kết quả"
      onValueChange={(v) => setUserId(v ?? '')}
    />
  )
}
```

### Mode không search (list tĩnh)

```tsx
<SearchableSelect
  searchable={false}
  value={status}
  options={[
    { label: 'Active', value: 'active' },
    { label: 'Inactive', value: 'inactive' },
  ]}
  onValueChange={(v) => setStatus(v ?? '')}
/>
```

Khi `searchable={true}` (mặc định) mà thiếu `loadOptions` → **throw**.

---

## Embed — `SearchableMultiSelect` (chọn nhiều)

```tsx
import { useState } from 'react'
import {
  SearchableMultiSelect,
  type SearchableSelectLoadParams,
} from '@platform/core'

async function loadOptions(params: SearchableSelectLoadParams) {
  const res = await callGetTimesheetOptions(params)
  return res.data // { options, hasMore }
}

function ProjectFilter() {
  const [ids, setIds] = useState<string[]>([])

  return (
    <SearchableMultiSelect
      value={ids}
      loadOptions={loadOptions}
      placeholder="Filter by"
      emptyMessage="No results"
      onValueChange={setIds}
    />
  )
}
```

Option có thể kèm `group` để hiện nhóm trong popup:

```ts
type SearchableMultiSelectOption = SearchableSelectOption & {
  group?: string
}
```

```ts
return {
  options: [
    { label: 'Alpha', value: 'p1', group: 'Projects' },
    { label: 'An', value: 'u1', group: 'Users' },
  ],
  hasMore: false,
}
```

---

## Dùng sẵn trong Timesheet

`TimesheetToolbar` embed `SearchableMultiSelect` + `callGetTimesheetOptions`:

```tsx
<SearchableMultiSelect
  value={filterIds}
  loadOptions={loadFilterOptions}
  placeholder={messages.page.filterBy}
  onValueChange={onFilterChange}
/>
```

API: `GET v1/timesheet/options?page=&pageSize=&search=`.

---

## Props dự án khác phải custom

### Props dùng chung (cả hai)

| Prop | Ý nghĩa dễ hiểu | Ví dụ truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|------------------|---------------------------|
| `loadOptions` | **Bắt buộc** (async mode) — gọi BE lấy option | `async (p) => callGetUsers(p)` | Mở / search / cuộn list |
| `context` | Extra param theo trang | `{ tenantId: 't1' }` | Gửi kèm mỗi lần `loadOptions` |
| `debounceMs` | Chờ trước khi search | `300` (mặc định) | Giảm số request khi gõ |
| `pageSize` | Số item mỗi lần load | `10` | Infinite scroll |
| `placeholder` | Placeholder ô nhập | `"Chọn…"` | Khi chưa chọn |
| `emptyMessage` | List trống | `"Không có kết quả"` | Sau load không có option |
| `loadingMessage` | Đang tải trang 1 | `"Đang tải…"` | Lần fetch đầu |
| `loadingMoreMessage` | Đang tải thêm | `"Đang tải thêm…"` | Cuộn cuối list |
| `disabled` | Khóa control | `true` | Không mở / không sửa |
| `triggerClassName` / `inputClassName` / `popupClassName` / `optionClassName` | Style | class Tailwind / kit | Tuỳ theme |

### `SearchableSelect` — khác biệt

| Prop | Ý nghĩa dễ hiểu | Ví dụ truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|------------------|---------------------------|
| `value` | Id đang chọn | `userId` | Hiện label đã chọn |
| `onValueChange` | Đổi chọn (1 giá trị) | `(v) => setUserId(v ?? '')` | Click option / clear → `null` |
| `searchable` | Bật combobox async | `true` (mặc định) / `false` | `false` → Prime Select + `options` |
| `options` | List tĩnh | `[{ label, value }]` | Chỉ khi `searchable={false}` |
| `searchPlaceholder` | (reserved / legacy) | — | Combobox dùng `placeholder` |
| `renderValue` | Custom trigger text | `(opt) => opt?.label` | Mode `searchable={false}` |
| `indicatorClassName` | Style chevron | `"text-slate-400"` | Icon mở list |

```tsx
async function loadTenants(params: SearchableSelectLoadParams) {
  const res = await axios.get('/api/v1/tenants', {
    params: { q: params.search, page: params.page, size: params.pageSize },
    signal: params.signal,
  })
  return {
    options: res.data.items.map((t: { id: string; name: string }) => ({
      label: t.name,
      value: t.id,
    })),
    hasMore: res.data.hasMore,
  }
}

<SearchableSelect
  value={tenantId}
  loadOptions={loadTenants}
  placeholder="Chọn tenant"
  onValueChange={(v) => setTenantId(v ?? '')}
/>
```

### `SearchableMultiSelect` — khác biệt

| Prop | Ý nghĩa dễ hiểu | Ví dụ truyền vào | Khi truyền vào thì làm gì |
|------|-----------------|------------------|---------------------------|
| `value` | **Bắt buộc** — mảng id đã chọn | `filterIds` | Hiện chip; Backspace xóa chip cuối |
| `onValueChange` | Đổi mảng id | `setFilterIds` | Toggle option / gỡ chip |
| `loadOptions` | **Bắt buộc** (luôn async) | như trên | Không có mode list tĩnh |
| `renderIndicator` | Icon bên phải trigger | `() => <Filter />` | Thay icon Filter mặc định |

```tsx
async function loadFilterOptions(params: SearchableSelectLoadParams) {
  const res = await callGetTimesheetOptions(params)
  return res.data
}

<SearchableMultiSelect
  value={filterIds}
  loadOptions={loadFilterOptions}
  placeholder="Filter by"
  onValueChange={setFilterIds}
/>
```

---

## Hành vi cần nhớ

- Search **debounce** mặc định 300ms.
- Infinite scroll: gần cuối list → `page + 1`.
- Label đã chọn được **cache** trong component — vẫn hiện đúng khi option không còn trong trang hiện tại.
- Multi: `value` là `string[]` (id được `String()` hóa so khớp).
- Single: `onValueChange` có thể trả `null` khi clear (mode basic).

---

## File liên quan

| File | Vai trò |
|------|---------|
| `src/common/SearchableSelect.tsx` | Single select |
| `src/common/SearchableMultiSelect.tsx` | Multi select |
| `src/hooks/useSearchableAsyncOptions.ts` | Load / phân trang / abort |
| `src/styles/kit.css` | Style `.kit-searchable-*` |
| Timesheet toolbar | Ví dụ multi + `callGetTimesheetOptions` |
