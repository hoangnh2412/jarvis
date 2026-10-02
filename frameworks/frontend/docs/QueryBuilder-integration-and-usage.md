# Query Builder

Bộ lọc nâng cao (AND/OR, nhiều field). Export UI + utilities — không có page riêng; gắn vào page host.

---

## Export chính

```ts
import {
  QueryBuilder,
  QueryBuilderApiToolbar,
  QueryBuilderResultsPanel,
  useQueryBuilderState,
  callGetCompanyEmployees,
  callGetQueryBuilderFields,
  getQueryBuilderFieldsMock,
  getCompanyEmployeesMock,
  toFilterAst,
  toFilterJson,
  EMPTY_QUERY,
} from '@platform/core'
```

---

## CSS (bắt buộc)

```css
@import "@platform/core/queryBuilder.css";
```

Peer: `react-querybuilder`, `@react-querybuilder/dnd`.

---

## Embed cơ bản

```tsx
import { QueryBuilder, useQueryBuilderState } from '@platform/core'

function FilterPanel() {
  const state = useQueryBuilderState({ loadFields: true, locale: 'vi' })

  return (
    <QueryBuilder
      fields={state.fields}
      query={state.query}
      onQueryChange={state.setQuery}
      locale={state.locale}
    />
  )
}
```

---

## Gắn trên list page

Một số list page đã hỗ trợ sẵn — bật/tắt bằng prop (ví dụ):

```tsx
<TenantListPage withQueryBuilder={true} locale="vi" />
```

Trên list demo, search gọi API **employees** — không filter trực tiếp danh sách entity trên page.

---

## Toolbar + kết quả

```tsx
<QueryBuilderApiToolbar
  loading={state.loading}
  onSearch={() => state.search()}
  onClear={() => state.clear()}
/>

<QueryBuilderResultsPanel
  loading={state.resultsLoading}
  rows={state.results?.items ?? []}
  total={state.results?.total ?? 0}
/>
```

---

## Fields

```ts
import { getFakeQueryBuilderFields, getQueryBuilderFieldsMock } from '@platform/core'

// Fake (dev)
const fields = getFakeQueryBuilderFields()

// Hoặc từ JSON seed
const fields = getQueryBuilderFieldsMock()
```

Mock JSON: `features/queryBuilder/mocks/get-fields.json`, `get-employees.json`.

---

## API


| Service                             | Method | Path                             |
| ----------------------------------- | ------ | -------------------------------- |
| `callGetQueryBuilderFields`         | GET    | `v1/query/fields`                |
| `callGetCompanyEmployees`           | POST   | `v1/query/employees`             |
| `callGetCompanyEmployeesListCustom` | POST   | `v1/query/employees/list-custom` |


Body search: filter AST/JSON từ `toFilterAst(query)` hoặc `toFilterJson(query)`.

---

## Utilities thường dùng


| Hàm                     | Mục đích                       |
| ----------------------- | ------------------------------ |
| `toFilterAst`           | Chuyển query UI → AST backend  |
| `toFilterParams`        | Chuyển → query params          |
| `toFilterJson`          | Serialize JSON                 |
| `fromFilterAst`         | Parse ngược từ AST             |
| `validateFilterAst`     | Validate trước khi gọi API     |
| `prepareEmployeeFilter` | Chuẩn hóa filter trước khi gửi |


Giới hạn: `FILTER_MAX_DEPTH`, `FILTER_MAX_CONDITIONS`.

---

## Tùy biến

Không có page wrapper `callback`/`content`. Tùy biến bằng props UI và hook state.

### Embed trong page khác

```tsx
const state = useQueryBuilderState({
  loadFields: true,
  locale: 'vi',
})

<QueryBuilder
  fields={state.fields}
  query={state.query}
  onQueryChange={state.setQuery}
  locale={state.locale}
  disabled={state.loading}
/>
```

### Thay API fields / search

```tsx
const state = useQueryBuilderState({
  loadFields: false,
  initialFields: customFields,
})

<QueryBuilderApiToolbar
  onSearch={() => runSearch(toFilterJson(state.query))}
/>
```

### List page — bật/tắt

