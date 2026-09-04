# Workflow: Chỉnh foundation Jarvis

Áp dụng khi **đã có** `AddCoreJson` và cần đổi policy CORS, JSON, hoặc ApiResponseWrapper.

## Checklist

```text
- [ ] 1. Xác định thay đổi (Cors | Json | Wrapper includes)
- [ ] 2. Cập nhật appsettings tương ứng
- [ ] 3. Không phá thứ tự pipeline UseCoreCors → UseCoreMiddleware → MapControllers
- [ ] 4. Validate response format / CORS
```

## CORS

Section `Cors` — origins, methods, headers theo môi trường. Không hard-code production origin trong repo.

## ApiResponseWrapper

```json
"Middlewares": {
  "ApiResponseWrapper": {
    "IsEnable": true,
    "Includes": ["^/api"]
  }
}
```

- Thêm prefix nếu API nằm ngoài `/api`
- Tắt `IsEnable` cho path debug nếu cần raw response

## JSON

```json
"Json": {
  "IgnoreNull": true,
  "NamingPolicy": "CamelCase"
}
```

## Anti-patterns

- `UseCoreMiddleware` sau `MapControllers` — middleware không chạy đúng
- Bỏ `AddCoreDomain` khi app dùng `IWorkContext` / enricher tenant
