# Account

Module xác thực và quản lý tài khoản: **Login**, **Register**, **Forgot password**, **Profile**, **Change password**.

---

## Khi nào dùng

- App cần trang auth có sẵn UI (PrimeReact + kit theme).
- Host app tự quyết định lưu token (localStorage, cookie…) qua `callback.onSubmit`.

---

## Export chính

```ts
import {
  LoginPage,
  RegisterPage,
  ForgotPasswordPage,
  AccountProfilePage,
  ChangePasswordPage,
  ACCOUNT_ROUTES,
  callLogin,
  callGetCurrentUser,
  accountHttp,
  getCurrentUserMock,
} from '@platform/core'
```

---

## CSS

```css
@import "@platform/core/styles.css";
```

Không cần CSS riêng cho account.

---

## Tích hợp routes

```tsx
import { Routes, Route, Navigate } from 'react-router-dom'
import {
  LoginPage,
  RegisterPage,
  ForgotPasswordPage,
  AccountProfilePage,
  ChangePasswordPage,
  ACCOUNT_ROUTES,
  callLogin,
} from '@platform/core'

<Routes>
  <Route path={ACCOUNT_ROUTES.login} element={<LoginPage callback={{ onSubmit: handleLogin }} />} />
  <Route path={ACCOUNT_ROUTES.register} element={<RegisterPage />} />
  <Route path={ACCOUNT_ROUTES.forgotPassword} element={<ForgotPasswordPage />} />
  <Route path="/profile" element={<AccountProfilePage />} />
  <Route path="/change-password" element={<ChangePasswordPage />} />
</Routes>
```

`ACCOUNT_ROUTES`: `login`, `register`, `forgotPassword`, …

---

## Login — API HTTP thật (khuyến nghị)

Page mặc định gọi `callLogin` qua axios. Sample gắn JWT interceptor và parse token:

```tsx
import { callLogin } from '@platform/core'
import { setAccessToken } from './auth'
import { extractLoginResult } from './auth/apiHelpers'

<LoginPage
  callback={{
    onSubmit: async (payload) => {
      const response = await callLogin(payload)
      const result = extractLoginResult(response)
      const token = result.tokens?.accessToken
      if (!token) throw new Error('Server không trả access token.')
      setAccessToken(token)
      return result
    },
  }}
/>
```

Interceptor Bearer token — **host app** (`Sample/clients/web/src/auth/setupAccountAuth.ts`):

```ts
import { accountHttp } from '@platform/core'

accountHttp.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
```

Gọi `setupSampleAccountAuth()` trong `main.tsx` cùng `configureSampleHttp()`.

Cấu hình env + proxy: xem [api-integration-and-usage.md](../../../docs/api-integration-and-usage.md).

---

## Login mock (Sample — phương án thay thế)

Demo không cần BE — tài khoản cố định trong `Sample/clients/web/src/constants/index.ts`:

- Email: `admin@gmail.com` / Password: `Admin@123`

```tsx
import { callLogin } from '@platform/core'
import { mockLogin, isMockAccountCredentials } from './constants'

<LoginPage
  callback={{
    onSubmit: async (payload) => {
      if (isMockAccountCredentials(payload)) {
        return mockLogin(payload) // không gọi HTTP
      }
      const res = await callLogin(payload)
      return res.data
    },
  }}
/>
```

User thật vẫn đi qua `callLogin` → BE.

---

## API endpoints


| Service              | Method | Path                         |
| -------------------- | ------ | ---------------------------- |
| `callLogin`          | POST   | `v1/account/login`           |
| `callRegister`       | POST   | `v1/account/register`        |
| `callGetCurrentUser` | GET    | `v1/account/current`         |
| `callUpdateProfile`  | PUT    | `v1/account/profile`         |
| `callUpdatePassword` | PUT    | `v1/account/password`        |
| `callForgotPassword` | POST   | `v1/account/forgot-password` |
| `callLogout`         | POST   | `v1/account/logout`          |


Mock JSON: `features/account/mocks/get-current-user.json`.

---

## Tùy biến

Host app **không cần fork kit** — dùng `callback` (logic) và `content` (giao diện).

### Pattern `callback` (ActionProps)

Luồng: `before` → `onSubmit` → side-effect page → `success` → `complete`. Return `false` ở `before` để hủy.


| Page               | `callback`        | Mặc định `onSubmit`  |
| ------------------ | ----------------- | -------------------- |
| LoginPage          | `callback` (root) | `callLogin`          |
| RegisterPage       | `callback`        | `callRegister`       |
| ForgotPasswordPage | `callback`        | `callForgotPassword` |
| AccountProfilePage | `callback`        | `callUpdateProfile`  |
| ChangePasswordPage | `callback`        | `callUpdatePassword` |