```tsx
<TenantListPage withQueryBuilder={false} />
```

### Locale & fields fake

```tsx
import { getFakeQueryBuilderFields, getQueryBuilderMessages } from '@platform/core'

const fields = getFakeQueryBuilderFields()
const messages = getQueryBuilderMessages('en')
```

---

## Props dự án khác phải custom

Feature **không có page** — host gắn UI filter và tự nối search API. Các prop hàm dưới đây là chỗ custom chính.

### Builder — props phải custom


| Prop                  | Ý nghĩa                                     | Ví dụ                                               | Khi truyền vào thì làm gì                       |
| --------------------- | ------------------------------------------- | --------------------------------------------------- | ----------------------------------------------- |
| `fields`              | Catalog field cho rule (tên, toán tử, kiểu) | `state.fields` hoặc `getFakeQueryBuilderFields()`   | Select field trong mỗi điều kiện                |
| `query`               | Query đang hiển thị (controlled)            | `state.query`                                       | Đồng bộ UI với state host                       |
| `onQueryChange`       | Mỗi lần user thêm/sửa/xóa rule              | `(query) => state.setQuery(query)`                  | Cập nhật state; thường gắn `toFilterAst` sau đó |
| `onAstChange`         | AST sau mỗi lần đổi (null nếu rỗng)         | `(ast) => setFilterAst(ast)`                        | Chuẩn bị body gọi BE                            |
| `onParamsChange`      | Object filter phẳng                         | `(params) => setApiFilter(params)`                  | Gắn query string / body đơn giản                |
| `onPagedParamsChange` | Params list (không gồm page/size)           | `(params) => setListQuery(prev => ({…prev, …params, page: 1}))` | Reset về trang 1 khi đổi filter                 |
| `disabled`            | Khóa UI khi đang load                       | `state.loading`                                     | Không cho sửa rule lúc fetch fields             |


```tsx
function handleQueryChange(query: RuleGroupType) {
  state.setQuery(query)
}

function handleAstChange(ast: FilterAst | null) {
  setFilterAst(ast)
}

function handleParamsChange(params: FilterParams) {
  setApiFilter(params)
}

function handlePagedParamsChange(
  params: Omit<FilterParams & { page?: number; size?: number }, 'page' | 'size'>,
) {
  setListQuery((prev) => ({ ...prev, ...params, page: 1 }))
}

<QueryBuilder
  fields={state.fields}
  query={state.query}
  disabled={state.loading}
  onQueryChange={handleQueryChange}
  onAstChange={handleAstChange}
  onParamsChange={handleParamsChange}
  onPagedParamsChange={handlePagedParamsChange}
/>
```

### Toolbar — hàm host tự nối


| Prop        | Ý nghĩa           | Ví dụ                                         | Khi truyền vào              |
| ----------- | ----------------- | --------------------------------------------- | --------------------------- |
| `onSearch`  | Bấm Tìm           | `() => runSearch(toFilterJson(state.query))`  | Gọi API với filter hiện tại |
| `onClear`   | Bấm Xóa điều kiện | `() => state.clear()`                         | Reset query + kết quả       |


```tsx
async function handleSearch() {
  const body = toFilterJson(state.query)
  const res = await callGetCompanyEmployees(body)
  setRows(res.data.items)
}

<QueryBuilderApiToolbar
  loading={state.loading}
  onSearch={handleSearch}
  onClear={() => state.clear()}
/>
```

### Hook state — options thường custom


| Option                                | Ý nghĩa                     | Ví dụ                        |
| ------------------------------------- | --------------------------- | ---------------------------- |
| `loadFields: true`                    | Tự gọi API catalog fields   | mặc định demo                |
| `loadFields: false` + `initialFields` | Dùng catalog field của host | `initialFields: customFields` |
| `locale`                              | Copy vi/en                  | `"vi"`                       |


### List page — bật/tắt

```tsx
<TenantListPage withQueryBuilder={false} />
```

---

## Ghi chú

- Control elements dùng **PrimeReact 11** (`primeReact11ControlElements`).
- Drag-drop rule qua Pragmatic DnD.
- Locale `vi` / `en` qua `getQueryBuilderMessages(locale)`.
