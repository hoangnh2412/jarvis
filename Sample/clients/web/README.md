# Sample web client (Vite + React)

Source SPA. Build output → `Sample/wwwroot`.

## Chạy 1 lệnh (khuyến nghị)

Profile `http` **không dùng SpaProxy** — ASP.NET serve UI từ `wwwroot`.

```powershell
dotnet run --project Sample --launch-profile http
```

- App: `http://localhost:5167`
- Swagger: `http://localhost:5167/swagger`
- Lần build Debug sẽ `npm run build` → `wwwroot` (bỏ qua bằng `-p:SkipFrontendBuild=true`)

Đổi UI frontend → build lại (hoặc `dotnet build`) rồi refresh; không có HMR.

## HMR (2 process — SpaProxy + Vite)

```powershell
dotnet run --project Sample --launch-profile http-hmr
```

- API: `http://localhost:5167`
- Vite: `http://127.0.0.1:5173`

## Elsa Embedded hoặc hosts tách riêng

Mặc định Sample chạy ở `Embedded`: Elsa API, Studio và Preview cùng chạy trên Sample host,
vì vậy không cần cấu hình URL frontend.

Khi chuyển Sample sang `Standalone`, chỉ cần cấu hình `Elsa:ServerUrl`. Nếu Studio chạy
ở host khác Workflow Server, cấu hình thêm `Elsa:StudioUrl`. Backend Sample sẽ redirect
route relative `/preview/{definitionId}/{instanceId?}` đến Studio, nên không cần sửa hoặc
build lại `OnboardingDemoPage`.

Khi chạy frontend bằng Vite độc lập, đặt `VITE_WORKFLOW_SERVER_URL` bằng origin của
Workflow Studio (ví dụ `https://localhost:56259`) để iframe và liên kết preview gọi đúng
standalone host. Có thể dùng `VITE_WORKFLOW_PREVIEW_PROXY_TARGET` nếu chỉ muốn đổi target
proxy `/preview` trong môi trường development.

Host Studio phải expose `/preview/{definitionId}/{instanceId?}` và cấu hình
`Elsa:ApiPathPrefix` là `/elsa/api`.
Nếu trang preview dùng persistence trực tiếp như host hiện tại, Studio và Server phải
dùng chung database/schema Elsa.

## Build tay vào wwwroot

```powershell
cd Sample/clients/web
npm install
npm run build
```

Lần đầu cần build `@platform/core` nếu chưa có `dist`:

```powershell
cd frameworks/frontend
npm install
npm run build
```
