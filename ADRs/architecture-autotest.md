# Tài liệu Kiến trúc Phần mềm (SAD) — Autotest

**Ánh xạ Jarvis (monorepo này):**

| SAD | Jarvis |
|-----|--------|
| Package `@core/automation` / `@minvoice/automation-core` | `@jarvis/autotest` — [`autotest/core/`](../autotest/core/) |
| Playwright driver trong Core `src/drivers/` | Satellite `@jarvis/autotest.playwright` — [`autotest/playwright/`](../autotest/playwright/) |
| Project consumer (SAD §17) | [`autotest/sample/`](../autotest/sample/) chống Host [`Sample/`](../Sample/) |
| Keyword `automation` | Dành cho Agent/AI — không đặt `@jarvis/automation*` |

Ranh giới folder: [architecture-software.md](./architecture-software.md) §0.2. Hub chạy: [autotest/README.md](../autotest/README.md).

---

# Playwright Automation Test Framework

**Version:** 0.2  
**Status:** Architecture baseline — Accepted (một số thành phần đang ở trạng thái target/pending)  
**Date:** 2026-08-17  
**Language:** TypeScript  
**Automation Engine:** Playwright  
**Test Scope:** UI + API (project có thể triển khai một phần)  
**Distribution:** Git repository + NPM package

---

## 1. Mục đích

Tài liệu này định nghĩa kiến trúc mục tiêu cho một **Automation Test Framework** có khả năng tái sử dụng, được xây dựng bằng **TypeScript** và **Playwright**.

Framework được thiết kế để:

- Hỗ trợ cả **UI automation** và **API automation**.
- Có thể tái sử dụng cho nhiều application/project.
- Tách biệt việc phát triển **framework** với việc phát triển **test automation** của từng project.
- Kiểm soát quyền truy cập và thay đổi **Core Framework**.
- Cho phép các Senior-level engineer/tester có năng lực tốt về programming và architectural thinking tiếp cận và thay đổi Core.
- Cho phép developer/tester của project sử dụng framework mà không cần sửa implementation bên trong Core.
- Hỗ trợ đồng thời hai hình thức sử dụng: **Git-based consumption** trong quá trình phát triển framework và **NPM-based consumption** đối với các version đã release.
- Áp dụng các nguyên tắc của **Clean Architecture** ở những nơi chúng mang lại giá trị thực tế, nhưng tránh over-engineering.
- Cho phép mỗi project triển khai một phần phạm vi: **chỉ API automation**, **chỉ UI automation**, hoặc cả hai. Core thiết kế để hỗ trợ cả hai nhưng không ép project triển khai layer không cần thiết.

---

# 2. Mục tiêu

## 2.1 Mục tiêu chính

1. Xây dựng một **company-level automation core** dùng chung.
2. Cho phép nhiều project sử dụng cùng một Core.
3. Hỗ trợ UI và API automation thông qua Playwright.
4. Làm cho test case thể hiện **business intent** thay vì technical implementation.
5. Cô lập các chi tiết implementation đặc thù của Playwright bên trong framework/adapters.
6. Giữ business knowledge của từng project nằm ngoài Core.
7. Xây dựng một **stable public API** giữa Core và các project sử dụng framework.
8. Hỗ trợ versioned release.
9. Đưa parallel execution, diagnostics, logging, reporting và test lifecycle thành các framework concern.
10. Kiểm soát thay đổi Core thông qua Git permissions và review.

## 2.2 Không nằm trong phạm vi

Core Framework không nên:

- Chứa business logic thuộc về một application cụ thể.
- Chứa project-specific workflows.
- Chứa project-specific pages.
- Chứa project-specific API endpoints.
- Trở thành một wrapper cho mọi Playwright API.
- Cố gắng abstraction hóa mọi UI element thành generic interface.
- Ép tất cả project sử dụng cùng một business model.
- Trở thành một general-purpose utility library chứa các helper không liên quan.

---

# 3. Các nguyên tắc kiến trúc

## 3.1 Automation Framework là một software product

Framework phải được xem như một **platform/library**, không đơn thuần là một tập hợp test script.

Do đó cần có:

- Versioning
- Public API boundaries
- Backward compatibility considerations
- Code review
- CI validation
- Documentation
- Release management
- Ownership
- Access control

---

## 3.2 Business intent quan trọng hơn technical implementation

Test nên thể hiện hệ thống cần làm gì, thay vì Playwright thực hiện thao tác đó như thế nào.

### Không nên

```javascript
await page.locator('#email').fill(email);
await page.locator('#password').fill(password);
await page.getByRole('button', { name: 'Login' }).click();
```

### Nên

```javascript
await authenticationWorkflow.loginAs(user);
```

Các layer phía dưới chịu trách nhiệm quyết định action này được thực hiện thông qua UI hay API.

---

## 3.3 Dependency Inversion

Các khái niệm ở high-level như test/business không nên phụ thuộc trực tiếp vào implementation detail của Playwright.

Về mặt khái niệm:

```text
Test
  ↓
Workflow
  ↓
UI Adapter / API Adapter
  ↓
Playwright
```

Không được có dependency ngược:

```text
Core
  X→ Project Workflow
  X→ Project Page
  X→ Project API Client
```

---

## 3.4 Project-specific knowledge nằm ngoài Core

Core phải giữ được tính generic.

Ví dụ Core không được biết về:

```text
CreateVietnamAirlinesInvoice
ApproveBelleClinicCustomer
CreateAeskulapAppointment
```

Đây là các application/project concepts.

Core chỉ nên cung cấp các capability tổng quát như:

```text
Browser lifecycle
API request handling
Authentication infrastructure
Configuration
Logging
Reporting
Retry
Test context
Fixtures
Workflow abstraction
```

---

## 3.5 Ưu tiên abstraction có giá trị thay vì abstraction tối đa

Clean Architecture không có nghĩa là phải wrapper mọi Playwright method.

### Các abstraction có giá trị

- Browser lifecycle
- Test context
- API client infrastructure
- Authentication
- Configuration
- Logging
- Reporting
- Retry
- Workflow contract
- Fixtures

### Các abstraction thường không cần thiết

- GenericButton
- GenericInput
- GenericLocator
- GenericPageManager
- Một wrapper riêng cho từng Playwright method

Playwright đã cung cấp các primitive khá tốt. Framework chỉ nên bổ sung abstraction ở nơi nó mang lại architectural value.

---

# 4. Các phương án kiến trúc đã xem xét

## 4.1 Phương án A — Direct Page Object / API Client

```text
Test
 ├── Page Object
 └── API Client
```

### Ưu điểm

- Đơn giản.
- Dễ hiểu.
- Chi phí implementation ban đầu thấp.
- Phù hợp với test suite nhỏ.

### Nhược điểm

- Business workflow bị phân tán trong các test.
- Test bị coupling mạnh với UI/API implementation.
- Khó chia sẻ business flow một cách nhất quán.
- Project lớn dễ phát sinh duplicated orchestration logic.

### Quyết định

**Không chọn làm kiến trúc chính.**

Phương án này vẫn có thể được sử dụng bên trong UI/API layer, nhưng không phải application-facing architecture chính.

---

## 4.2 Phương án B — Test → Workflow → UI/API Adapter

```text
Test
  ↓
Workflow
  ├── UI Adapter
  └── API Adapter
```

### Ưu điểm

- Tách rõ business intent và technical implementation.
- UI và API có thể cùng tồn tại một cách tự nhiên.
- Cân bằng tốt giữa simplicity và architectural control.
- Phù hợp với nhiều project.
- Dễ reuse business workflow.
- Dễ sử dụng API để setup data và UI để verify.
- Không yêu cầu conceptual overhead cao như full Screenplay Pattern.

