# @jarvis/autotest — Core (production)

Engine-agnostic test runtime: HTTP qua port, auth composable, reporting, retry. **Không** chứa selector hay URL sản phẩm VNA.

## Kiến trúc

| Layer | Thành phần | Trách nhiệm |
|-------|------------|-------------|
| Port | `IHttpTransport`, `IBrowserDriver` | Adapter Playwright / fetch / engine khác |
| API | `ApiClient` | JSON REST, mọi verb HTTP, `requestRaw` cho assert status |
| Auth | `AuthStrategy` | `ApiKeyAuth`, `BearerTokenAuth`, `CompositeAuth` |
| Runtime | `Configuration`, `TestContext`, `RunContext` | Env, runId, thư mục report |
| Resilience | `pollUntil` | Chờ điều kiện async (integration, seed data) |

Satellite: `@jarvis/autotest.playwright` implement port.

## ApiClient

- **JSON helpers (protected):** `get`, `head`, `post`, `put`, `patch`, `delete` — ném `ApiClientError` khi `!ok()` hoặc body không parse JSON.
- **`requestRaw(method, path, options?)`:** luôn trả `{ ok, status, url, text, body }` — dùng ABAC/404, envelope lỗi.
- **`buildUrl`:** nối `baseUrl` (không slash cuối) + path; query bỏ qua `undefined`.
- **Header:** merge `authHeaders` + `options.headers`; POST/PUT/PATCH/DELETE thêm `Content-Type: application/json` nếu chưa có.

```typescript
class ProductApi extends ApiClient {
  listItems() {
    return this.get<ItemPage>('/api/items');
  }
  updateItem(id: string, body: ItemDto) {
    return this.put<ItemDto>(`/api/items/${id}`, { body });
  }
}

const raw = await client.requestRaw('GET', `/api/items/${id}`);
if (raw.status === 404) { /* ABAC */ }
```

## Cấu hình

```typescript
import { loadConfiguration, assertConfigurationReady } from '@jarvis/autotest';

const config = loadConfiguration();
assertConfigurationReady(config);
```

Biến môi trường (ưu tiên trái → phải): `AUTOTEST_BASE_URL`, `BASE_URL`, `WEB_URL`.  
Report root: `AUTOTEST_REPORTS_ROOT`, `REPORTS_ROOT` (mặc định `./reports`).

## Auth

```typescript
import { BearerTokenAuth, ApiKeyAuth, CompositeAuth } from '@jarvis/autotest';

const headers = new CompositeAuth([
  new BearerTokenAuth(token),
  { applyHeaders: () => ({ 'X-Tenant': tenantCode }) },
]).applyHeaders();
```

## Lỗi

`ApiClientError`: `status`, `body`, `url`, `method?` — message cắt body > 500 ký tự (log an toàn).

## Mở rộng sản phẩm

1. `extends ApiClient` trong repo consumer (`autotest/src/integrations/…`).
2. `createPlaywrightTransport(request)` + `HarnessDependencies` nếu cần registry client.
3. Không sửa core cho một endpoint — thêm method trên subclass.

## Chất lượng

```bash
cd jarvis/autotest/core
npm run typecheck
npm test
```

Mọi thay đổi port/API phải có unit test trong `tests/unit/`.
