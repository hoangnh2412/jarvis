# Workflow: Thêm scheme authentication

Áp dụng khi **đã có** một scheme và cần thêm scheme thứ hai hoặc đổi provider.

## Checklist

```text
- [ ] 1. Đọc provider mới (jwt | api-key | cognito)
- [ ] 2. Thêm package nếu chưa có
- [ ] 3. Chain thêm vào AddAuthentication (policy name rõ ràng)
- [ ] 4. Cập nhật Swagger SecuritySchemes nếu cần
- [ ] 5. Controller [Authorize] / policy đúng scheme
```

## Multi-scheme

Jarvis hỗ trợ cấu hình qua `Authentication:Type` — khi cần nhiều scheme, đăng ký từng extension và gán policy trên controller.

## API Key provider

Implement `IApiKeyProvider` — xem [providers/api-key/SKILL.md](../providers/api-key/SKILL.md).

## Cognito

ProjectReference / package Cognito — xem [providers/cognito/SKILL.md](../providers/cognito/SKILL.md).
