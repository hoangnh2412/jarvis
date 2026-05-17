# {Product} Backend

ASP.NET Core **.NET 9** API — phân lớp Clean Architecture + [Jarvis]({JarvisRoot}) framework.

## Cấu trúc

```text
src/
├── {Product}.Domain.Shared
├── {Product}.Domain
├── {Product}.Application
├── {Product}.Infrastructure
└── {Product}.Host          ← F5 startup project
```

## Chạy local

1. PostgreSQL (connection string trong `src/{Product}.Host/appsettings.Development.json`).
2. `dotnet run --project src/{Product}.Host`
3. Swagger: https://localhost:7006/swagger
4. `GET /api/ping`

## Health

- `/health/live` — liveness
- `/health/ready` — readiness (cần DB khi bật PostgreSQL check)
