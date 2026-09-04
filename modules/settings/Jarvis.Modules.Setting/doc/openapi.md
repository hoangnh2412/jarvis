# Tài liệu API Quản lý cấu hình (SettingManagement)

Tài liệu mô tả các API dùng để **xem và lưu cấu hình** của từng khách hàng (ví dụ: cấu hình gửi email). Phù hợp cho BA viết kịch bản nghiệp vụ và Tester kiểm thử.

**Đường dẫn gốc:** `/api/v1/settings`

Mọi phản hồi thành công / lỗi đều bọc trong cùng một **khung JSON chung** — xem [Phụ lục: Khung phản hồi chung](#response-body-envelope).  
Danh sách mã lỗi nghiệp vụ: [Phụ lục: Mã lỗi](#setting-error-codes).

### Thuật ngữ dùng trong tài liệu

| Thuật ngữ | Ý nghĩa đơn giản |
|-----------|------------------|
| Nhóm (`group`) | Nhóm cấu hình trên màn hình (vd. `Email`) |
| Mã cấu hình (`key`) | Mã định danh một ô cấu hình (vd. `Email.Host`) |
| Giá trị (`value`) | Nội dung người dùng nhập / hệ thống lưu |
| Danh mục có sẵn | Các nhóm/ô cấu hình do sản phẩm khai báo sẵn — không phải khách hàng tự tạo tên mới |
| Khách hàng (`tenant`) | Đơn vị thuê hệ thống; mỗi khách hàng có bộ giá trị cấu hình riêng |
| Chỉ đọc (`isReadOnly`) | Không cho phép sửa hoặc xóa |
| Cần bảo mật (`isEncrypted`) | Giá trị nhạy cảm (vd. mật khẩu); khi lưu non-empty sẽ được mã hóa at-rest, khi trả về API vẫn là plaintext. Chuỗi rỗng/whitespace **không** được lưu (`98107`). Client bỏ key khỏi payload nếu không đổi secret. |

### Auth & phân quyền (ranh giới module)

Package Setting **không** gắn `[Authorize]` và **không** trả `401`/`403` tự động. Khi host chưa bật auth, gọi API **vẫn thành công** như bình thường (chỉ cần ngữ cảnh tenant nếu host có middleware). Host muốn bảo vệ thì tự thêm policy — xem mẫu `Sample/Settings/SettingHttpApiAuthorizationSample.cs`. Các mục dưới ghi `Authorization` là **tuỳ host**, không phải bắt buộc của module.

---

## 1. API lấy danh sách nhóm cấu hình

Lấy danh sách các **nhóm cấu hình** để hiển thị menu / tab trên màn hình (vd. nhóm Email). Chỉ trả thông tin mô tả nhóm, **không** lấy giá trị đã lưu của khách hàng.

- **Phương thức:** `GET`
- **Đường dẫn:** `/api/v1/settings/groups`

### Dữ liệu gửi lên (Request)

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host** (module không bắt buộc)
    - `RequestId`: `{uuid}` — mã theo dõi request do phía gọi tự tạo (dùng khi hỗ trợ / tra cứu)

- **Body:** không có.

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã `RequestId` đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ khi cần hỗ trợ kỹ thuật.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là **danh sách nhóm**:

**Mỗi phần tử trong `data`:**

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `name` | string | Mã nhóm (vd. `Email`). |
| `displayName` | string | Tên hiển thị trên giao diện (tiêu đề tab / mục menu). |
| `description` | string \| null | Mô tả ngắn / gợi ý của nhóm. |
| `order` | integer | Thứ tự sắp xếp (số nhỏ hiện trước). |

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": [
        {
            "name": "Email",
            "displayName": "Email",
            "description": "Cấu hình gửi email SMTP",
            "order": 10
        }
    ]
}
```

### Khi lỗi

- **Headers:** giống khi thành công (`RequestId`, `TraceId` nếu có).
- **Mã HTTP (ví dụ):**
    - **401** / **403** — Chỉ khi **host** đã gắn Auth/policy; module Setting không trả các mã này.
    - **500** — Lỗi hệ thống.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi (xem phụ lục).

---

## 2. API lấy form cấu hình theo nhóm

**API chính** khi mở màn hình cấu hình. Một lần gọi trả về **toàn bộ ô nhập** của nhóm: nhãn, kiểu ô, danh sách chọn… kèm **giá trị hiện tại** (đã lưu của khách hàng, hoặc giá trị mặc định nếu chưa lưu lần nào).

- **Phương thức:** `GET`
- **Đường dẫn:** `/api/v1/settings/form`

### Dữ liệu gửi lên (Request)

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Tham số trên URL (Query)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `group` | string | Có* | Mã nhóm (vd. `Email`). *Một số môi trường demo có thể mặc định `Email` nếu bỏ trống — nên luôn gửi rõ. |

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là **danh sách ô trên form**:

**Mỗi phần tử trong `data`:**

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `group` | string | Mã nhóm. |
| `key` | string | Mã cấu hình (vd. `Email.Host`). |
| `name` | string | Nhãn hiển thị bên cạnh ô nhập (vd. “Host”, “Mật khẩu”). |
| `type` | string | Kiểu ô nhập: `Text` (một dòng), `Number` (số), `Textarea` (nhiều dòng), `Combobox` (chọn một), `Password` (mật khẩu). |
| `options` | string \| null | Danh sách lựa chọn cho Combobox, dạng `mã:nhãn\|mã:nhãn\|…`. |
| `description` | string \| null | Gợi ý / chú thích dưới ô nhập. |
| `defaultValue` | string \| null | Giá trị mặc định khi chưa lưu. |
| `isReadOnly` | boolean | `true` = không cho sửa/xóa. |
| `isEncrypted` | boolean | `true` = giá trị nhạy cảm (lưu có mã hóa); **API vẫn trả giá trị đọc được**. |
| `value` | string | Giá trị đang dùng trên form (đã lưu, hoặc mặc định nếu chưa lưu). |
| `isPersisted` | boolean | `true` = khách hàng đã lưu ô này trước đó; `false` = chưa lưu, đang dùng mặc định. |

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": [
        {
            "group": "Email",
            "key": "Email.Host",
            "name": "Host",
            "type": "Text",
            "options": null,
            "description": null,
            "defaultValue": "",
            "isReadOnly": false,
            "isEncrypted": false,
            "value": "smtp.gmail.com",
            "isPersisted": true
        },
        {
            "group": "Email",
            "key": "Email.Port",
            "name": "Port",
            "type": "Number",
            "options": null,
            "description": null,
            "defaultValue": "587",
            "isReadOnly": false,
            "isEncrypted": false,
            "value": "587",
            "isPersisted": true
        },
        {
            "group": "Email",
            "key": "Email.Password",
            "name": "Password",
            "type": "Password",
            "options": null,
            "description": null,
            "defaultValue": "",
            "isReadOnly": false,
            "isEncrypted": true,
            "value": "secret-plain",
            "isPersisted": true
        },
        {
            "group": "Email",
            "key": "Email.EnableSsl",
            "name": "Enable SSL",
            "type": "Combobox",
            "options": "true:Yes|false:No",
            "description": null,
            "defaultValue": "true",
            "isReadOnly": false,
            "isEncrypted": false,
            "value": "true",
            "isPersisted": false
        }
    ]
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **400** — Thiếu hoặc sai tham số `group`.
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Nhóm không có trong danh mục cấu hình có sẵn (`98102`).
    - **500** — Lỗi hệ thống / lỗi khóa mã hóa (`98106`).

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98102",
    "message": "Setting definition was not registered in the library.",
    "errors": [
        {
            "fieldName": "group",
            "message": "Group 'Payment' is not defined."
        }
    ]
}
```