```tsx
<RegisterPage
  callback={{
    before: async () => { if (!termsAccepted) return false },
    onSubmit: async (data) => {
      const res = await callRegister(data)
      return res.data
    },
    success: () => analytics.track('registered'),
  }}
/>
```

### Tùy biến giao diện — `content`

`content` là `ReactNode` hoặc `(ctx) => ReactNode`. Context export: `LoginPageContentContext`, `AccountProfilePageContentContext`, …

```tsx
<LoginPage
  logo={<MyLogo />}
  title="Đăng nhập"
  submitLabel="Vào hệ thống"
  showRegisterLink={false}
  content={(ctx) => (
    <form onSubmit={ctx.submit}>
      {/* ctx.register, ctx.control, ctx.errors — react-hook-form */}
      <MyEmailField register={ctx.register} errors={ctx.errors} />
      <MyPasswordField control={ctx.control} errors={ctx.errors} />
      <button type="submit" disabled={ctx.isSubmitting}>Đăng nhập</button>
    </form>
  )}
/>
```

Dùng form kit mặc định: `content={(ctx) => ctx.DefaultContent}`.

`AccountProfilePage` — layout 2 cột:

```tsx
<AccountProfilePage
  left={<MyAvatarCard />}
  headerActions={<Button label="Xóa tài khoản" />}
  content={(ctx) => ctx.DefaultContent}
/>
```


| Prop                              | Tác dụng                                   |
| --------------------------------- | ------------------------------------------ |
| `withShell={false}`               | Bỏ `AuthShell` / layout page — host tự bọc |
| `onForgotClick`, `forgotHref`     | Tuỳ chỉnh link quên mật khẩu               |
| `onRegisterClick`, `registerHref` | Tuỳ chỉnh link đăng ký                     |
| `defaultValues`                   | Prefill form                               |


Context thường dùng: `register`, `control`, `errors`, `submit`, `isSubmitting`, `DefaultContent` (Login); `DefaultLeft` (Profile).

---

## Props dự án khác phải custom

Host gắn logic auth/token/điều hướng qua các prop hàm dưới đây. Copy UI (`title`, `logo`, …) chỉ tùy chọn.

Mỗi `callback` dùng `ActionProps`: `before` → `onSubmit` → (side-effect page) → `success` → `complete`; lỗi: `error` → `complete`. `before` return `false` để hủy.

### `LoginPage` — props phải custom


| Prop                | Ý nghĩa dễ hiểu                   | Ví dụ hàm truyền vào                                                                    | Khi truyền vào thì làm gì                                                                |
| ------------------- | --------------------------------- | --------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `callback.onSubmit` | Hàm đăng nhập thật của app        | `async (data) => { const r = await callLogin(data); setAccessToken(…); return r.data }` | User bấm Đăng nhập → page gọi hàm này thay API mặc định; bạn lưu token / session tại đây |
| `callback.before`   | Chặn submit nếu chưa đủ điều kiện | `({ data }) => data.email.includes('@') ? undefined : false`                            | Chạy trước API; `false` = không gọi login                                                |
| `callback.success`  | Side-effect sau login OK          | `({ result }) => navigate('/dashboard')`                                                | Sau khi `onSubmit` thành công + page xử lý xong                                          |
| `callback.error`    | Báo lỗi login                     | `({ error }) => notify.error(getErrorMessage(error))`                                   | Khi `onSubmit` throw / reject                                                            |
| `onForgotClick`     | Click “Quên mật khẩu”             | `() => navigate('/auth/forgot')`                                                        | Thay navigate mặc định theo `forgotHref`                                                 |
| `onRegisterClick`   | Click “Đăng ký”                   | `() => openRegisterDrawer()`                                                            | Thay link đăng ký mặc định                                                               |


```tsx
async function handleLogin(data: LoginFormData) {
  // Gọi API login → lấy token → lưu storage host
  const response = await callLogin(data)
  const token = response.data.tokens?.accessToken
  if (!token) throw new Error('Thiếu access token')
  setAccessToken(token)
  return response.data
}

function afterLoginSuccess() {
  // Điều hướng vào app sau khi đã có token
  navigate('/dashboard')
}

<LoginPage
  callback={{
    onSubmit: handleLogin,
    success: afterLoginSuccess,
    error: ({ error }) => notify.error(getErrorMessage(error)),
  }}
  onForgotClick={() => navigate('/auth/forgot')}
/>
```

### `RegisterPage` — props phải custom