### Nhược điểm

- Cần kỷ luật về architectural boundaries.
- Workflow được thiết kế kém có thể trở thành class quá lớn.
- Cần phân biệt rõ Core abstraction và Project implementation.

### Quyết định

**Được lựa chọn.**

Đây là architecture chính của framework.

---

## 4.3 Phương án C — Screenplay Pattern

```text
Actor
  ↓
Tasks
  ↓
Interactions
  ↓
UI/API
```

### Ưu điểm

- Business-oriented modeling mạnh.
- Rất expressive đối với domain phức tạp.
- Hỗ trợ reusable tasks và abilities.
- Phù hợp với automation system rất lớn.

### Nhược điểm

- Conceptual complexity cao hơn.
- Cần đào tạo nhiều abstraction hơn.
- Có thể trở thành over-engineering đối với các CRUD/business workflow thông thường.
- Chi phí onboarding cao hơn.

### Quyết định

**Không chọn làm architecture chính.**

Có thể áp dụng một số ý tưởng của Screenplay Pattern sau này nếu framework phát triển đến mức mà chúng tạo ra giá trị đo được.

---

# 5. Kiến trúc được lựa chọn

Kiến trúc được lựa chọn:

```text
                    ┌──────────────────────────┐
                    │      PROJECT TESTS       │
                    │                          │
                    │ Smoke                    │
                    │ Functional               │
                    │ Regression               │
                    │ Integration              │
                    └────────────┬─────────────┘
                                 │
                                 ▼
                    ┌──────────────────────────┐
                    │        WORKFLOW          │
                    │      Project-owned       │
                    │                          │
                    │ Login                    │
                    │ CreateOrder              │
                    │ ApproveOrder              │
                    │ CreateCustomer            │
                    └────────────┬─────────────┘
                                 │
                       ┌─────────┴─────────┐
                       ▼                   ▼
              ┌────────────────┐   ┌────────────────┐
              │   UI ADAPTER   │   │   API ADAPTER  │
              │ Project-owned  │   │ Project-owned  │
              │ Pages          │   │ API Clients    │
              │ Components     │   │                │
              └───────┬────────┘   └───────┬────────┘
                      │                    │
                      └─────────┬──────────┘
                                ▼
                    ┌──────────────────────────┐
                    │      AUTOMATION CORE     │
                    │                          │
                    │ Workflow abstraction     │
                    │ Test Context             │
                    │ UI infrastructure        │
                    │ API infrastructure       │
                    │ Fixtures                 │
                    │ Authentication           │
                    │ Configuration            │
                    │ Logging                  │
                    │ Reporting                │
                    │ Retry                    │
                    └────────────┬─────────────┘
                                 │
                                 ▼
                         ┌───────────────┐
                         │   Playwright  │
                         │ Browser/API   │
                         └───────────────┘
```

---

# 6. Ranh giới giữa Core và Project

Đây là architectural boundary quan trọng nhất giữa company Core và từng application project.

## 6.1 Trách nhiệm của Core

Core sở hữu:

- Test runtime.
- Playwright lifecycle.
- Browser/context management.
- API request infrastructure.
- UI adapter infrastructure.
- API adapter infrastructure.
- Workflow abstraction.
- Test context.
- Fixtures.
- Authentication infrastructure.
- Configuration infrastructure.
- Logging.
- Reporting.
- Retry mechanisms.
- Common test lifecycle.
- Common test data infrastructure ở mức generic.
- Framework-level utilities.

## 6.2 Trách nhiệm của Project

Mỗi project sử dụng Core sở hữu:

- Test cases.
- Business workflows.
- Project pages.
- Project UI components.
- Project API clients.
- Project-specific test data.
- Project-specific assertions.
- Project-specific configuration.
- Application-specific authentication behavior khi cần.

---

# 7. Workflow Architecture

## 7.1 Quyết định

**Workflow abstraction nằm trong Core.**

**Workflow implementation nằm trong consuming application/project.**

Quan hệ:

```text
Core
 └── Workflow abstraction

Project
 ├── LoginWorkflow
 ├── CreateCustomerWorkflow
 ├── CreateOrderWorkflow
 └── ApproveOrderWorkflow
```

## 7.2 Lý do

Nếu Workflow chỉ tồn tại trong Project:

- Không có common lifecycle/contract.
- Các project có thể triển khai theo các pattern hoàn toàn khác nhau.
- Khó standardize các capability ở framework level.

Nếu business Workflow implementation nằm trong Core:

- Core bị coupling với business domain.
- Giảm khả năng reuse.
- Framework trở thành application-specific framework.

Vì vậy:

> **Core định nghĩa abstraction; Project implement business behavior.**

---

# 8. UI Architecture

UI layer chịu trách nhiệm chuyển business action của project thành Playwright UI interactions.

Cấu trúc khuyến nghị:

```text
Project
└── ui/
    ├── pages/
    │   ├── LoginPage.js
    │   ├── CustomerPage.js
    │   └── OrderPage.js
    │
    └── components/
        ├── Header.js
        ├── DataTable.js
        └── Dialog.js
```

## 8.1 Trách nhiệm của Page Object

Page Object nên đại diện cho UI interaction và UI state.

Không nên chứa các business workflow lớn.

Nên:

```javascript
await orderPage.selectCustomer(customer);
await orderPage.addProduct(product);
await orderPage.submit();
```

Không nên:

```javascript
await orderPage.createCustomerAndOrderAndPay();
```

Phần thứ hai thuộc về Workflow.

---

# 9. API Architecture

API layer nên cô lập application-specific endpoints khỏi test.

Khuyến nghị:

```text
Project
└── api/
    ├── CustomerApi.js
    ├── OrderApi.js
    └── PaymentApi.js
```

Ví dụ conceptual usage:

```javascript
const customer = await customerApi.create(customerData);
const order = await orderApi.create(orderData);
```

Test không nên lặp lại việc:

```javascript
request.post('/api/v1/orders', {
    headers: ...
});
```

API Client chịu trách nhiệm về endpoint-specific knowledge.

Core chịu trách nhiệm về generic HTTP/request infrastructure.

---

# 10. UI và API là các Parallel Adapter

UI và API nên được xem là hai mechanism để tương tác với cùng một application.

```text
                 Workflow
                /        \
               /          \
             UI            API
             │              │
        Page Objects     API Clients
             │              │
             └──────┬───────┘
                    │
                Playwright
```

Điều này cho phép các scenario như:

```text
API → Create test data
       ↓
UI → Perform business action
       ↓
API → Verify result
```

Cách này thường phù hợp hơn việc bắt mọi setup phải thực hiện thông qua UI.

---

# 11. Test Context

Core nên cung cấp một **Test Context**.

Conceptually:

```javascript
context = {
    browser,
    page,
    request,
    config,
    logger,
    testInfo,
    auth,
    data
};
```

Test Context là một controlled dependency mechanism.

Không nên biến nó thành một unrestricted global object chứa mọi application state.

Chỉ nên thêm dependency khi dependency đó có mục đích rõ ràng ở framework level.

> Implementation tham chiếu hiện tại tối giản chỉ gồm `{ runId, env }`; các field còn lại chỉ được bổ sung khi có framework concern thực tế.

---

# 12. Public API Boundary

Core cần expose một **controlled public API**.

Conceptually:

```text
automation-core/
├── src/
│   ├── core/
│   ├── ui/
│   ├── api/
│   ├── workflow/
│   ├── fixtures/
│   ├── logging/
│   └── internal/
│
└── index.ts
```

`index.ts` là public entry point.

Consumer sử dụng:

```javascript
import {
    TestApp,
    Workflow,
    UiAdapter,
    ApiAdapter
} from '@core/automation';
```