---

## 3. API lấy danh sách định nghĩa cấu hình (chỉ thông tin mô tả)

Lấy **danh mục các ô cấu hình** (nhãn, kiểu ô, mặc định…) **không kèm giá trị** khách hàng đã lưu. Dùng khi chỉ cần xem “hệ thống có những ô nào”, không mở form để sửa.

- **Phương thức:** `GET`
- **Đường dẫn:** `/api/v1/settings/definitions`

### Dữ liệu gửi lên (Request)

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Tham số trên URL (Query)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `group` | string | Không | Lọc theo mã nhóm; bỏ trống thì trả toàn bộ danh mục. |

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là danh sách định nghĩa:

**Mỗi phần tử trong `data`:**

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `group` | string | Mã nhóm. |
| `key` | string | Mã cấu hình (duy nhất trong một khách hàng). |
| `name` | string | Nhãn hiển thị. |
| `type` | string | Kiểu ô nhập (Text, Number, …). |
| `options` | string \| null | Danh sách lựa chọn (nếu là Combobox). |
| `description` | string \| null | Gợi ý / chú thích. |
| `defaultValue` | string \| null | Giá trị mặc định khi tạo mới. |
| `isReadOnly` | boolean | Không cho sửa/xóa nếu `true`. |
| `isEncrypted` | boolean | Giá trị nhạy cảm — lưu có mã hóa. |

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": [
        {
            "group": "Email",
            "key": "Email.Host",
            "name": "Host",
            "type": "Text",
            "options": null,
            "description": null,
            "defaultValue": "",
            "isReadOnly": false,
            "isEncrypted": false
        },
        {
            "group": "Email",
            "key": "Email.EnableSsl",
            "name": "Enable SSL",
            "type": "Combobox",
            "options": "true:Yes|false:No",
            "description": null,
            "defaultValue": "true",
            "isReadOnly": false,
            "isEncrypted": false
        }
    ]
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **500** — Lỗi hệ thống.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

