# ADR — Dynamic Paged List Query (Filter / Sort / Columns)

> **Trạng thái:** ✅ Accepted — đã chốt Q1–Q9. Sẵn sàng implement theo lộ trình mục 10.
> **Vấn đề:** Nâng cấp `PagedListRequest` + `PagedListExecutor` để hỗ trợ **filter động** (DSL dạng JSON), **sort động** (chuỗi), **column projection**, đồng thời cho phép **customize** phần Filter/Sort cho case phức tạp không thể dynamic. Các bộ xử lý chuỗi viết dạng **Extension tái dùng được**.
> **Liên quan:** [`Jarvis.Domain/Repositories/PagedListRequest.cs`](../Jarvis.Domain/Repositories/PagedListRequest.cs), [`Jarvis.EntityFramework/Repositories/PagedListExecutor.cs`](../Jarvis.EntityFramework/Repositories/PagedListExecutor.cs), [`Jarvis.EntityFramework/Helpers/QueryFilterExpressionHelper.cs`](../Jarvis.EntityFramework/Helpers/QueryFilterExpressionHelper.cs).

---

## 1. Khuyến nghị (tóm tắt để chốt nhanh)

1. **Đổi tên field thành `Page`/`Size` (1-based)** theo yêu cầu — **breaking change**, cần migration (mục 3 + mục 11).
2. **Filter DSL = định dạng nested-array kiểu DevExtreme** bạn đưa ra. Parse thành cây AST provider-agnostic (`Jarvis.Domain`), rồi dịch sang `Expression<Func<T,bool>>` ở tầng EF.
3. **Kiểu dữ liệu của `RightOpt` suy ra từ kiểu property của entity** (reflection), **không** nhét type-token vào DSL. Đơn giản hoá payload, tránh client và server lệch kiểu.
4. **Bắt buộc whitelist field** (opt-in per query type) để chống lộ cột nhạy cảm và DoS; giới hạn độ sâu lồng nhau.
5. **Customize** qua một `options`/builder cho phép: (a) predicate tĩnh cộng dồn vào filter động, (b) override hoàn toàn Sort/Filter, (c) map field-name → expression cho field tính toán.
6. **Sort mặc định phải xác định (deterministic)** — vì `Skip/Take` hiện không `OrderBy` là bug tiềm ẩn. Mặc định `UpdatedAt desc, CreatedAt asc`, fallback tie-break theo `Id`.

---

## 2. Hiện trạng

```csharp
// PagedListRequest.cs — hiện tại
public sealed class PagedListRequest
{
    public int PageIndex { get; init; }   // zero-based
    public int PageSize  { get; init; }
    public string? Columns { get; init; } // "FirstName,LastName"
}
```

```csharp
// PagedListExecutor.cs — hiện tại: KHÔNG có OrderBy trước Skip/Take
q = q.ApplyColumnSelection(paging.Columns);
var items = await q.Skip(PageIndex * PageSize).Take(PageSize).ToListAsync();
// mới (1-based): q.OrderBy(...).Skip((Page - 1) * Size).Take(Size)
```

⚠️ **Latent bug:** phân trang không `OrderBy` → thứ tự do DB quyết định, không ổn định giữa các trang. Thêm Sort (mục 6) vá luôn điểm này.

---

## 3. `PagedListRequest` mới — các field

| Field | Kiểu | Ý nghĩa | Mặc định |
|---|---|---|---|
| `Page` | `int` | Trang hiện tại, **1-based** (trang đầu = 1) | (bắt buộc ≥ 1) |
| `Size` | `int` | Số bản ghi/trang | (bắt buộc > 0) |
| `Filter` | `string?` | JSON nested-array (mục 4) | `null` → không lọc |
| `Sort` | `string?` | `"FirstName:asc,LastName:desc"` (mục 6) | `null` → `UpdatedAt:desc,CreatedAt:asc` |
| `Columns` | `string?` | Danh sách field trả về (đã có) | `null` → tất cả |