Consumer không nên phụ thuộc vào:

```javascript
@core/automation/internal/...
```

`internal` là implementation detail.

---

# 13. Distribution Architecture

## 13.1 Quyết định

Core phải hỗ trợ đồng thời:

1. Git repository consumption.
2. NPM package consumption.

Hai hình thức này là hai distribution channel của cùng một framework artifact.

```text
                    Framework Source
                          │
                    Build / Test
                          │
                    Versioned Release
                       /       \
                      /         \
                    Git         NPM
```

**Lưu ý adoption:** Project phải consume Core như một dependency qua Git/NPM. Không được tự định nghĩa lại (copy) các abstraction đã có trong Core (ví dụ transport port, `ApiClient`). Trong giai đoạn migration, nếu project tạm giữ bản sao của một contract Core, bản sao đó phải được loại bỏ khi project chính thức dùng Core — tránh tồn tại hai định nghĩa cho cùng một contract.

## 13.2 Git consumption

Git phù hợp cho:

- Framework development.
- Test các thay đổi chưa release.
- Test feature branch.
- Debugging.
- Temporary project-specific compatibility.

Ví dụ:

```json
{
  "dependencies": {
    "@core/automation":
      "git+ssh://git@github.com/core/automation.git"
  }
}
```

## 13.3 NPM consumption

NPM được ưu tiên cho việc sử dụng bình thường của project.

Ví dụ:

```json
{
  "dependencies": {
    "@core/automation": "^1.5.0"
  }
}
```

Registry thực tế có thể là:

- GitHub Packages.
- GitLab Package Registry.
- Azure Artifacts.
- Nexus.
- Một private NPM-compatible registry khác.

Registry cụ thể là infrastructure decision và không thuộc Core architecture.

---

# 14. Versioning

Sử dụng **Semantic Versioning**:

```text
MAJOR.MINOR.PATCH
```

Ví dụ:

```text
1.0.0
1.1.0
1.1.1
2.0.0
```

Quy ước:

| Thay đổi | Version |
|---|---|
| Backward-compatible bug fix | PATCH |
| Backward-compatible capability | MINOR |
| Breaking public API change | MAJOR |

Project phải có khả năng pin một framework version đã biết và chủ động quyết định thời điểm upgrade.

---

# 15. Core Access Control

Core repository không nên được đối xử như một project repository thông thường.

Governance khuyến nghị:

```text
                    Core Repository
                           │
                    Protected Branch
                           │
             ┌─────────────┴─────────────┐
             │                           │
       Framework Team               Other Team
             │                           │
       Can modify Core              Consume Core
             │
       Pull Request
             │
       Required Review
             │
       CI Validation
             │
          Release
```

Có thể sử dụng:

- Protected branches.
- CODEOWNERS.
- Mandatory Pull Request review.
- Required CI checks.
- Release tagging.
- Giới hạn quyền publish NPM package cho authorized maintainers.

Mục tiêu là biến việc thay đổi Core thành một engineering activity có kiểm soát.

---

# 16. Cấu trúc Core Repository khuyến nghị

Package: `@core/automation`.  
**Nguyên tắc:** Core **engine-agnostic** — không chứa business logic của application; Playwright nằm trong `drivers/` (package peer optional).

```text
automation-core/
│
├── src/
│   ├── core/                             # Runtime & lifecycle
│   │   ├── TestContext.ts
│   │   ├── Configuration.ts
│   │   └── createTestContext.ts
│   │
│   ├── ports/                            # Abstraction — không phụ thuộc engine
│   │   ├── IHttpTransport.ts
│   │   └── IBrowserDriver.ts
│   │
│   ├── workflow/
│   │   └── Workflow.ts                   # Contract Workflow<TResult>
│   │
│   ├── api/
│   │   ├── ApiAdapter.ts
│   │   ├── ApiClient.ts                  # HTTP base — dùng IHttpTransport
│   │   └── ApiClientError.ts
│   │
│   ├── ui/
│   │   ├── UiAdapter.ts
│   │   ├── Page.ts
│   │   ├── Component.ts
│   │   └── BrowserManager.ts
│   │
│   ├── composition/
│   │   └── createTestHarness.ts          # Bootstrap generic (không biết project)
│   │
│   ├── authentication/
│   │   ├── AuthStrategy.ts
│   │   └── ApiKeyAuth.ts
│   │
│   ├── logging/
│   │   ├── Logger.ts
│   │   └── ConsoleLogger.ts
│   │
│   ├── reporting/
│   │   ├── RunContext.ts
│   │   └── CsvExporter.ts
│   │
│   ├── retry/
│   │   ├── pollUntil.ts
│   │   └── RetryPolicy.ts
│   │
│   ├── test-data/
│   │   └── IdGenerator.ts
│   │
│   ├── drivers/                          # Engine-specific — optional peer package
│   │   └── playwright/
│   │       ├── PlaywrightTransport.ts
│   │       └── createPlaywrightFixture.ts
│   │
│   └── internal/                         # KHÔNG export — Project cấm import
│       └── parsers/
│
├── tests/
│   ├── unit/
│   └── integration/
│
├── index.ts                              # Public API whitelist
├── package.json
├── tsconfig.json
├── README.md
└── docs/
    └── public-api.md
```

### 16.1 Mô tả folder — Core

| Folder / file | Trách nhiệm | Được phép chứa | Không được chứa |
|---------------|-------------|----------------|-----------------|
| `src/core/` | Test runtime: context, config merge, factory lifecycle | `TestContext`, `Configuration`, DI container | Endpoint, selector, business DTO |
| `src/ports/` | **Port** trừu tượng hóa engine (HTTP, browser) | `IHttpTransport`, `IBrowserDriver` | Playwright import, URL cụ thể |
| `src/workflow/` | **Contract** cho project workflow | `Workflow<TResult>` interface, optional lifecycle hooks | `LoginWorkflow`, `sendOrder` |
| `src/api/` | HTTP infrastructure dùng chung | `ApiClient` base, parse/error, `ApiAdapter` contract | `/api/v1/orders`, credential, token value |
| `src/ui/` | UI infrastructure dùng chung | `Page`, `Component`, `BrowserManager` base | `LoginPage` của project, locator cụ thể |
| `src/composition/` | Factory bootstrap **generic** | `createTestHarness(options)` | `XxxTestApp`, wire client của project |
| `src/authentication/` | Cơ chế auth cross-cutting | `AuthStrategy`, `ApiKeyAuth`, token header helper | Credential, token/header value cụ thể |
| `src/logging/` | Log interface + impl mặc định | `Logger`, `ConsoleLogger` | Business log message |
| `src/reporting/` | Run context, export báo cáo | `runId`, report path, `results.csv` exporter | Mapping business test-case id của project |
| `src/retry/` | Poll/retry framework-level | `pollUntil`, `RetryPolicy` observable | `isOrderNotFound` (domain → Project) |
| `src/test-data/` | Helper data **generic** | `IdGenerator`, pattern unique id | Business DTO của project, payload nghiệp vụ (XML/JSON) |
| `src/drivers/` | **Driver** — implement port cho từng engine | `PlaywrightTransport`, fixture factory | Business workflow |
| `src/internal/` | Implementation detail | Parser, glue code | — |
| `tests/unit/` | Unit test Core — bắt buộc trước release | Test `ApiClient`, `pollUntil`, `createRunId` | E2E business |
| `tests/integration/` | Smoke integration Core + driver | Fixture factory smoke | Project test case |
| `index.ts` | **Single entry** — public API whitelist | Re-export modules được phép | Re-export `internal/` |

Tên folder cụ thể có thể thay đổi trong quá trình implementation; **dependency boundaries** và **ranh giới public/internal** quan trọng hơn tên folder.