---

## 4. API lấy giá trị cấu hình đã lưu theo nhóm

Lấy các cấu hình **đã được khách hàng lưu** thuộc một nhóm.  
**Khác với API form (mục 2):** không trả các ô chưa từng lưu; không điền sẵn giá trị mặc định.

- **Phương thức:** `GET`
- **Đường dẫn:** `/api/v1/settings`

### Dữ liệu gửi lên (Request)

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Tham số trên URL (Query)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `group` | string | Có* | Mã nhóm (vd. `Email`). *Một số môi trường demo có thể mặc định `Email` nếu bỏ trống. |

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là danh sách cấu hình đã lưu (có thể **rỗng** nếu khách hàng chưa lưu gì):

**Mỗi phần tử trong `data`:**

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `id` | string (uuid) | Mã bản ghi. |
| `tenantId` | string (uuid) | Mã khách hàng sở hữu bản ghi. |
| `group` | string | Mã nhóm. |
| `key` | string | Mã cấu hình. |
| `name` | string | Nhãn hiển thị (lưu lại lúc tạo). |
| `value` | string | Giá trị đọc được (đã giải mã nếu là mật khẩu / dữ liệu bảo mật). |
| `type` | string | Kiểu ô nhập. |
| `options` | string \| null | Danh sách lựa chọn (nếu có). |
| `description` | string \| null | Gợi ý / chú thích. |
| `isReadOnly` | boolean | Không cho sửa/xóa nếu `true`. |

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": [
        {
            "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ab",
            "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "group": "Email",
            "key": "Email.Host",
            "name": "Host",
            "value": "smtp.gmail.com",
            "type": "Text",
            "options": null,
            "description": null,
            "isReadOnly": false
        }
    ]
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **400** — Thiếu hoặc sai tham số `group`.
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **500** — Lỗi hệ thống / chưa xác định được khách hàng đang thao tác (`98105`).

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