**✅ ĐÃ CHỐT — `Page`/`Size`, 1-based.**
- Validate: `Page ≥ 1`, `Size > 0`.
- Công thức phân trang đổi thành `Skip((Page - 1) * Size).Take(Size)`.
- **Breaking change** — xem migration mục 11.

---

## 4. Filter DSL — cú pháp & ngữ nghĩa

### 4.1 Grammar

Định dạng là **cây nhị phân dạng nested JSON array** (trùng chuẩn filter của DevExtreme DataGrid):

```
Node      := Condition | Group
Condition := [ LeftOpt, Operator, RightOpt ]          // 3 phần tử
Group     := [ Node, LogicOp, Node (, LogicOp, Node)* ] // số phần tử lẻ, xen kẽ
LogicOp   := "AND" | "OR"
```

- **`LeftOpt`** = tên field trên entity (string).
- **`Operator`** = toán tử so sánh (mục 4.3).
- **`RightOpt`** = giá trị (mục 4.2); với `between`/`in` là **mảng**.

**Phân biệt Condition vs Group (quy tắc parser):**
- Phần tử tại **chỉ số lẻ** của một Group luôn là `LogicOp` (`AND`/`OR`).
- Một mảng là **Condition** khi có đúng 3 phần tử và `element[1]` là toán tử so sánh; ngược lại nếu `element[1]` ∈ {AND, OR} thì là **Group**.

### 4.2 Kiểu dữ liệu `RightOpt`

Yêu cầu liệt kê: `bool`, `int`, `string`, `datetime`, `date`.

**✅ ĐÃ CHỐT — suy ra kiểu từ property của entity qua reflection**, rồi convert chuỗi → CLR type bằng `InvariantCulture`. Client chỉ gửi string, server tự ép kiểu đúng theo cột. Lợi: payload gọn, không lệch kiểu client/server.

| Kiểu logic | CLR đích | Parse |
|---|---|---|
| bool | `bool` | `true`/`false` (case-insensitive) |
| int | `int`/`long`/`short` | `int.Parse(Invariant)` |
| string | `string` | nguyên văn |
| datetime | `DateTime`/`DateTimeOffset` | ISO-8601 `2026-01-01T10:00:00.000Z`, ép về UTC |
| date | `DateOnly`/`DateTime` (00:00) | `yyyy-MM-dd` |

**✅ ĐÃ CHỐT — bổ sung đầy đủ:** ngoài `bool`/`int`/`string`/`datetime`/`date`, hỗ trợ thêm:
- **`decimal`/`double`/`float`** — tiền tệ, số thực.
- **`Guid`** — cho `Id` và các khóa Guid.
- **`enum`** — parse theo tên hoặc giá trị số (`Enum.TryParse`, ignore-case).
- **`null`** — cho `isnull`/`isnotnull` (mục 4.3).

### 4.3 Toán tử

Yêu cầu liệt kê: `=`, `>`, `<`, `<=`, `>=`, `contains`, `between`.

| Toán tử | Ngữ nghĩa | Ghi chú |
|---|---|---|
| `=` | bằng | |
| `>` `<` `>=` `<=` | so sánh | chỉ cho kiểu so sánh được |
| `contains` | chuỗi chứa | dịch sang `LIKE '%v%'` (xem 4.4) |
| `between` | trong khoảng `[a,b]` | `RightOpt` = mảng 2 phần tử `["a","b"]`, inclusive |

**✅ ĐÃ CHỐT — bổ sung đầy đủ:**
- **`!=` / `<>`** — khác.
- **`in`** — thuộc tập, `RightOpt` = mảng nhiều phần tử.
- **`startswith` / `endswith`** — dịch `LIKE 'v%'` / `LIKE '%v'`.
- **`notcontains`** — phủ định `contains`.
- **`isnull` / `isnotnull`** — kiểm null (nullable field), `RightOpt` bỏ trống/`null`.

### 4.4 `contains` và collation (case sensitivity)

- Dịch `contains` bằng `string.Contains` (EF → `LIKE '%v%'`) hoặc `EF.Functions.Like` để escape ký tự `%`, `_`.
- **Case-sensitive hay không phụ thuộc collation của DB**, không tự ép `.ToLower()` (phá index). → Ghi rõ trong doc là hành vi theo collation.