### 16.2 Trạng thái triển khai tham chiếu (reference implementation)

Bảng dưới đây ghi nhận thành phần nào đã có trong **reference implementation** và thành phần nào đang là **target** (chỉ bổ sung khi có nhu cầu thực tế). Đây là trạng thái tại thời điểm viết tài liệu, không phải ràng buộc kiến trúc.

| Thành phần | Trạng thái |
|---|---|
| `src/core/` (`TestContext`, `Configuration`, `createTestContext`) | Đã implement |
| `src/ports/IHttpTransport` | Đã implement |
| `src/ports/IBrowserDriver` | Target — bổ sung khi có UI requirement |
| `src/workflow/Workflow` | Đã implement |
| `src/api/ApiClient` (+ `ApiClientError`) | Đã implement |
| `src/api/ApiAdapter` | Target |
| `src/ui/` (`UiAdapter`, `Page`, `Component`, `BrowserManager`) | Target — cần khi project có UI test |
| `src/composition/createTestHarness` | Đã implement |
| `src/authentication/` (`AuthStrategy`, `ApiKeyAuth`) | Đã implement |
| `src/logging/` (`Logger`, `ConsoleLogger`) | Đã implement |
| `src/reporting/RunContext` | Đã implement |
| `src/reporting/CsvExporter` | Target — export tùy biến có thể đặt ở project `scripts/` |
| `src/retry/pollUntil` | Đã implement |
| `src/retry/RetryPolicy` | Target |
| `src/test-data/IdGenerator` | Target — project có thể tạm dùng helper riêng |
| `src/drivers/playwright/` | Target — engine integration thường do project implement |
| `src/internal/` | Target |
| `index.ts` (public API whitelist) | Đã implement |
| `tests/unit/` | Đã implement |
| `tests/integration/` | Target |
| Build (`dist/`) + NPM publish | Target |

---

# 17. Cấu trúc Project Repository khuyến nghị

Ví dụ cho một application ERP (và mọi project consumer Core).  
**Nguyên tắc:** Project **engine-agnostic** ở `application/`, `integrations/`, `composition/`; runner (Playwright) chỉ trong `drivers/<engine>/`.

```text
erp-automation/
│
├── tests/                                # Spec — verify behavior
│   ├── smoke/
│   ├── functional/
│   ├── regression/
│   └── integration/
│
├── application/                          # Use cases — business orchestration
│   ├── authentication/
│   │   ├── login.workflow.ts
│   │   ├── data.ts
│   │   └── verifier.ts
│   ├── customer/
│   │   ├── create-customer.workflow.ts
│   │   └── update-customer.workflow.ts
│   ├── order/
│   │   ├── create-order.workflow.ts
│   │   └── cancel-order.workflow.ts
│   ├── shared/
│   │   └── verifiers/
│   └── providers/                        # Test doubles (mock DB, message bus)
│
├── integrations/                         # Outbound adapters — gọi ra app/API ngoài
│   ├── contracts/
│   │   └── models.ts                     # DTO request/response
│   └── erp/
│       ├── customer-api.client.ts
│       ├── order-api.client.ts
│       └── payment-api.client.ts
│
├── composition/                          # Composition Root — wire dependency
│   ├── bootstrap.ts                      # factory clients từ transport + config
│   ├── client-registry.ts                # gom integrations clients (scoped)
│   └── erp-app.ts                        # Facade — entry cho spec (app.*)
│
├── ui/                                   # UI adapter (khi có UI test)
│   ├── pages/
│   │   ├── LoginPage.ts
│   │   ├── CustomerPage.ts
│   │   └── OrderPage.ts
│   └── components/
│       ├── Header.ts
│       └── DataTable.ts
│
├── drivers/
│   └── playwright/                       # CHỈ Playwright — không leak ra ngoài
│       ├── playwright.config.ts
│       ├── global-setup.ts
│       ├── playwright-transport.ts
│       └── fixtures/
│           └── api-test.ts               # test.extend({ app, ... })
│
├── data/
│   ├── scenarios/                        # Data matrix (happy/unhappy)
│   ├── factories/
│   ├── builders/
│   └── payloads/                         # file JSON/XML tĩnh
│
├── config/
│   └── env.ts                    # Đọc cấu hình từ .env gốc của project
│
├── .env.example                 # Template cấu hình — copy sang .env (không commit .env)
│
├── helpers/                              # Primitive thuần (không env, không HTTP)
│
├── scripts/                              # Post-run: CSV, retention, …
│
└── package.json                          # depend: @core/automation
```

> **Alias tên cũ (tham khảo):** `workflows/` ≈ `application/` · `api/` ≈ `integrations/` · `fixtures/` (composition) ≈ `composition/` + `drivers/*/fixtures/`.

### 17.1 Mô tả folder — Project