---

## 5. API lưu cả form theo nhóm

**API chính** khi người dùng bấm **Lưu** trên màn hình cấu hình.  
Gửi nhiều mã cấu hình cùng lúc: **chưa có thì tạo mới, đã có thì cập nhật**. Đây là **partial upsert**: chỉ các key có trong `values` được xử lý; key không gửi lên được giữ nguyên.  
Mỗi mã phải thuộc đúng nhóm và có trong danh mục có sẵn; ô **chỉ đọc** không được ghi đè.

- **Phương thức:** `PUT`
- **Đường dẫn:** `/api/v1/settings/group/{group}`

### Dữ liệu gửi lên (Request)

- **Tham số trên đường dẫn (Path)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `group` | string | Có | Mã nhóm cần lưu (vd. `Email`). |

- **Headers**
    - `Content-Type`: `application/json`
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Body (JSON)**

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `values` | object (map string → string) | Có | Danh sách cặp mã cấu hình và giá trị cần lưu (toàn bộ hoặc một phần form). Password/encrypted: chuỗi rỗng/whitespace → `98107` (không mã hóa, không lưu). Bỏ key khỏi `values` nếu không đổi secret. |

```json
{
    "values": {
        "Email.Host": "smtp.gmail.com",
        "Email.Port": "587",
        "Email.Password": "secret-plain",
        "Email.EnableSsl": "true"
    }
}
```

> **Quy ước secret:** Password/IsEncrypted chỉ lưu khi giá trị non-empty (mã hóa at-rest). Chuỗi rỗng/whitespace → `98107`. Muốn giữ secret cũ khi lưu form: **không gửi** key đó trong `values` (partial upsert). Khi đọc, API trả plaintext.

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là danh sách cấu hình **sau khi lưu** (cùng cấu trúc mục 4).

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": [
        {
            "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ab",
            "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "group": "Email",
            "key": "Email.Host",
            "name": "Host",
            "value": "smtp.gmail.com",
            "type": "Text",
            "options": null,
            "description": null,
            "isReadOnly": false
        },
        {
            "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ac",
            "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "group": "Email",
            "key": "Email.Port",
            "name": "Port",
            "value": "587",
            "type": "Number",
            "options": null,
            "description": null,
            "isReadOnly": false
        }
    ]
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **400** — JSON sai cấu trúc, thiếu `values`, hoặc secret rỗng (`98107`).
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Nhóm hoặc mã cấu hình không có trong danh mục (`98102`).
    - **409** — Xung đột nghiệp vụ (vd. mã cấu hình không thuộc nhóm đang lưu).
    - **422** — Ô chỉ đọc (`98104`) hoặc vi phạm quy tắc nghiệp vụ.
    - **500** — Lỗi hệ thống / chưa xác định khách hàng / lỗi mã hóa (`98105`, `98106`).

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98104",
    "message": "Setting is read-only and cannot be modified.",
    "errors": [
        {
            "fieldName": "Email.SystemId",
            "message": "Read-only setting cannot be updated."
        }
    ]
}
```

---

## 6. API lấy một cấu hình theo mã

Lấy **một** cấu hình đã lưu theo mã (`key`). Nếu khách hàng chưa từng lưu mã này → trả lỗi không tìm thấy (404).

- **Phương thức:** `GET`
- **Đường dẫn:** `/api/v1/settings/{key}`

### Dữ liệu gửi lên (Request)

- **Tham số trên đường dẫn (Path)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `key` | string | Có | Mã cấu hình (vd. `Email.Host`). Nếu có ký tự đặc biệt trên đường dẫn thì cần mã hóa URL. |

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là **một** cấu hình (cùng cấu trúc mục 4).

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": {
        "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ab",
        "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "group": "Email",
        "key": "Email.Host",
        "name": "Host",
        "value": "smtp.gmail.com",
        "type": "Text",
        "options": null,
        "description": null,
        "isReadOnly": false
    }
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Không có cấu hình với mã này của khách hàng hiện tại (`98101`).
    - **500** — Lỗi hệ thống.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98101",
    "message": "Setting key was not found for the current tenant.",
    "errors": [
        {
            "fieldName": "key",
            "message": "Email.Unknown was not found."
        }
    ]
}
```

