# Jarvis Autotest

Platform **test-time** (TypeScript): không nằm trong `Jarvis.sln`, không đi vào runtime Host.

| Package / folder | Vai trò |
|------------------|---------|
| `@jarvis/autotest` (`core/`) | Contract engine-agnostic: `ApiClient`, `IHttpTransport`, `IBrowserDriver`, `Workflow`, reporting, `pollUntil` |
| `@jarvis/autotest.playwright` (`playwright/`) | Satellite Playwright — `PlaywrightTransport`, `PlaywrightBrowserDriver` |
| `sample/` | Consumer SAD §17 chống Host [`Sample/`](../Sample/) (API + SPA). **Không** copy vào `Sample/clients/` |

Keyword **`automation`** dành cho Agent/AI — không đặt `@jarvis/automation*`.

## Cài đặt

```bash
cd autotest
npm install
npm run build
npx playwright install chromium   # cho sample UI
```

Unit test core:

```bash
npm run test:unit
```

## Sample (cần Sample Host đang chạy)

```bash
dotnet run --project Sample
cd autotest/sample
cp .env.example .env
npm test                 # API + UI
npm run test:api
npm run test:ui
npm run test:smoke
```

Login UI dùng tài khoản mock SPA: `admin@gmail.com` / `Admin@123`.