| Folder | Trách nhiệm | Vai trò (C# tương đương) | Import Playwright? |
|--------|-------------|--------------------------|--------------------|
| `tests/` | Mô tả **behavior cần verify**; gọi `app.*` hoặc facade | `*.IntegrationTests` | Chỉ qua fixture export |
| `tests/smoke/` | Happy path P0 — chạy nhanh | Smoke test suite | Không trực tiếp |
| `tests/functional/` | Happy + unhappy theo scenario matrix | Functional test | Không trực tiếp |
| `tests/regression/` | Negative, edge, full pipeline | Regression test | Không trực tiếp |
| `tests/integration/` | E2E cross-system (API + mock + DB) | Integration test | Không trực tiếp |
| `application/` | **Use case** — orchestration nghiệp vụ | `Application.Services` | **Cấm** |
| `application/<domain>/` | Workflow theo bounded context (invoice, order, …) | Handler / service theo module | **Cấm** |
| `application/*/workflow*.ts` | Entry use case: `sendXxx`, `waitXxx` — không `expect` | Application method | **Cấm** |
| `application/*/data.ts` | Loader + builder payload nghiệp vụ (XML, JSON, body) | Factory/builder nghiệp vụ | **Cấm** |
| `application/*/verifier.ts` | Assert business (trace, response code) | Assertion helper | Chỉ `expect` assert lib |
| `application/shared/` | Helper dùng chung nhiều domain | Shared kernel test | **Cấm** |
| `application/providers/` | Mock/stub hạ tầng (Kafka, Mongo, callback) | Test double | **Cấm** |
| `integrations/` | **Outbound HTTP** — map endpoint | `Infrastructure.HttpClients` | **Cấm** |
| `integrations/contracts/` | DTO request/response — không logic | `Application.Contracts` | **Cấm** |
| `integrations/<app>/` | Typed client per external API (`*ApiClient`) | `I*ApiClient` impl | **Cấm** |
| `composition/` | **Composition Root** — wire DI, không business rule | `Program.cs` / `ConfigureServices` | **Cấm** |
| `composition/bootstrap.ts` | Tạo clients từ `IHttpTransport` + `config` | Service registration | **Cấm** |
| `composition/client-registry.ts` | Gom client đã wire (scoped per test) | `IServiceProvider` | **Cấm** |
| `composition/*-app.ts` | Facade mỏng cho spec (`app.order.create()`) | `I*AppService` | **Cấm** |
| `ui/pages/` | Page Object — UI interaction, locator | Page Object | Extend Core `Page` |
| `ui/components/` | UI component tái sử dụng | Reusable UI partial | Extend Core `Component` |
| `drivers/playwright/` | Runner config, transport, fixture lifecycle | `WebApplicationFactory` | **Được** |
| `drivers/playwright/fixtures/` | `test.extend()` — inject `app`, `testId`, … | Test host DI hook | **Được** |
| `data/scenarios/` | Parameterized matrix — tag, id, kind | Test data provider | Không |
| `data/factories/` · `builders/` | Tạo object test có variation | Test data factory | Không |
| `data/payloads/` | File tĩnh (XML, JSON) | Test fixture files | Không |
| `config/` | Env, profile, secret mapping (không hardcode) | `appsettings`, `IConfiguration` | Không |
| `helpers/` | Primitive thuần (`loadXml`, `encode`) — không biết env | Utility thuần | Không |
| `scripts/` | Post-processing (CSV export, cleanup reports) | Build/ops script | Không |

> **Cách tổ chức test:** Cả hai cách đều hợp lệ — (1) tách folder theo loại test như bảng trên (`tests/smoke/`, `tests/functional/`, …); hoặc (2) tách theo layer (`tests/api/`, `tests/ui/`) và phân loại bằng tag `@smoke` / `@functional` / `@regression`. Project chọn một cách và giữ nhất quán.

### 17.2 Quy tắc dependency — Project

```text
tests/
  ↓
composition/*-app.ts          (facade)
  ↓
application/*/workflow.ts     (use case)
  ↓
integrations/<app>/*.client.ts
  ↓
@core/automation      (ApiClient + IHttpTransport)
  ↓
drivers/playwright/           (PlaywrightTransport)
  ↓
Playwright
```

---

# 18. Dependency Rules

Dependency direction baseline:

```text
Test
 ↓
Project Workflow
 ↓
Project UI / Project API
 ↓
Core infrastructure
 ↓
Playwright
```

Core không được dependency vào Project implementation.

Các ví dụ bị cấm:

```text
Core → Project Workflow       FORBIDDEN
Core → Project Page           FORBIDDEN
Core → Project API Client     FORBIDDEN
Core → Project Business Model FORBIDDEN
```

Được phép:

```text
Project → Core
Core → Playwright
Project → Playwright (chỉ khi có lý do rõ ràng)
```

Dependency cuối cùng nên được hạn chế tối đa. Project code thông thường nên sử dụng abstraction do Core cung cấp.

---

# 19. Test Design Philosophy

Một test chủ yếu nên trả lời:

> Behavior nào đang được verify?

Ví dụ:

```javascript
test('customer can create an order', async ({ app }) => {
    const customer = await app.customer.createValid();

    await app.authentication.loginAs(customer);

    await app.order.create({
        customer,
        product: 'PRODUCT-001'
    });

    await app.order.shouldBeCreated();
});
```

API chính xác ở ví dụ trên là implementation decision và sẽ được refine trong implementation phase.

Mục tiêu kiến trúc là test không cần biết:

- CSS selectors.
- Browser initialization.
- HTTP headers.
- Token handling.
- Retry implementation.
- Screenshot logic.
- Trace configuration.
- Low-level request construction.

---

# 20. Test Data

Test data nên được tách khỏi test logic.

Các pattern có thể sử dụng:

```text
Factory
Builder
Fixture
Data Provider
```

Ví dụ:

```javascript
const customer = CustomerFactory.createValid();
```

thay vì liên tục tạo các complex object trực tiếp trong test.

Project-specific data thuộc về Project.

Generic test-data infrastructure có thể thuộc về Core.

---

# 21. Assertions

Khi phù hợp, assertions nên gần với business intent.

Low-level:

```javascript
expect(response.status()).toBe(201);
expect(responseBody.status).toBe('Created');
```

Higher-level project assertion:

```javascript
order.shouldBeCreated();
```

Core có thể cung cấp assertion infrastructure, nhưng business assertion cụ thể của application nên nằm trong consuming project.

---

# 22. Test Isolation và Parallel Execution

Architecture phải giả định rằng test có thể chạy parallel.

Test nên tránh uncontrolled shared mutable state.

Ưu tiên:

```text
Test A → Test Data A
Test B → Test Data B
```

thay vì:

```text
Test A ─┐
        ├→ Shared mutable test data
Test B ─┘
```

Test isolation phải được xem xét ngay từ đầu thay vì chỉ bổ sung sau khi parallel execution trở thành requirement.

---

# 23. Observability và Diagnostics

Khi test fail, framework cần cung cấp đủ thông tin để điều tra.

Framework nên được thiết kế để hỗ trợ:

```text
Test failure
    │
    ├── Screenshot
    ├── Trace
    ├── Video khi phù hợp
    ├── Console logs
    ├── Network/request information
    ├── API request/response information
    ├── Test metadata
    └── Execution duration
```

Reporting stack cụ thể có thể được lựa chọn trong implementation phase.

Quyết định kiến trúc quan trọng là diagnostics thuộc trách nhiệm của Core, không nên được duplicate trong từng Project.

---

# 24. Configuration

Configuration nên được centralized ở framework level nhưng vẫn cho phép project-specific values.

Conceptually:

```text
Core defaults
      +
Environment configuration
      +
Project configuration
      +
Runtime/test overrides
```

Ví dụ:

```text
BASE_URL
API_URL
BROWSER
HEADLESS
TIMEOUT
RETRY
ENVIRONMENT
AUTHENTICATION
```

Secrets không được hard-code trong test hoặc Core source repository.

---

# 25. Authentication

Authentication là một cross-cutting framework concern.

Core nên cung cấp generic mechanisms cho:

- Authentication lifecycle.
- Token/session handling.
- Browser authentication state.
- API authentication.
- Credential/configuration integration.

Project code cung cấp application-specific authentication details khi cần.

Architecture nên cho phép:

```text
API authentication
       +
UI authentication
       +
Shared authentication state
```

mà không ép tất cả project phải sử dụng một authentication mechanism duy nhất.

---

# 26. Error Handling và Retry

Retry nên là framework-level capability thay vì để từng test tự implement.

Tuy nhiên retry không được che giấu defect thật.

Framework nên phân biệt:

```text
Infrastructure/transient failure
```

và:

```text
Actual product/test failure
```

Retry policy phải explicit và observable.

Nếu test vẫn fail sau retry, framework phải cung cấp evidence từ các lần fail.

---

# 27. Clean Architecture Principles

Framework áp dụng Clean Architecture theo hướng thực dụng. Mục tiêu không phải sao chép nguyên xi cấu trúc của một backend application, mà áp dụng các nguyên tắc giúp automation code có dependency rõ ràng, dễ thay đổi và dễ bảo trì.

## 27.1 Dependency Rule

Dependency phải hướng từ layer có tính application/business cao hơn về abstraction ổn định hơn.

Baseline:

```text
Test
 ↓
Workflow
 ↓
Project UI / Project API
 ↓
Core Infrastructure
 ↓
Playwright
```

Core tuyệt đối không phụ thuộc vào Project.

```text
Core → Project        FORBIDDEN
Project → Core        ALLOWED
```

## 27.2 Separation of Concerns

Mỗi layer phải có một responsibility rõ ràng:

```text
Test
  = Behavior verification

Workflow
  = Business operation orchestration

UI
  = UI interaction

API
  = API interaction

Core
  = Automation infrastructure

Playwright
  = Automation engine
```

Không được sử dụng một layer để thay thế responsibility của layer khác.

## 27.3 Dependency Inversion

High-level logic không nên bị coupling trực tiếp với low-level implementation nếu abstraction mang lại giá trị.

Ví dụ Workflow không nên tự quản lý:

```text
Browser launch
Page creation
HTTP authentication
Request lifecycle
Screenshot
Retry
```

Những concern này thuộc Core.

## 27.4 Single Responsibility Principle

Một class/module chỉ nên có một primary responsibility và một lý do chính để thay đổi.

Ví dụ:

```text
LoginPage
  → UI login interaction

LoginApi
  → Login API interaction

LoginWorkflow
  → Login business flow
```

Không nên gom cả ba vào một class.

## 27.5 Open/Closed Principle

Core nên được thiết kế để có thể mở rộng behavior mà hạn chế sửa code ổn định.

Ví dụ:

```text
AuthenticationStrategy
├── ApiAuthentication
├── UiAuthentication
└── TokenAuthentication
```

Thêm một strategy mới không nên yêu cầu sửa toàn bộ framework lifecycle.

## 27.6 Liskov Substitution Principle

Khi Core cung cấp abstraction/contract, implementation phải tuân thủ behavior contract của abstraction.

Ví dụ:

```text
ApiAdapter
├── ProjectApiAdapterA
└── ProjectApiAdapterB
```

Không được có implementation phá vỡ assumptions của consumer.

## 27.7 Interface Segregation

Không tạo abstraction quá lớn.

Không nên:

```text
IAutomationManager
  + browser()
  + page()
  + api()
  + database()
  + email()
  + kafka()
  + screenshot()
  + report()
  + retry()
  + ...
```

Ưu tiên các abstraction nhỏ, có responsibility rõ.

## 27.8 Composition over Inheritance

Ưu tiên:

```text
Workflow
 ├── AuthenticationService
 ├── CustomerApi
 ├── CustomerPage
 └── Logger
```

thay vì:

```text
BaseWorkflow
   ↓
BaseAuthenticatedWorkflow
   ↓
BaseCustomerWorkflow
   ↓
CreateCustomerWorkflow
```

Inheritance chỉ được sử dụng khi có một quan hệ "is-a" thực sự và behavior contract rõ ràng.

---

# 28. 10 Architecture Philosophies

Đây là các triết lý nền tảng mà mọi người phát triển Core và Project phải hiểu trước khi mở rộng framework.

## Philosophy 1 — Business Intent over Technical Implementation

### Nguyên tắc

Test phải thể hiện:

> Hệ thống cần làm gì?

thay vì:

> Playwright phải làm những thao tác gì?

### Ví dụ không tốt

```javascript
await page.locator('#email').fill(email);
await page.locator('#password').fill(password);
await page.getByRole('button', { name: 'Login' }).click();
```

### Ví dụ tốt

```javascript
await authenticationWorkflow.loginAs(user);
```

### Lợi ích

Nếu UI thay đổi:

```text
CSS selector
DOM
UI framework
Page layout
```

business test không nhất thiết phải thay đổi theo.

---

## Philosophy 2 — Core Provides Capability, Project Provides Business Behavior

Core là platform.

Project là nơi biết application business.

```text
Core
 ├── Browser
 ├── API
 ├── Authentication
 ├── Logging
 ├── Reporting
 ├── Retry
 └── Workflow abstraction

Project
 ├── LoginWorkflow
 ├── CreateOrderWorkflow
 ├── ApproveOrderWorkflow
 └── CustomerWorkflow
```

### Quy tắc

Nếu logic chỉ có ý nghĩa đối với một application cụ thể, mặc định phải đặt câu hỏi:

> Tại sao logic này cần nằm trong Core?

Nếu không có lý do architecture rõ ràng, logic phải nằm trong Project.

---

## Philosophy 3 — Dependency Direction Must Be Explicit

Dependency direction không được hình thành một cách ngẫu nhiên.

```text
Test
 ↓
Workflow
 ↓
UI / API
 ↓
Core
 ↓
Playwright
```

Core không được import:

```text
Project Workflow
Project Page
Project API
Project Domain Model
```

### Mục tiêu

Có thể thay đổi Project mà không cần sửa Core.

---

## Philosophy 4 — Abstraction Has to Earn Its Existence

Không tạo abstraction chỉ vì:

- Muốn code "đẹp".
- Muốn có thêm interface.
- Muốn áp dụng Design Pattern.
- Muốn giống Clean Architecture.

Một abstraction nên tồn tại khi nó giải quyết ít nhất một vấn đề thực tế:

```text
Coupling
Duplication
Changeability
Complexity
Testability
Replaceability
Lifecycle management
```

### Ví dụ

Có giá trị:

```text
ApiClient
```

vì có thể centralize:

```text
authentication
headers
timeout
retry
logging
error handling
```

Không nhất thiết có giá trị:

```text
GenericPlaywrightWrapper
```

nếu nó chỉ chuyển tiếp:

```javascript
wrapper.click()
    → page.click()
```

---

## Philosophy 5 — One Responsibility, One Reason to Change

Các thành phần phải có responsibility rõ ràng.

```text
Test
    → verification

Workflow
    → orchestration

Page
    → UI interaction

Component
    → reusable UI interaction

ApiClient
    → API interaction

Fixture
    → test lifecycle/data setup

Core
    → framework infrastructure
```

Nếu một class thay đổi vì quá nhiều nguyên nhân khác nhau, đó là dấu hiệu cần refactor.

---

## Philosophy 6 — Prefer Composition over Inheritance

Framework phải ưu tiên composition.

Ví dụ:

```javascript
class CreateOrderWorkflow {
    constructor(orderApi, orderPage, logger) {
        this.orderApi = orderApi;
        this.orderPage = orderPage;
        this.logger = logger;
    }
}
```

Workflow được compose từ các capability cần thiết.

Không nên tạo inheritance hierarchy sâu.

### Anti-pattern

```text
BaseTest
 ↓
BaseWebTest
 ↓
BaseAuthenticatedTest
 ↓
BaseCustomerTest
 ↓
BaseOrderTest
 ↓
CreateOrderTest
```

Inheritance sâu làm tăng coupling và làm behavior khó dự đoán.

---

## Philosophy 7 — Tests Must Be Deterministic and Isolated

Một test phải có thể chạy:

```text
alone
parallel
repeatedly
in different order
```

mà không phụ thuộc vào test khác.

Không nên:

```text
Test A creates Customer 001
        ↓
Test B expects Customer 001
```

Nên:

```text
Test A → own Customer
Test B → own Customer
```

Test data và state phải được kiểm soát.

---

## Philosophy 8 — Framework Should Hide Complexity, Not Hide Failure

Core có nhiệm vụ che giấu infrastructure complexity.

Ví dụ Test không cần biết:

```text
BrowserContext creation
API authentication
Retry mechanism
Trace configuration
Screenshot
Logging
```

Nhưng Core không được che giấu:

```text
Assertion failure
Application error
HTTP error
Unexpected UI state
Test data problem
```

Nếu framework retry một test, người dùng vẫn phải biết test đã retry.

Nếu framework catch exception, exception context vẫn phải được giữ lại.

---

## Philosophy 9 — Stable Public API, Replaceable Internal Implementation

Project chỉ nên phụ thuộc vào public API.

```text
Project
   ↓
@core/automation
   ↓
Public API
   ↓
Internal implementation
```

Project không được phụ thuộc vào:

```text
src/internal/*
```

Điều này cho phép Core team thay đổi:

```text
internal architecture
folder structure
implementation
dependency
Playwright integration
```

mà hạn chế ảnh hưởng đến Project.

---

## Philosophy 10 — Automation Code Is Production Code

Automation framework phải được phát triển với engineering discipline tương đương production software.

Bao gồm:

```text
Code Review
Unit Tests
Integration Tests
CI
Versioning
Documentation
Logging
Observability
Error Handling
Security
Dependency Management
Backward Compatibility
```

Không nên có tư duy:

> "Chỉ là test code nên viết nhanh cũng được."

Framework là infrastructure được nhiều Project phụ thuộc. Một lỗi trong Core có thể ảnh hưởng đồng thời đến nhiều test suite.

---

# 29. Design Patterns

Design Pattern không phải checklist bắt buộc phải sử dụng. Pattern chỉ được sử dụng khi nó giải quyết một vấn đề architecture cụ thể.

Pattern được phân thành:

```text
REQUIRED
RECOMMENDED
CONDITIONAL
AVOID
```

---

## 29.1 Adapter Pattern — REQUIRED

Adapter dùng để tạo boundary giữa framework abstraction và implementation.

```text
Workflow
   ↓
UI Adapter / API Adapter
   ↓
Playwright
```

### Mục đích

- Giảm coupling.
- Chuẩn hóa interaction.
- Cho phép thay đổi implementation.
- Cô lập Playwright-specific code.

### Quy tắc

Không để Test chứa Playwright infrastructure code nếu Core đã cung cấp abstraction tương ứng.

---

## 29.2 Facade Pattern — REQUIRED

Facade cung cấp API đơn giản cho Test.

Ví dụ:

```javascript
await app.authentication.loginAs(user);
await app.customer.create();
await app.order.create(order);
```

thay vì để Test phải điều phối:

```text
Browser
Page
API Request
Auth
Logger
Retry
Config
```

### Mục tiêu

Test phải làm việc với business-oriented API.

---

## 29.3 Dependency Injection — REQUIRED

Dependency phải được inject thay vì tạo tùy tiện bên trong class.

```javascript
class CreateOrderWorkflow {
    constructor(orderApi, orderPage, logger) {
        this.orderApi = orderApi;
        this.orderPage = orderPage;
        this.logger = logger;
    }
}
```

### Lợi ích

- Loose coupling.
- Testability.
- Replaceability.
- Controlled lifecycle.
- Dễ mock/stub trong unit test.

---

## 29.4 Factory Pattern — RECOMMENDED

Factory phù hợp khi việc tạo object có complexity hoặc lifecycle cần kiểm soát.

Các đối tượng có thể cần Factory:

```text
Browser
BrowserContext
ApiClient
Page Object
Workflow
Test Data
```

Ví dụ:

```javascript
const workflow = workflowFactory.create('CreateOrder');
```

Không sử dụng Factory nếu nó chỉ thêm một lớp chuyển tiếp không mang lại giá trị.

---

## 29.5 Builder Pattern — RECOMMENDED

Builder đặc biệt phù hợp cho complex test data.

```javascript
const order = new OrderBuilder()
    .withCustomer(customer)
    .withProduct(product)
    .withDiscount(10)
    .build();
```

### Nên dùng khi

- Object có nhiều optional fields.
- Test cần nhiều variations.
- Constructor trở nên khó đọc.
- Test data cần fluent API.

---

## 29.6 Strategy Pattern — RECOMMENDED

Strategy phù hợp khi một behavior có nhiều implementation.

Ví dụ authentication:

```text
AuthenticationStrategy
├── ApiAuthentication
├── UiAuthentication
└── TokenAuthentication
```

Hoặc:

```text
RetryStrategy
├── DefaultRetry
├── ApiRetry
└── UiRetry
```

Không dùng Strategy khi chỉ có một implementation và không có khả năng thay đổi.

---

## 29.7 Template Method — CONDITIONAL

Có thể dùng cho standardized lifecycle:

```text
Workflow
 ├── beforeExecute()
 ├── execute()
 └── afterExecute()
```

Chỉ sử dụng khi framework thực sự cần standardized extension points.

Không dùng Template Method để tạo inheritance hierarchy sâu.

---

## 29.8 Repository Pattern — CONDITIONAL

Không mặc định sử dụng Repository cho mọi test data.

Có thể dùng khi framework cần abstraction giữa nhiều data source:

```text
CustomerRepository
├── ApiCustomerRepository
├── DbCustomerRepository
└── FileCustomerRepository
```

Repository chỉ nên được đưa vào khi data access abstraction thực sự có architectural value.

---

## 29.9 Page Object Pattern — REQUIRED cho UI

Page Object encapsulate:

- Locator.
- UI interaction.
- Page state.
- Page-specific behavior.

Page Object không chứa business workflow lớn.

### Đúng

```javascript
await orderPage.selectCustomer(customer);
await orderPage.addProduct(product);
await orderPage.submit();
```

### Sai

```javascript
await orderPage.createCustomerAndOrderAndPay();
```

Business orchestration phải thuộc Workflow.

---

## 29.10 Component / Composite Pattern — RECOMMENDED cho UI

Reusable UI components nên được model hóa độc lập.

Ví dụ:

```text
DataTable
Dialog
Dropdown
DatePicker
Header
Navigation
Pagination
```

Page compose các Component thay vì duplicate locator/interaction logic.

```text
OrderPage
 ├── Header
 ├── CustomerSelector
 ├── ProductTable
 └── ConfirmationDialog
```

---

# 30. Design Patterns không nên sử dụng tùy tiện

## 30.1 Singleton — AVOID

Không nên dùng Singleton cho:

```text
Page
Browser
BrowserContext
TestContext
ApiClient
Workflow
```

vì Singleton dễ tạo:

- Global mutable state.
- Test interference.
- Parallel execution issues.
- Lifecycle khó kiểm soát.

Mỗi test nên có lifecycle phù hợp với BrowserContext/Page/Request context của nó.

---

## 30.2 God Object — AVOID

Không tạo các class kiểu:

```text
AutomationManager
TestManager
FrameworkManager
Helper
```

chứa hàng chục responsibilities.

Ví dụ không nên:

```javascript
automationManager.login();
automationManager.createCustomer();
automationManager.createOrder();
automationManager.takeScreenshot();
automationManager.retry();
automationManager.sendEmail();
automationManager.queryDatabase();
```

Đây là dấu hiệu nhiều responsibility đã bị gom vào một abstraction.

---

## 30.3 Deep Inheritance — AVOID

Không tạo inheritance hierarchy sâu:

```text
BaseTest
 ↓
BaseWebTest
 ↓
BaseAuthenticatedTest
 ↓
BaseCustomerTest
 ↓
BaseOrderTest
```

Ưu tiên composition.

---

## 30.4 Generic Wrapper Explosion — AVOID

Không tạo wrapper cho mọi Playwright API:

```text
GenericButton
GenericInput
GenericSelect
GenericLocator
GenericPage
GenericElement
```

nếu các abstraction này chỉ forward call:

```javascript
wrapper.click()
    → page.click()
```

Framework phải cung cấp abstraction có behavior/value thực sự.

---

# 31. Architecture Rules

Các rule dưới đây là governance rules của framework.

| Rule | Level | Nội dung |
|---|---|---|
| Core không import Project code | MUST | Core phải độc lập với consuming Project |
| Project không truy cập Core `internal/` | MUST | Chỉ sử dụng Public API |
| Business Workflow không nằm trong Page Object | MUST | Page chỉ xử lý UI interaction |
| Core không chứa application-specific business logic | MUST | Business knowledge thuộc Project |
| Test không chứa low-level Playwright infrastructure | MUST | Sử dụng Core abstraction khi đã có |
| Shared mutable state giữa test bị hạn chế nghiêm ngặt | MUST | Bảo đảm test isolation |
| Dependency nên được inject | SHOULD | Tránh hidden dependency |
| Composition ưu tiên hơn inheritance | SHOULD | Giảm coupling |
| Factory chỉ dùng khi creation có complexity | SHOULD | Tránh unnecessary abstraction |
| Không tạo abstraction chỉ để áp dụng pattern | MUST | Pattern phải giải quyết vấn đề thực tế |
| Public API phải được kiểm soát | MUST | Tránh accidental API |
| Breaking change phải được version hóa | MUST | Tuân thủ Semantic Versioning |
| Core change phải qua review | MUST | Bảo vệ framework |
| Core phải có automated tests | MUST | Không release behavior chưa được verify |
| Framework diagnostics phải observable | MUST | Không che giấu failure |

---

# 32. Architecture Governance

Do Core là shared infrastructure của nhiều Project, việc thay đổi Core phải được quản lý khác với project test thông thường.

## 32.1 Quy trình thay đổi Core

```text
Developer / Tester
        │
        │ đề xuất thay đổi
        ▼
Architecture / Framework Team
        │
        ▼
Architecture Review
        │
        ▼
Implementation
        │
        ▼
Unit / Integration Tests
        │
        ▼
Pull Request
        │
        ▼
Code Review
        │
        ▼
CI
        │
        ▼
Release
        │
        ▼
NPM Package
```

## 32.2 Khi nào cần Architecture Review?

Architecture Review nên được yêu cầu khi thay đổi:

- Public API.
- Dependency direction.
- Core module boundary.
- Workflow abstraction.
- UI/API Adapter contract.
- Test Context.
- Authentication architecture.
- Configuration architecture.
- Retry model.
- Reporting/observability architecture.
- Distribution mechanism.
- Breaking change.

Các bug fix nội bộ nhỏ có thể chỉ cần Code Review nếu không ảnh hưởng architecture.

---

## 32.3 Architecture Change Questions

Mỗi Pull Request thay đổi Core nên trả lời được:

1. Vì sao thay đổi này cần thiết?
2. Vấn đề architecture nào đang được giải quyết?
3. Vì sao implementation này thuộc Core thay vì Project?
4. Có abstraction mới không?
5. Nếu có, abstraction đó giải quyết vấn đề gì?
6. Design Pattern nào đang được sử dụng?
7. Vì sao Pattern này phù hợp?
8. Có thay đổi Dependency Direction không?
9. Có thay đổi Public API không?
10. Có Breaking Change không?
11. Các Project hiện tại có bị ảnh hưởng không?
12. Có automated tests cho behavior mới không?
13. Có ảnh hưởng Test Isolation không?
14. Có ảnh hưởng Parallel Execution không?
15. Có ảnh hưởng Performance không?
16. Có ảnh hưởng Debuggability/Observability không?

---

# 33. Definition of Done cho Core Change

Một thay đổi Core chỉ được xem là hoàn thành khi:

```text
[ ] Architecture boundary không bị phá vỡ
[ ] Dependency direction vẫn đúng
[ ] Public API được xác định rõ
[ ] Internal implementation không bị expose ngoài ý muốn
[ ] Automated tests được bổ sung/cập nhật
[ ] Existing tests vẫn pass
[ ] Parallel execution được kiểm tra khi có ảnh hưởng
[ ] Logging/diagnostics phù hợp
[ ] Documentation được cập nhật nếu cần
[ ] Version impact được xác định
[ ] Pull Request được review
[ ] CI pass
```

---

# 34. Architecture Smells

Các dấu hiệu sau cần được xem là cảnh báo architecture:

### Test chứa quá nhiều Playwright

```javascript
await page.locator(...);
await page.locator(...);
await page.locator(...);
await request.post(...);
```

### Workflow trở thành God Object

```text
CreateOrderWorkflow
 ├── login
 ├── customer
 ├── product
 ├── payment
 ├── email
 ├── database
 └── reporting
```

### Page Object chứa business logic

```text
OrderPage
 ├── createOrder
 ├── approveOrder
 ├── cancelOrder
 └── refundOrder
```

### Core chứa business knowledge

```text
Core
 ├── VietnamAirlinesInvoice
 ├── BelleClinicCustomer
 └── AeskulapAppointment
```

### Project phụ thuộc internal Core

```javascript
import X from '@core/automation/src/internal/X';
```

### Utility module trở thành dumping ground

```text
utils/
 ├── browser
 ├── auth
 ├── database
 ├── api
 ├── email
 ├── customer
 ├── order
 └── everything
```

Khi gặp các dấu hiệu này, cần xem xét refactoring thay vì tiếp tục bổ sung code vào cấu trúc hiện tại.

---

# 35. Architectural Decision Summary — Updated

| Decision | Lựa chọn | Lý do |
|---|---|---|
| Language | TypeScript | Theo yêu cầu |
| Automation engine | Playwright | Hỗ trợ UI + API |
| Main architecture | Test → Workflow → UI/API Adapter | Business-oriented và reusable |
| Core | Shared framework library | Dùng cho nhiều Project |
| Workflow abstraction | Core | Chuẩn hóa framework capability |
| Workflow implementation | Project | Giữ business knowledge ngoài Core |
| UI abstraction | Page Object + Component | Encapsulate UI behavior |
| API abstraction | API Adapter + API Client | Encapsulate endpoint behavior |
| Core distribution | Git + NPM | Hỗ trợ development và stable release |
| Core access | Restricted | Kiểm soát framework quality |
| Versioning | Semantic Versioning | Controlled upgrade |
| Dependency management | Dependency Injection | Loose coupling |
| Object creation | Factory khi cần | Controlled creation/lifecycle |
| Complex test data | Builder | Readability và variation |
| Variable behavior | Strategy | Replaceable behavior |
| Common lifecycle | Template Method khi cần | Standardized extension points |
| Data access abstraction | Repository khi cần | Chỉ khi có nhiều data source |
| UI modeling | Page Object | UI boundary |
| Reusable UI | Component/Composite | Giảm duplication |
| Generic Playwright wrapper | Không | Tránh abstraction explosion |
| Singleton | Không | Tránh global mutable state |
| Deep inheritance | Không | Ưu tiên composition |
| God Object | Không | Enforce Single Responsibility |
| Core business logic | Không | Core phải application-agnostic |
| Architecture review | Bắt buộc cho architectural changes | Governance |
| Core automated tests | Bắt buộc | Shared infrastructure |
| Public API | Controlled | Stable contract |
| Internal API | Không được consume từ Project | Replaceable implementation |

---

# 36. Core Architectural Principle

Nguyên tắc cao nhất của framework:

> **Core cung cấp capabilities và contracts. Project cung cấp business behavior. Workflow mô tả business operation. Test mô tả behavior cần verify. Playwright là implementation detail trong phạm vi hợp lý.**

Có thể tóm tắt toàn bộ architecture bằng bốn câu:

```text
Core
  = HOW the automation platform works

Project
  = HOW this application is automated

Workflow
  = WHAT business operation is performed

Test
  = WHAT behavior is verified
```

Nếu một thay đổi mới không phù hợp với bốn nguyên tắc trên, thay đổi đó phải được xem xét lại trước khi đưa vào Core.


# 37. Target Architecture

Kiến trúc cuối cùng:

```text
                    COMPANY AUTOMATION PLATFORM
                              │
                              │
                  ┌───────────▼───────────┐
                  │   AUTOMATION CORE     │
                  │                       │
                  │ Workflow abstraction  │
                  │ Test Context          │
                  │ Playwright lifecycle  │
                  │ UI infrastructure     │
                  │ API infrastructure    │
                  │ Fixtures              │
                  │ Authentication        │
                  │ Configuration         │
                  │ Logging               │
                  │ Reporting             │
                  │ Retry                 │
                  └───────────┬───────────┘
                              │
                   NPM / Git distribution
                              │
              ┌───────────────┼────────────────┐
              │               │                │
              ▼               ▼                ▼
         PROJECT A        PROJECT B        PROJECT C
              │               │                │
       ┌──────┴──────┐ ┌──────┴──────┐ ┌──────┴──────┐
       │             │ │             │ │             │
      Tests       Workflows        Tests         Workflows
                    │
              ┌─────┴─────┐
              ▼           ▼
             UI           API
              │           │
         Page/Object   API Client
              │           │
              └─────┬─────┘
                    ▼
                Playwright
```