---

## 7. API tạo một cấu hình theo mã

Tạo mới **một** cấu hình từ danh mục có sẵn.  
Nếu không gửi `value` thì dùng giá trị mặc định của danh mục.  
Nếu mã đã tồn tại với khách hàng hiện tại → báo lỗi trùng.

- **Phương thức:** `POST`
- **Đường dẫn:** `/api/v1/settings/{key}`

### Dữ liệu gửi lên (Request)

- **Tham số trên đường dẫn (Path)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `key` | string | Có | Mã cần tạo (phải có trong danh mục có sẵn). |

- **Headers**
    - `Content-Type`: `application/json`
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Body (JSON)** — nên luôn gửi object:

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `value` | string | Không | Giá trị cần lưu; bỏ trống thì lấy giá trị mặc định từ danh mục. |

```json
{
    "value": "smtp.gmail.com"
}
```

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK` (một số môi trường có thể dùng `201 Created` khi tạo mới).

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là cấu hình vừa tạo.

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": {
        "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ab",
        "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "group": "Email",
        "key": "Email.Host",
        "name": "Host",
        "value": "smtp.gmail.com",
        "type": "Text",
        "options": null,
        "description": null,
        "isReadOnly": false
    }
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **400** — JSON sai cấu trúc.
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Mã không có trong danh mục có sẵn (`98102`).
    - **409** — Mã đã tồn tại với khách hàng hiện tại (`98103`).
    - **500** — Lỗi hệ thống / khách hàng / mã hóa.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98103",
    "message": "Setting key already exists for the current tenant.",
    "errors": [
        {
            "fieldName": "key",
            "message": "Email.Host already exists."
        }
    ]
}
```

---

## 8. API cập nhật một cấu hình theo mã

Cập nhật giá trị của **một** mã đã tồn tại.  
Không cho phép nếu ô được đánh dấu **chỉ đọc**.

- **Phương thức:** `PUT`
- **Đường dẫn:** `/api/v1/settings/{key}`

### Dữ liệu gửi lên (Request)

- **Tham số trên đường dẫn (Path)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `key` | string | Có | Mã cần cập nhật. |

- **Headers**
    - `Content-Type`: `application/json`
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Body (JSON)**

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|--------|
| `value` | string | Có | Giá trị mới (plaintext). Password/encrypted: rỗng/whitespace → `98107`. |

```json
{
    "value": "smtp.office365.com"
}
```

Password/encrypted **không** chấp nhận `""`. Với setting thông thường, chuỗi rỗng vẫn là giá trị cập nhật hợp lệ.

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `200 OK`