### 4.5 Precedence & an toàn

- **Không cho trộn AND/OR ở cùng một cấp** trừ khi cùng loại — muốn ưu tiên phải **lồng mảng** (đúng ngữ nghĩa DevExtreme). Parser sẽ báo lỗi nếu một Group trộn `AND` và `OR`. → Loại bỏ mơ hồ độ ưu tiên.
- **Giới hạn độ sâu lồng** (vd ≤ 8) và **số điều kiện** (vd ≤ 50) để chống payload độc hại (DoS build expression).
- **Whitelist field** (mục 7) áp cho `LeftOpt`.

---

## 5. Customize Filter / Sort (case phức tạp)

Yêu cầu #2: cho phép can thiệp khi không thể dynamic. Đề xuất một `options` truyền vào executor:

**✅ ĐÃ CHỐT — giai đoạn 1 chỉ `Predicate` + `CustomFilter`/`CustomSort`** (theo Simplicity First; `FieldMap` để sau khi có nhu cầu thật).

```csharp
public sealed class PagedQueryOptions<TEntity>
{
    // Whitelist field cho LeftOpt/Sort/Columns (BẮT BUỘC — mục 7). Rỗng = chặn hết filter/sort động.
    public ISet<string>? AllowedFields { get; init; }

    // Cộng dồn (AND) predicate tĩnh vào filter động — vd scope tenant, soft-delete
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }

    // Override HOÀN TOÀN: nếu set thì bỏ qua parser tương ứng
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? CustomFilter { get; init; }
    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? CustomSort { get; init; }
}
```

Thứ tự áp dụng: `Predicate` (tĩnh) → `Filter` (động từ DSL) → `CustomFilter` (nếu có, override) → `Sort`/`CustomSort` → `Columns` → `Skip/Take`.

---

## 6. Sort

- Cú pháp: `"FirstName:asc,LastName:desc"` → build chuỗi `OrderBy/ThenBy`.
- **Mặc định khi trống:** `UpdatedAt desc, CreatedAt asc`.

**✅ ĐÃ XÁC MINH interface audit có sẵn trong `Jarvis.Domain.Entities`:**
- `ILogUpdatedEntity` → `DateTime UpdatedAt` (+ `UpdatedBy`).
- `ILogCreatedEntity` → `DateTime CreatedAt` (+ `CreatedBy`).

Logic default sort (deterministic, guard bằng interface):
1. Nếu `TEntity is ILogUpdatedEntity` → `OrderByDescending(UpdatedAt)`.
2. Nếu `TEntity is ILogCreatedEntity` → `ThenBy(CreatedAt)`.
3. **Tie-break theo `Id`** để phân trang ổn định.

**⚠️ Cạm bẫy `Id`:** ràng buộc repository là `where TEntity : class, IEntity`, mà **`IEntity` (non-generic) KHÔNG có `Id`** — chỉ `IEntity<TKey>` mới có. → Không cast cứng sang `Id` được; phải **dò `Id` bằng reflection** (như `QueryableColumnSelectHelper` đang làm) rồi build `OrderBy` động; nếu entity không có cả audit lẫn `Id` thì không thể bảo đảm deterministic → ném lỗi rõ ràng yêu cầu chỉ định `Sort`.

---

## 7. Bảo mật & giới hạn

1. **✅ Whitelist field BẮT BUỘC cho `LeftOpt`, `Sort`, `Columns`** — không cho phép mọi public property tuỳ tiện (tránh lộ `PasswordHash`, sort theo cột không index…). Cơ chế: `PagedQueryOptions.AllowedFields` (opt-in danh sách field theo từng query type); field ngoài danh sách → ném lỗi rõ ràng. Field không nằm trong whitelist mặc định **bị chặn**.
2. **Giới hạn** độ sâu lồng filter, tổng số điều kiện, `PageSize` tối đa.
3. **`contains`/`like`**: escape `%`, `_` qua `EF.Functions.Like`.
4. **Reflection an toàn**: chỉ property `Public|Instance`, đúng tên, ném lỗi rõ ràng khi field lạ (đã có tiền lệ ở `QueryableColumnSelectHelper`).

