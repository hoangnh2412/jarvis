# Workflow: Thêm version / security Swagger

Áp dụng khi **đã có** `AddCoreSwagger()` và cần mở rộng.

## Checklist

```text
- [ ] 1. Chọn provider (jwt-security | api-key-security) hoặc thêm version
- [ ] 2. Cập nhật Swagger:Versions và SecuritySchemes
- [ ] 3. Đảm bảo authentication runtime đã cấu hình (jarvis-dotnet authentication)
- [ ] 4. Validate Swagger UI authorize
```

## Thêm API version

1. `Swagger:Versions`: `["v1", "v2"]`
2. Controller/action `[ApiVersion("2.0")]`
3. `AddApiVersioning` / explorer đã bật

## Security providers

| Scheme | SKILL |
|--------|-------|
| JWT | [providers/jwt-security/SKILL.md](../providers/jwt-security/SKILL.md) |
| API Key | [providers/api-key-security/SKILL.md](../providers/api-key-security/SKILL.md) |

## Anti-patterns

- Bật security scheme mà chưa `AddAuthentication` tương ứng
- Tắt `ApiResponseDocumentationPathPrefixes` cho route cần `BaseResponse` schema
