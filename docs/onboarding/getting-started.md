# Onboarding — Bắt đầu với Jarvis Platform

Checklist cho dev mới vào monorepo `Jarvis_framework`.

## 1. Chuẩn bị môi trường

- [.NET SDK](https://dotnet.microsoft.com/download) phù hợp solution (`net9.0` — xem `global.json` / project nếu có)
- IDE: Visual Studio / Rider / VS Code + C# Dev Kit
- (Tùy Sample) PostgreSQL, Redis — xem `Sample/appsettings.json` và `ConnectionStrings`

## 2. Clone và build

```bash
dotnet build Platform.sln
```

Chạy thử host demo:

```bash
dotnet run --project Sample/Sample.csproj
```

## 3. Hiểu cây thư mục

| Thư mục | Vai trò |
|---------|---------|
| `frameworks/` | Package hạ tầng Atomic (NuGet `Platform.*`) |
| `modules/` | Bounded context (Account, Setting, …) |
| `Sample/` | Host demo API + SPA |
| `UnitTest/` | Test framework / module |
| `ADRs/` | Quyết định kiến trúc |

Chi tiết: [README.md](../../README.md).

## 4. Bước tiếp theo

1. Đọc [features/README.md](../features/README.md) — platform có những capability gì.
2. Đọc [guides/README.md](../guides/README.md) — tích hợp phần bạn đang làm (auth, cache, …).
3. Khi đụng thiết kế / constraint: tra [ADRs/README.md](../../ADRs/README.md).

## 5. Quy ước làm việc (tóm tắt)

- Ưu tiên `ProjectReference` trong monorepo; product bên ngoài dùng NuGet feed.
- Lưu ý đặt tên: folder `Platform.Authentication.*` vs package `Platform.Authentications.*` (có **s**) — xem README root.

---

*Tài liệu này sẽ được bổ sung: Docker compose, seed DB, biến môi trường Sample, quy trình PR.*