---

## 8. Kiến trúc & phân tầng (Extension tái dùng)

Yêu cầu #3: viết dạng Extension dùng lại được. Tách 2 giai đoạn theo đúng ranh giới Clean Architecture:

```text
Jarvis.Domain (provider-agnostic)
  Repositories/PagedListRequest.cs        ← thêm Filter, Sort
  Querying/FilterNode.cs, FilterParser    ← string JSON  ->  AST (không phụ thuộc EF)
  Querying/SortParser                     ← string       ->  danh sách (field, direction)

Jarvis.EntityFramework (provider-specific)
  Extensions/QueryableFilterExtensions.cs ← AST     -> IQueryable.Where(Expression)
  Extensions/QueryableSortExtensions.cs   ← sort    -> IQueryable.OrderBy/ThenBy
  Extensions/QueryableColumnExtensions.cs ← (chuyển ApplyColumnSelection về đây)
  Repositories/PagedListExecutor.cs       ← orchestrate + PagedQueryOptions
```

- **Parse (string → AST)** là logic thuần, không đụng EF → đặt ở `Jarvis.Domain`, test đơn giản.
- **Translate (AST → Expression)** cần `EF.Functions.Like`, `DbContext` → đặt ở `Jarvis.EntityFramework`.
- Tất cả là `static` extension methods trên `string`/`IQueryable<T>` → tái dùng ở bất kỳ repository nào.

**✅ ĐÃ CHỐT — parser ở `Jarvis.Domain`, translate ở `Jarvis.EntityFramework`.**

---

## 9. Câu hỏi mở cần chốt

| # | Quyết định | Kết quả |
|---|---|---|
| Q2 | Chiến lược kiểu `RightOpt` | ✅ **Infer từ entity** (reflection) |
| Q3 | Bổ sung kiểu | ✅ **Thêm `Guid`, `decimal/double`, `enum`, `null`** |
| Q4 | Bổ sung toán tử | ✅ **Thêm `!=`, `in`, `startswith/endswith`, `isnull/isnotnull`** (+`notcontains`) |
| Q5 | Mức customize giai đoạn 1 | ✅ **`Predicate` + `CustomFilter/CustomSort`** (bỏ `FieldMap`) |
| Q6 | Default sort | ✅ **`UpdatedAt desc, CreatedAt asc`** qua `ILogUpdatedEntity`/`ILogCreatedEntity`, tie-break `Id` (reflection) |
| Q1 | Tên field phân trang | ✅ **`Page`/`Size` (1-based)** — breaking, xem mục 11 |
| Q7 | Whitelist field | ✅ **Bắt buộc** qua `PagedQueryOptions.AllowedFields` |
| Q8 | Nơi đặt parser | ✅ **`Jarvis.Domain`** (parse) + `Jarvis.EntityFramework` (translate) |
| Q9 | Đối chiếu `IPagingDto` hiện có (mục 12) | ✅ **Đồng bộ `IPagingDto`/`IPagedDto` sang DSL string + `Page/Size`** |

---

## 10. Lộ trình implement (sau khi chốt)

1. Đổi `PagedListRequest`: `Page`/`Size` (1-based) + thêm `Filter`, `Sort` → verify: build.
2. `Jarvis.Domain/Querying`: `FilterParser` (string → AST), `SortParser` → verify: `FilterParserTests`, `SortParserTests` (13.1–13.2).
3. `Jarvis.EntityFramework/Extensions`: translate AST → `Expression`, sort, column → verify: `QueryFilterTranslateTests` (13.3).
4. `PagedQueryOptions` (whitelist + predicate + override) + `PagedListExecutor` (`OrderBy` deterministic, `Skip((Page-1)*Size)`) → verify: `QueryPagingExecutorTests`, `QueryWhitelistTests`, `QueryCustomizeTests` (13.4–13.6).
5. Cập nhật `IQueryRepository.PaginationAsync` nhận `options` (overload) → verify: `Sample` chạy Swagger.
6. Đồng bộ `IPagingDto`/`IPagedDto` (`Jarvis.Application.Contracts`): `Filters`/`Sort` (Dict) → `string Filter`/`string Sort`; `PageIndex/PageSize` → `Page/Size` → verify: `dotnet test` toàn solution xanh.