- **Body:** theo [khung phản hồi chung](#response-body-envelope). Phần `data` là cấu hình sau khi cập nhật.

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": {
        "id": "01936f3a-8c2e-7b1a-9d4f-1234567890ab",
        "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "group": "Email",
        "key": "Email.Host",
        "name": "Host",
        "value": "smtp.office365.com",
        "type": "Text",
        "options": null,
        "description": null,
        "isReadOnly": false
    }
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **400** — JSON sai, thiếu `value`, hoặc secret rỗng (`98107`).
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Không tìm thấy mã (`98101`).
    - **422** — Ô chỉ đọc (`98104`).
    - **500** — Lỗi hệ thống.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98104",
    "message": "Setting is read-only and cannot be modified.",
    "errors": [
        {
            "fieldName": "key",
            "message": "Read-only setting cannot be updated."
        }
    ]
}
```

---

## 9. API xóa một cấu hình theo mã

Xóa **một** cấu hình đã lưu theo mã.  
Không cho phép nếu ô được đánh dấu **chỉ đọc**.

- **Phương thức:** `DELETE`
- **Đường dẫn:** `/api/v1/settings/{key}`

### Dữ liệu gửi lên (Request)

- **Tham số trên đường dẫn (Path)**

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|--------|
| `key` | string | Có | Mã cần xóa. |

- **Headers**
    - `Authorization`: `Bearer {access_token}` — **tuỳ host**
    - `RequestId`: `{uuid}` — mã theo dõi request

- **Body:** không có.

### Dữ liệu trả về (Response)

- **Headers:**
    - `RequestId`: trả lại mã đã gửi (nếu hợp lệ), hoặc mã do hệ thống gán.
    - `TraceId`: `{uuid}` — mã theo dõi nội bộ.

### Khi thành công

- **Mã HTTP:** `204 No Content` — xóa thành công, thường **không có nội dung** trong body.  
  (Một số môi trường có thể trả `200` kèm khung phản hồi với `data = null`.)

Ví dụ nếu có body:

```json
{
    "code": "0000",
    "message": "Success",
    "errors": [],
    "data": null
}
```

### Khi lỗi

- **Headers:** giống khi thành công.
- **Mã HTTP (ví dụ):**
    - **401** / **403** — Chỉ khi host đã gắn Auth/policy.
    - **404** — Không tìm thấy mã (`98101`).
    - **422** — Ô chỉ đọc (`98104`).
    - **500** — Lỗi hệ thống.

- **Body:** theo [khung phản hồi chung](#response-body-envelope) dạng lỗi.

```json
{
    "code": "98104",
    "message": "Setting is read-only and cannot be modified.",
    "errors": [
        {
            "fieldName": "key",
            "message": "Read-only setting cannot be deleted."
        }
    ]
}
```

---

## 10. Gợi ý kiểm thử / kịch bản màn hình cấu hình

### Luồng chính trên giao diện (khuyến nghị)

```text
1. GET /api/v1/settings/groups
   → Hiển thị menu / tab các nhóm cấu hình

2. GET /api/v1/settings/form?group=Email
   → Nhận danh sách ô: nhãn, kiểu, lựa chọn, giá trị hiện tại, chỉ đọc hay không…

3. Vẽ form theo type / options
   → Combobox: tách chuỗi "mã:nhãn|mã:nhãn|…" thành danh sách chọn

4. Người dùng sửa → PUT /api/v1/settings/group/Email
   Body: { "values": { "Email.Host": "...", "Email.Port": "587", ... } }
