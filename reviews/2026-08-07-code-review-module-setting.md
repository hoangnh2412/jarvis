# Code review: `module-setting` vs `develop`

| Mục | Giá trị |
|-----|---------|
| Branch | `module-setting` |
| Base | `origin/develop` |
| Merge-base | `138c586f4cb28543e8ecfdaac70fc97a8e4a6599` |
| Phạm vi | ~116 files (Setting module + Sample + frontend) |
| Ngày review | 2026-08-07 |
| Trọng tâm | Bug, regression hành vi, bảo mật, thiếu test |

---

## Findings (theo mức độ nghiêm trọng)

### [P1] HTTP API Setting không auth nhưng trả secret dạng plaintext

**File:** `modules/settings/Platform.Modules.Setting.API/Controllers/SettingController.cs`

`UseHttpApi()` expose CRUD/form mà **không** có `[Authorize]`. Sample gắn API này nhưng không gọi `UseAuthorization()`; tenant lấy từ header `X-Tenant-Id`. Frontend còn hardcode tenant id với `apiKey` rỗng.

Ai gọi được Sample và gửi `X-Tenant-Id` đều có thể `GET /form?group=Email` và nhận `Email.Password` (và secret khác) dưới dạng **plaintext**. AES at-rest không bảo vệ mặt HTTP. Host copy Sample dễ mang đúng lỗ hổng này sang production.

**Đề xuất:** Bắt buộc auth/policy trên host mẫu; hoặc mask secret trên HTTP response nếu caller không thật sự cần plaintext.

---

### [P1] Sample gỡ `Setting:EncryptionKey` — không lưu được password/encrypted

**File:** `Sample/appsettings.json` (commit `6be8cdd`)

`Demo.Password` / `Email.Password` đều `IsEncrypted`. `SettingEncryptor` ném `98106` khi thiếu key. Block `Setting` và `Cache:Items:Setting` đã bị xóa khỏi Sample → lưu nhóm có secret sẽ fail lúc encrypt. Luồng demo UI cho Email/Demo password bị gãy.

**Đề xuất:** Khôi phục cấu hình `Setting:EncryptionKey` + `Cache:Items:Setting` cho Sample (key demo / env, không commit key thật nếu môi trường prod).

---

### [P1] Tài liệu còn nói “password rỗng = giữ nguyên”, code đã ghi đè bằng chuỗi rỗng

**File:** `openapi.md` / `README.md` / ADR vs `SettingManager` / `ISettingManager`

Code + `ISettingManager` hiện persist secret rỗng thành rỗng (vẫn mã hóa). Nhưng contract docs vẫn dạy ngược lại:

- `openapi.md`: ví dụ `value: ""` “giữ nguyên mật khẩu”
- Ma trận test: “password rỗng … Secret cũ được giữ nguyên”
- Bảng type trong `README`: “rỗng khi update = giữ nguyên”
- ADR mục tiêu #7 vẫn đánh dấu **Đạt** cho keep-on-blank

Client bỏ/clear password khi save (pattern phổ biến) sẽ **xóa** secret SMTP/demo. Cần đồng bộ docs với code, hoặc khôi phục keep-on-blank trong `UpdateAsync` / `SaveGroupAsync`.

---

### [P2] `DeleteAsync` bỏ qua `IsReadOnly` của Library

**File:** `modules/settings/Platform.Modules.Setting/Services/SettingManager.cs`

`UpdateAsync` chặn khi `entity.IsReadOnly || definition.IsReadOnly`. `DeleteAsync` chỉ kiểm tra `entity.IsReadOnly`.

Nếu definition đổi sang `IsReadOnly = true` sau khi row tạo với `IsReadOnly = false`, update bị chặn nhưng `DELETE /{key}` vẫn thành công.

**Đề xuất:** Áp dụng cùng guard như update; cân nhắc `SaveGroupAsync` cũng check `entity.IsReadOnly`.

---

### [P2] OpenAPI hứa `401` khi chưa đăng nhập; Sample không enforce

**File:** `modules/settings/Platform.Modules.Setting/doc/openapi.md`

Case “Chưa đăng nhập → 401” không đúng với controller + pipeline Sample hiện tại. Dễ gây hiểu nhầm khi kiểm thử bảo mật và tích hợp host.

---

### [P2] Sample gỡ cache item `Setting` — cache silent no-op

**File:** `Sample/appsettings.json`

Không còn `Cache:Items:Setting` thì `GetOrSetAsync` không resolve được entry (`Resolve` → null). Đọc luôn vào DB. Không crash, nhưng trái README và tắt đường cache mà module thiết kế quanh đó.

---

## Đánh giá tổng quan

Kiến trúc module ổn (Library definition, AES-GCM at-rest, cache ciphertext, validate theo type, form API, frontend feature pack). Trước khi merge nên:

1. Sửa cấu hình Sample (encryption key + cache item)
2. Khóa auth trên host mẫu (hoặc không trả plaintext secret qua HTTP)
3. Thống nhất contract password rỗng giữa code và docs

**Thiếu test:** chưa có unit/integration test cho encrypt/decrypt, empty-secret, `IsReadOnly` delete/update, validator types, cache invalidate — README vẫn để TODO. Rủi ro còn lại cao nhất nằm ở secret handling + auth.

**Ghi chú WIP local:** working tree đang rename `AddPlatformSetting` → `AddCoreSetting` (chưa commit). Nếu commit, cần cập nhật README/XML docs cùng lúc.