---

## 11. Migration cho breaking change `Page`/`Size`

Đổi `PageIndex/PageSize` → `Page/Size` (1-based) ảnh hưởng các nơi (đã rà, không tính `obj/bin`):

| File | Ảnh hưởng |
|---|---|
| `Jarvis.Domain/Repositories/PagedListRequest.cs` | Định nghĩa — đổi field |
| `Jarvis.EntityFramework/Repositories/PagedListExecutor.cs` | Validate + công thức `Skip` (đổi sang `(Page-1)*Size`) |
| `Jarvis.EntityFramework/Helpers/PagedListRequestParser.cs` | Liên quan (parse) |

> ⚠️ `IPagingDto`/`IPagedDto` (`Jarvis.Application.Contracts`) vẫn dùng `PageIndex/PageSize` — xem mục 12 để quyết có đồng bộ tên/base không.

---

## 12. ❗ Q9 — Đối chiếu với `IPagingDto` hiện có (mới phát hiện)

`Jarvis.Application.Contracts/DTOs/IPagingDto` **đã tồn tại** và mô tả input phân trang **theo mô hình khác**:

```csharp
public interface IPagingDto
{
    int PageSize { get; set; }
    int PageIndex { get; set; }
    Dictionary<string, string> Filters { get; set; }   // Key=field, Value=value  ← KHÔNG có operator/AND-OR
    Dictionary<string, string> Sort { get; set; }       // Key=field, Value=asc/desc
    IEnumerable<string> Columns { get; set; }
}
```

**Mâu thuẫn cần chốt:**
1. **Filter**: `IPagingDto.Filters` là `Dictionary<field, value>` — **không** biểu diễn được toán tử (`>`, `contains`, `between`) hay logic lồng `AND/OR` của DSL mới. → Nếu đây là contract mà client thật sự gửi, DSL nested-array **không khớp**. Cần quyết: (a) `IPagingDto.Filters` nâng thành `string Filter` (JSON DSL) cho khớp `PagedListRequest`, hay (b) DSL chỉ dùng nội bộ và có tầng map riêng.
2. **Tên field**: DTO dùng `PageIndex/PageSize`, `PagedListRequest` sắp đổi `Page/Size` → **lệch tên giữa 2 tầng**. Nên đồng bộ (cùng `Page/Size`) hay giữ khác nhau và map ở boundary?
3. **Ai map `IPagingDto` → `PagedListRequest`?** Hiện chưa thấy chỗ map — cần xác định tầng chịu trách nhiệm (Application?).

> **✅ ĐÃ CHỐT — đồng bộ về một chuẩn DSL string:**
> - `IPagingDto.Filters` (Dict) → `string Filter` (JSON DSL); `IPagingDto.Sort` (Dict) → `string Sort` (`"field:asc,..."`).
> - `IPagingDto`/`IPagedDto`: `PageIndex/PageSize` → `Page/Size` (1-based) cho khớp `PagedListRequest`.
> - Map `IPagingDto` → `PagedListRequest` đặt ở tầng Application (boundary), sẽ xác định chính xác khi implement bước 6.

---

## 13. Kế hoạch Unit Test

Dùng lại hạ tầng test hiện có: **xUnit** + **`Microsoft.EntityFrameworkCore.InMemory`** (đã có trong `UnitTest/UnitTest.csproj`), theo convention `UnitTest/{Module}/...Tests.cs` (như `UnitTest/Multitenancy/MultitenancyEfTests.cs`). Đặt tests trong **`UnitTest/Querying/`**.

**Nguyên tắc:** test theo tiêu chí verifiable của lộ trình mục 10 (Goal-Driven). Parser test **thuần, không EF**; translate/executor test qua **InMemory DbContext**.