```

### Cách đọc danh sách chọn (Combobox)

Chuỗi dạng `mã:nhãn|mã:nhãn|...` — phần **mã** là giá trị lưu/gửi API; phần **nhãn** chỉ để hiển thị:

| Ví dụ | Ý nghĩa |
|-------|---------|
| `true:Yes\|false:No` | Bật / tắt |
| `smtp:Custom SMTP\|gmail:Gmail\|ses:Amazon SES` | Chọn nhà cung cấp |
| `1:Có\|0:Không` | Mã số tùy ý |

### Lưu ý bảo mật khi kiểm thử

- API **luôn nhận và trả giá trị đọc được** (kể cả mật khẩu).
- Việc mã hóa khi lưu vào cơ sở dữ liệu do hệ thống xử lý phía sau — Tester không cần (và thường không) thấy chuỗi mã hóa trên response.
- Module **không** bắt đăng nhập: chưa Auth vẫn gọi được API. Host có thể tự gắn Auth (khi đó mới có `401`). Cần đúng ngữ cảnh khách hàng (tenant) trước khi đọc/ghi giá trị.

### Gợi ý case kiểm thử nhanh

| Case | Kỳ vọng |
|------|---------|
| Mở form nhóm Email lần đầu (chưa lưu) | `GET /form` trả đủ ô; một số `isPersisted = false`, `value` = mặc định |
| Lưu cả form | `PUT /group/Email` thành công; gọi lại `/form` thấy giá trị mới, `isPersisted = true` |
| Lưu form bỏ key password (không đổi secret) | Secret cũ giữ nguyên (partial upsert) |
| Lưu / cập nhật password bằng chuỗi rỗng | `400` / mã `98107` — không mã hóa, không ghi |
| Cập nhật setting thông thường bằng chuỗi rỗng | Giá trị được cập nhật thành chuỗi rỗng |
| Sửa ô chỉ đọc | `422` / mã `98104` |
| Tạo trùng mã | `409` / mã `98103` |
| Lấy mã chưa từng lưu | `404` / mã `98101` |
| Nhóm không tồn tại | `404` / mã `98102` |
| Chưa đăng nhập (module / Sample mặc định) | Vẫn gọi API bình thường (không `401` từ Setting) |

---

<a id="response-body-envelope"></a>

## Phụ lục: Khung phản hồi chung (body)

Mọi phản hồi JSON của API (trừ trường hợp ghi chú riêng, ví dụ `204` không có body) dùng cùng một khung ở ngoài cùng.

### Các trường ở ngoài cùng

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `code` | string | Mã kết quả nghiệp vụ. Thành công: `"0000"`. Lỗi cấu hình: `98101`–`98107`. |
| `message` | string | Thông báo ngắn (có thể rỗng khi thành công). |
| `errors` | array | Khi thành công: `[]`. Khi lỗi: danh sách chi tiết theo từng trường (xem dưới). |
| `data` | object \| array \| null | Nội dung nghiệp vụ **tuỳ từng API**. Khi lỗi: `null` hoặc không gửi. |

### Khi thành công

- `code` = `"0000"`.
- `errors` = `[]` (hoặc không gửi trường `errors`).
- `data` chứa dữ liệu theo từng API.

### Khi lỗi

- `code` khác `"0000"`.
- `message` mô tả ngắn gọn.
- `errors` liệt kê lỗi theo từng trường (có thể rỗng nếu chỉ dùng `message`).
- `data` = `null` hoặc không gửi.

**Mỗi phần tử trong `errors`:**

| Trường | Kiểu | Mô tả |
|--------|------|--------|
| `fieldName` | string | Tên trường / tham số liên quan (vd. `key`, `group`, `values.Email.Host`). |
| `message` | string | Mô tả lỗi của trường đó. |

> Một số môi trường có thể bổ sung thêm mã theo dõi kỹ thuật (vd. `traceId`). Khi kiểm thử nghiệp vụ, ưu tiên đối chiếu `code`, `message`, `errors`, `data`.

---

<a id="setting-error-codes"></a>

## Phụ lục: Mã lỗi

| Mã | Ý nghĩa dễ hiểu | Thường gặp khi |
|----|-----------------|----------------|
| `98101` | Không tìm thấy cấu hình theo mã với khách hàng hiện tại | Xem / sửa / xóa theo mã (`/{key}`) |
| `98102` | Mã hoặc nhóm không có trong danh mục cấu hình có sẵn | Tạo mới / lưu form / mở form nhóm lạ |
| `98103` | Mã cấu hình đã tồn tại | Tạo mới (`POST /{key}`) khi đã có sẵn |
| `98104` | Cấu hình chỉ đọc — không được sửa/xóa | Sửa / xóa / lưu form có ô chỉ đọc |
| `98105` | Hệ thống chưa xác định được khách hàng đang thao tác | Các API đọc/ghi giá trị đã lưu |
| `98106` | Khóa mã hóa thiếu hoặc sai cấu hình | Đọc/ghi ô mật khẩu / dữ liệu bảo mật |
| `98107` | Giá trị không hợp lệ theo kiểu / secret rỗng | Validate Type; lưu Password/encrypted với chuỗi rỗng |
