# Sample web client (Vite + React)

Source SPA. Build output → `Sample/wwwroot`. ASP.NET serve static từ đó.

## Chạy Sample

```powershell
dotnet run --project Sample --launch-profile http
```

- App: `http://localhost:5167` (mở `/settings`)
- Swagger: `http://localhost:5167/swagger`
- Debug build tự `npm run build` → `wwwroot` (bỏ qua: `-p:SkipFrontendBuild=true`)

Đổi UI → `dotnet build` (hoặc `npm run build` trong thư mục này) rồi refresh.

## Build tay

```powershell
cd Sample/clients/web
npm install
npm run build
```

Lần đầu cần build `@jarvis/core` nếu chưa có `dist`:

```powershell
cd frameworks/frontend
npm install
npm run build
```