### 13.1 `FilterParserTests` (Domain, thuần — không EF)
- **Grammar hợp lệ:** parse leaf `["Age",">","10"]`; group `AND`/`OR`; **lồng nhiều tầng** đúng như payload ví dụ ở đầu ADR → AST đúng cấu trúc.
- **Phân biệt leaf vs group:** mảng 3 phần tử với `element[1]` là compare-op ⇒ leaf; `element[1]` ∈ {AND,OR} ⇒ group.
- **Toán tử đặc biệt:** `between` (RightOpt = mảng 2), `in` (mảng n), `isnull/isnotnull` (RightOpt rỗng).
- **Lỗi phải ném rõ ràng:** JSON sai định dạng; trộn `AND`+`OR` cùng cấp (mục 4.5); toán tử lạ; số phần tử group chẵn; vượt **giới hạn độ sâu/số điều kiện**.

### 13.2 `SortParserTests` (Domain, thuần)
- `"FirstName:asc,LastName:desc"` → list `(field, direction)` đúng thứ tự.
- Chuỗi rỗng/null → rơi về default marker (`UpdatedAt desc, CreatedAt asc`).
- Direction thiếu → mặc định `asc`; direction lạ → ném lỗi.

### 13.3 `QueryFilterTranslateTests` (EF InMemory)
Trên entity mẫu (vd có `string FirstName`, `int Age`, `Guid Id`, `DateTime CreatedAt`, `bool IsActive`, `Status enum`, `decimal Amount`, nullable field):
- **Từng toán tử** ra đúng tập kết quả: `=`, `!=`, `>`, `<`, `>=`, `<=`, `contains`, `notcontains`, `startswith`, `endswith`, `in`, `between`, `isnull`, `isnotnull`.
- **Ép kiểu `RightOpt` theo property (Q2):** `"10"`→int, `"true"`→bool, Guid string→Guid, ISO datetime, `yyyy-MM-dd`→date, enum theo tên & số, decimal invariant-culture.
- **Logic lồng:** `(A OR B) AND C` cho kết quả đúng.
- **Predicate cộng dồn (Q5):** `PagedQueryOptions.Predicate` AND với filter động.

### 13.4 `QueryPagingExecutorTests` (EF InMemory)
- **Phân trang 1-based (Q1):** `Page=1/Size=10` trả 10 bản ghi đầu; `Page=2` trả tiếp; `Skip((Page-1)*Size)` đúng; `TotalCount` đúng.
- **Determinism (vá latent bug):** không truyền `Sort` → vẫn có `OrderBy` mặc định ổn định; hai trang không trùng/không sót bản ghi.
- **Default sort qua interface (Q6):** entity `ILogUpdatedEntity`+`ILogCreatedEntity` → sắp `UpdatedAt desc, CreatedAt asc`; entity **không** audit nhưng có `Id` → tie-break `Id`; entity không có cả hai → ném lỗi yêu cầu chỉ định `Sort`.
- **Columns:** projection chỉ field yêu cầu (đã có `ApplyColumnSelection`).
- **Validate:** `Page < 1` / `Size ≤ 0` → `ArgumentOutOfRangeException`.

### 13.5 `QueryWhitelistTests` (EF InMemory) — bảo mật (Q7)
- Field **ngoài** `AllowedFields` trong `Filter`/`Sort`/`Columns` → **ném lỗi**, không lộ cột.
- `AllowedFields` rỗng/null → chặn toàn bộ filter/sort động.

### 13.6 `QueryCustomizeTests` (EF InMemory) — Q5
- `CustomFilter` được set → **bỏ qua** parser filter, áp delegate.
- `CustomSort` được set → **bỏ qua** sort parser, áp delegate.
- Thứ tự áp dụng đúng: `Predicate` → `Filter` → `CustomFilter` → `Sort/CustomSort` → `Columns` → `Skip/Take`.

> Bổ sung bước verify tương ứng vào lộ trình mục 10: mỗi bước 2–4 kèm test ở 13.1–13.6 trước khi sang bước sau.
