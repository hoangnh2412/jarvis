# ADR — FluentValidation cho Application input

> **Trạng thái:** 🔴 **Proposed** (chờ confirm §7).  
> **Ngày:** 2026-08-07  
> **Loại:** API / Application layer / validation  
> **Liên quan:** [CRUD AppService](./2026-08-07-adr-jarvis-crud-app-service.md) (🔴), [architecture-rules.md](./architecture-rules.md) (§0.1 Clean Architecture), [README](../README.md) (Application = command/query/DTO).  
> **Phạm vi:** chọn **FluentValidation** làm chuẩn validate input ở tầng Application; wiring DI + chỗ gọi validate (CQRS dispatcher / AppService); map lỗi → `BadRequest` / `BaseResponse` hiện có.  
> **Ngoài phạm vi:** Domain invariant phức tạp (aggregate method); Authorization / RBAC; Localization catalog đầy đủ; thay `AddBadRequestHandler` DataAnnotations ModelState; AutoMapper; OpenAPI schema từ validator.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm  
> **Docs:** [FluentValidation](https://docs.fluentvalidation.net/en/latest/)

---

## 1. Bối cảnh

Jarvis đã có CQRS dispatcher (`Jarvis.DDD.Application`) và chuẩn lỗi HTTP (`BadRequestException`, `ApiResponseWrapperMiddleware`, `AddBadRequestHandler` cho ModelState). **Chưa** có chuẩn validate input Application (command / query / DTO CRUD):

| Thành phần | Hiện tại |
|------------|----------|
| `Jarvis.DDD.Application` | `CommandDispatcher` / `QueryDispatcher` — **không** bước validate trước handler |
| Host / Sample | Validate ad-hoc hoặc dựa ModelState / DataAnnotations rải rác |
| `Jarvis.Mvc` | `InvalidModelStateResponseFactory` → `BaseResponse` + field details |
| Domain | Business rule qua exception (`BadRequestException`, …) — không FluentValidation |
| ADR CRUD | Đề xuất `CrudAppService` — cần chỗ chuẩn validate `TCreateInput` / `TUpdateInput` |

[FluentValidation](https://docs.fluentvalidation.net/en/latest/) là thư viện .NET phổ biến cho rule typed (`AbstractValidator<T>`), hỗ trợ DI, async, cascade, test extensions. FV 12 hỗ trợ .NET 8+ (Jarvis đang .NET 9).

Clean Architecture: **input validation ở biên Application**; Domain giữ invariant nghiệp vụ (không phụ thuộc FluentValidation).

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không có convention validate Application | Mỗi host tự invent; lệch field error / HTTP 400 |
| P2 | Dispatcher gọi thẳng handler | Input invalid vào use case; khó test rule tách khỏi handler |
| P3 | Chỉ DataAnnotations / ModelState | Rule phức tạp (async DB unique, cross-property) khó; không tái dùng ngoài MVC |
| P4 | CRUD / CQRS sắp thêm | Không chốt validator → lặp `if` trong mỗi service/handler |
| P5 | Map lỗi FluentValidation chưa gắn `BaseResponseError` | Client nhận format khác ModelState / `BadRequestException` |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | FluentValidation **không** vào `Jarvis.DDD.Domain` / Domain.Shared (Domain thuần) |
| C2 | Validator sống Application (host Application project hoặc module Application) — validate **DTO / command / query**, không thay Domain invariant |
| C3 | Lỗi validation HTTP **400**; shape lỗi khớp `BaseResponse` / `BaseResponseErrorDetail` (field + codes) càng gần `AddBadRequestHandler` càng tốt |
| C4 | **Không** bắt buộc FluentValidation.AspNetCore auto-validation MVC làm đường chính — ưu tiên validate trong Application (docs FV khuyến nghị tách khỏi MVC pipeline cho app hiện đại) |
| C5 | Package FV: **FluentValidation** + **FluentValidation.DependencyInjectionExtensions** (scan assembly); bản FV **12.x** (hoặc LTS team chốt §7) |
| C6 | Commercial: FluentValidation free; dự án thương mại nên [sponsor](https://github.com/sponsors/JeremySkinner) — ngoài phạm vi Jarvis license |
| C7 | Semantic CQRS / exception hiện có **không** đổi ngoài chỗ gọi validate + map exception |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | **Wiring trong `Jarvis.DDD.Application`** — dispatcher (hoặc helper) resolve `IValidator<T>` trước handler | Một chỗ; CQRS + CRUD dùng chung; khớp Clean | Application reference FluentValidation |
| O2 | Package mỏng `Jarvis.FluentValidation` (bridge map lỗi + extension DI) | Opt-in Atomic; Application không cứng phụ thuộc FV | Thêm PackageId; Host phải compose |
| O3 | Chỉ convention + Sample (Host tự `ValidateAsync`) | Zero surface Jarvis | Không chuẩn hóa; dễ lệch |
| O4 | Giữ DataAnnotations / ModelState only | Đã có `AddBadRequestHandler` | Yếu cho rule async / tái dùng ngoài MVC |
| O5 | Auto-validation FluentValidation.AspNetCore trên Controllers | Ít code Host API | Lệch Application-first; FV đã hạ thấp khuyến nghị auto MVC |

**Khuyến nghị:** **O1** cho MVP (dispatcher + hook CRUD); tách **O2** Later nếu muốn Application không reference FV hoặc module không dùng CQRS Jarvis.

---

## 5. Quyết định

Chúng ta sẽ **chuẩn hóa FluentValidation** cho input Application; Jarvis gọi validate trước use case (CQRS / CRUD); map failure → lỗi 400 theo `BaseResponse`.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Thư viện | **FluentValidation** (+ DI extensions) — docs: [fluentvalidation.net](https://docs.fluentvalidation.net/en/latest/) |
| D2 | Vị trí rule | `AbstractValidator<T>` trong **Application** (host/module); `T` = command, query, create/update DTO |
| D3 | Wiring Jarvis (MVP) | `Jarvis.DDD.Application` reference FV; helper / pipeline validate trong dispatcher **khi** có `IValidator<T>` đăng ký |
| D4 | Hành vi thiếu validator | **Pass-through** (không bắt buộc mọi command có validator) — Host thêm validator khi cần |
| D5 | Hành vi fail | Map `ValidationResult` → exception Jarvis (vd. `BadRequestException` hoặc type/`BaseResponse` detail — chốt §7) với **Field** = property name, **Codes** = error code / message |
| D6 | DI Host | `services.AddValidatorsFromAssembly(typeof(…ApplicationMarker).Assembly)` (hoặc tương đương); gọi trong Application Layer Extension |
| D7 | Sync / async | Ưu tiên `ValidateAsync` trên đường async dispatcher / AppService; sync dispatcher gọi sync `Validate` hoặc `ValidateAsync().GetAwaiter()` — chốt §7 |
| D8 | CRUD | Khi implement [CrudAppService](./2026-08-07-adr-jarvis-crud-app-service.md): `CreateAsync` / `UpdateAsync` gọi `IValidator<TCreateInput>` / `IValidator<TUpdateInput>` nếu có |
| D9 | Domain | Unique / ACL / rule gắn aggregate → Domain method + `BusinessException`; **có thể** gọi từ validator async (inject repo) nhưng **không** bắt Domain phụ thuộc FV |
| D10 | MVC DataAnnotations | Giữ `AddBadRequestHandler` cho binding/ModelState; **không** xóa; FV là lớp Application bổ sung |
| D11 | Skill / docs | Cập nhật `application-dotnet` (+ Sample validator) khi implement |
| D12 | Package riêng | **Không** tạo `Jarvis.FluentValidation` ở MVP (O1); revisit O2 nếu Application phình hoặc team muốn opt-out cứng |

### 5.1 Sơ đồ

```text
  Host Controller
        │
        ▼
  CommandDispatcher / QueryDispatcher / CrudAppService
        │
        │  IValidator<T>?  ──► FluentValidation
        │         │ fail
        │         ▼
        │  BadRequest (BaseResponse field details)
        │ ok
        ▼
  Handler / use case  ──► Domain (invariants, không FV)
```

### 5.2 Sketch API (đích)

```csharp
// Host Application
public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxCode).NotEmpty();
    }
}

// Jarvis — trong dispatcher (ý tưởng)
var validator = _serviceProvider.GetService<IValidator<TCommand>>();
if (validator is not null)
{
    var result = await validator.ValidateAsync(command, cancellationToken);
    if (!result.IsValid)
        throw MapToBadRequest(result); // → BaseResponseErrorDetail
}
```

### 5.3 Khi nào **không** dùng FluentValidation

| Tình huống | Dùng thay |
|------------|-----------|
| Invariant aggregate (state machine, money consistency) | Method trên entity / domain service + `BusinessException` |
| Chỉ binding JSON / required field MVC | ModelState + `AddBadRequestHandler` (vẫn được) |
| Authorization | `[Authorize]` / policy — không nhét vào validator |

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Rule typed, testable; tái dùng ngoài controller; chuẩn hóa 400 |
| Tốt | CQRS + CRUD dùng một pipeline validate |
| Xấu / chi phí | `Jarvis.DDD.Application` phụ thuộc FluentValidation; Host phải đăng ký assembly validators |
| Xấu / chi phí | Cần map rõ error code vs message (localization Later) |
| Trung lập | DataAnnotations / ModelState vẫn chạy song song cho API binding |
| Trung lập | Module Setting / Tenants facade có thể dùng FV cho input riêng — không bắt buộc |

---

## 7. Confirm *(bắt buộc trước khi Accepted)*

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D12, C1–C7? | _chờ_ |
| Q2 | Chọn phương án? | **O1** (khuyến nghị) / O2 / O3 / O4 / O5 |
| Q3 | Fail → `BadRequestException` hay object `BaseResponse` trả thẳng (không throw)? | Throw `BadRequestException` (+ details) — OK? |
| Q4 | Version FluentValidation? | **12.x** — OK? |
| Q5 | Query cũng validate khi có `IValidator<TQuery>`? | Có (cùng D3/D4) — OK? |
| Q6 | Tạo `Jarvis.FluentValidation` ngay? | **Không** (D12) — OK? |
| Q7 | Ưu tiên làm trước / cùng CRUD AppService? | _chờ_ |

Sau khi chốt: cập nhật **Trạng thái** → 🟡 Accepted, ghi ngày Accept.

---

## 8. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Status → Accepted | — |
| 1 | Thêm package FV vào `Jarvis.DDD.Application`; helper `ValidateOrThrowAsync` + map lỗi | Build | 0 |
| 2 | Gắn dispatcher (async trước; sync nếu cần) | Test xanh | 1 |
| 3 | Sample: 1 command + `AbstractValidator` + assert 400 shape | Sample OK | 2 |
| 4 | Hook CRUD khi [CRUD ADR](./2026-08-07-adr-jarvis-crud-app-service.md) implement | Create/Update validate | CRUD Phase 2+ |
| 5 | Skill `application-dotnet` + README | Docs đồng bộ | 3 |

---

## 9. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Confirm §7 → Accepted | 🔴 |
| 2 | Helper validate + map `BaseResponse` / `BadRequestException` | 🔴 |
| 3 | Dispatcher (async) gọi validator khi có | 🔴 |
| 4 | Sample validator + test 400 | 🔴 |
| 5 | CRUD hooks (khi CRUD sẵn) | 🔴 |
| 6 | Skill / docs Application | 🔴 |
| 7 | ADR + [README index](./README.md) cập nhật | 🟡 (Proposed) |

---

## 10. Tham chiếu thêm

- [FluentValidation docs](https://docs.fluentvalidation.net/en/latest/) — start, DI, async, ASP.NET Core  
- [Installation](https://docs.fluentvalidation.net/en/latest/installation.html) · [Dependency Injection](https://docs.fluentvalidation.net/en/latest/di.html) · [ASP.NET Core](https://docs.fluentvalidation.net/en/latest/aspnet.html)  
- [Creating your first validator](https://docs.fluentvalidation.net/en/latest/start.html)  
- Jarvis: `AddBadRequestHandler`, `BadRequestException`, `BaseResponseErrorDetail`  
- Không supersede: Domain exception model; [CRUD AppService](./2026-08-07-adr-jarvis-crud-app-service.md)