| Prop                | Ý nghĩa                 | Ví dụ hàm                                         | Khi truyền vào                   |
| ------------------- | ----------------------- | ------------------------------------------------- | -------------------------------- |
| `callback.onSubmit` | Gửi form đăng ký lên BE | `async (data) => (await callRegister(data)).data` | Bấm Đăng ký → tạo tài khoản      |
| `callback.success`  | Sau đăng ký OK          | `() => navigate('/login')`                        | Chuyển về login / hiện toast     |
| `onLoginClick`      | Click về trang login    | `() => navigate('/login')`                        | Override link “Đã có tài khoản?” |


```tsx
async function handleRegister(data: RegisterFormData) {
  const res = await callRegister(data)
  return res.data
}

<RegisterPage
  callback={{
    onSubmit: handleRegister,
    success: () => {
      analytics.track('registered')
      navigate('/login')
    },
  }}
/>
```

### `ForgotPasswordPage` — props phải custom


| Prop                | Ý nghĩa           | Ví dụ hàm                                  | Khi truyền vào                      |
| ------------------- | ----------------- | ------------------------------------------ | ----------------------------------- |
| `callback.onSubmit` | Gửi email quên MK | `async (data) => callForgotPassword(data)` | User nhập email → BE gửi mail reset |
| `callback.success`  | Thông báo đã gửi  | `() => notify.info('Kiểm tra hộp thư')`    | Sau khi API OK                      |


```tsx
async function handleForgotPassword(data: ForgotPasswordFormData) {
  await callForgotPassword(data)
}

<ForgotPasswordPage
  successMessage="Kiểm tra hộp thư của bạn"
  callback={{
    onSubmit: handleForgotPassword,
    success: () => notify.info('Đã gửi email'),
  }}
/>
```

### `AccountProfilePage` — props phải custom


| Prop                | Ý nghĩa                  | Ví dụ hàm                                              | Khi truyền vào                     |
| ------------------- | ------------------------ | ------------------------------------------------------ | ---------------------------------- |
| `callback.onSubmit` | Lưu hồ sơ lên BE         | `async (data) => (await callUpdateProfile(data)).data` | Bấm Lưu → cập nhật profile         |
| `onCancel`          | Nút Hủy                  | `() => navigate(-1)`                                   | Thoát form không lưu               |
| `defaultValues`     | Prefill từ user hiện tại | `{ fullName: user.name, email: user.email }`           | Không phải hàm — truyền object sẵn |


```tsx
async function handleUpdateProfile(data: ProfileFormData) {
  const res = await callUpdateProfile(data)
  return res.data
}

<AccountProfilePage
  defaultValues={{ fullName: user.name, email: user.email }}
  onCancel={() => navigate(-1)}
  callback={{
    onSubmit: handleUpdateProfile,
    success: () => notify.success('Đã cập nhật hồ sơ'),
  }}
/>
```

### `ChangePasswordPage` — props phải custom


| Prop                | Ý nghĩa                       | Ví dụ hàm                                                                     | Khi truyền vào              |
| ------------------- | ----------------------------- | ----------------------------------------------------------------------------- | --------------------------- |
| `callback.before`   | Check confirm password        | `({ data }) => data.newPassword === data.confirmPassword ? undefined : false` | Sai confirm → không gọi API |
| `callback.onSubmit` | Đổi mật khẩu trên BE          | `async (data) => callUpdatePassword(data)`                                    | Bấm Đổi mật khẩu            |
| `callback.success`  | Toast / logout buộc login lại | `() => { notify.success('OK'); logout() }`                                    | Sau khi đổi MK thành công   |


```tsx
async function handleChangePassword(data: ChangePasswordFormData) {
  await callUpdatePassword(data)
}

<ChangePasswordPage
  callback={{
    before: ({ data }) =>
      data.newPassword === data.confirmPassword ? undefined : false,
    onSubmit: handleChangePassword,
    success: () => notify.success('Đã đổi mật khẩu'),
  }}
/>
```

### Props UI tùy chọn (không bắt buộc)

`title`, `description`, `submitLabel`, `logo`, `forgotHref`, `registerHref`, `showForgotLink`, `showRegisterLink`, `content`, `withShell`, `left`, `headerActions`, `className` — chỉ đổi giao diện / layout, không ảnh hưởng API.

---

## Ghi chú

- Form validation dùng **Zod** (`loginSchema`, `profileSchema`, …).
- `AuthShell` / `AccountAuthPage` dùng cho layout trang guest.
- Interceptor axios (Bearer token) cấu hình ở host app — xem `Sample/clients/web/src/auth/setupAccountAuth.ts`.

