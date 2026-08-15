# @jarvis/core

React UI-kit trên **PrimeReact 11** (Account, CRUD, common helpers). Primitives re-export từ PrimeReact — không còn folder `src/ui` custom.

## Ví dụ App.tsx

File mẫu đầy đủ (auth + `AdminLayout` + tenant routes):

[`examples/App.tsx`](./examples/App.tsx)

Copy vào `src/App.tsx` của dự án host, kèm `BrowserRouter` + `PrimeReactProvider` như comment đầu file.

### Trong monorepo / local (dev)

```bash
# từ app
npm i file:../../../frameworks/frontend
```

### Publish rồi cài từ registry

```bash
# trong frameworks/frontend
npm login
npm publish --access public

# dự án khác
npm i @jarvis/core
```

> Đổi `name` trong `package.json` (vd. `@company/ui-kit`) trước khi publish nội bộ.

## Peer dependencies (app phải có)

```bash
npm i react react-dom lucide-react zod react-hook-form @hookform/resolvers primereact @primeuix/themes
```

> **Lưu ý local link (`file:…`)**: app Vite cần `resolve.dedupe` + alias `react`/`react-dom` về `node_modules` của app, tránh 2 bản React → lỗi `Invalid hook call`.

**PrimeReactProvider + Toast** — bắt buộc ở root:

```tsx
import { PrimeReactProvider } from '@primereact/core'
import { kitPrimeReactConfig, Toaster } from '@jarvis/core'
import '@jarvis/core/styles.css'

createRoot(...).render(
  <PrimeReactProvider {...kitPrimeReactConfig}>
    <App />
    <Toaster />
  </PrimeReactProvider>,
)
```

`Button` / `InputText` dùng **props PrimeReact** (`severity`, `rounded`, `fluid`, `variant`, `size`, `invalid`, `style`, `pt`, …).

> App Vite: `resolve.dedupe` + alias `react` / `primereact` / `@primereact/core` về một `node_modules` — tránh theme inject lệch bản.

> **License:** PrimeReact 11 cần key (Community miễn phí nếu đủ điều kiện: [primeui.dev/licenses](https://primeui.dev/licenses)). Truyền `license="YOUR_KEY"` vào `PrimeReactProvider`. Kit ẩn badge `Invalid PrimeUI License` qua CSS — production nên dùng key hợp lệ.

UI primitives từ kit: `Button`, `InputText`, `InputPassword`, `Select`, `Dialog`, `Tabs`, `ToggleSwitch`, …

Thông báo: `notify.success('…')` / `notify.error('…')` (wrapper trên Prime Toast).

Confirm: `ConfirmDialog` (trên `Dialog` của PrimeReact).

App **bắt buộc** có Tailwind và **dùng cấu hình từ package**.

### Tailwind v4 (vd. `test-fe` — `@tailwindcss/vite`)

`src/index.css` (tránh full `@import "tailwindcss"` — preflight đè Aura):

```css
@layer theme, base, primereact, utilities;

@import "tailwindcss/theme.css" layer(theme);
@import "tailwindcss/utilities.css" layer(utilities);

@source "../node_modules/@jarvis/core/dist";
@import "@jarvis/core/theme.css";
@import "@jarvis/core/styles.css";
```

### Tailwind v3 (`postcss` + `tailwind.config.js`)

```js
// tailwind.config.js
module.exports = {
  presets: [require('@jarvis/core/tailwind.preset.cjs')],
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
    './node_modules/@jarvis/core/dist/**/*.{js,cjs}',
  ],
}
```

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
@import "@jarvis/core/styles.css";
```

Thiếu `@source` / `content` → class kit không generate (UI trắng).  
Thiếu `theme.css` / preset → `bg-paper`, `text-ink`, `animate-account-auth-in` không có.

Sau khi sửa kit: `npm run build` trong `frameworks/frontend`, rồi **restart** `npm run dev` ở app.

## Cách dùng

### CRUD (create / edit biểu mẫu)

Package lo **UI (ItemLeft/ItemRight) + Zod + react-hook-form**. App chỉ gọi API trong `onSubmit`.

```tsx
import { CrudPage, type TemplateFormData } from '@jarvis/core'

// Tạo mới
function CreateTemplate() {
  return (
    <CrudPage
      mode="create"
      onBack={() => navigate('/templates')}
      onCancel={() => navigate('/templates')}
      onSubmit={async (data: TemplateFormData) => {
        await api.post('/templates', data) // throw Error nếu fail
        navigate('/templates')
      }}
    />
  )
}

// Cập nhật — prefill sau khi fetch
function EditTemplate({ id }: { id: string }) {
  const { data } = useTemplate(id)

  return (
    <CrudPage
      mode="edit"
      defaultValues={data}
      onBack={() => navigate('/templates')}
      onCancel={() => navigate('/templates')}
      onSubmit={async (values) => {
        await api.put(`/templates/${id}`, values)
        navigate('/templates')
      }}
      onPreview={(values) => console.log('preview', values)}
    />
  )
}
```

Layout thuần (không form sẵn): `CrudPageShell` + `left` / `right` tùy chỉnh.

### Account (login / register / forgot / profile)

Package lo **UI + validate (Zod) + react-hook-form**. App chỉ gọi API trong `onSubmit` (và điều hướng link nếu cần).

```tsx
import {
  LoginPage,
  RegisterPage,
  ForgotPasswordPage,
  AccountProfilePage,
  type LoginFormData,
} from '@jarvis/core'

function Login() {
  return (
    <LoginPage
      onSubmit={async (data: LoginFormData) => {
        await api.post('/auth/login', data) // throw Error nếu fail
      }}
      onForgotClick={() => navigate('/forgot')}
      onRegisterClick={() => navigate('/register')}
    />
  )
}

function Register() {
  return (
    <RegisterPage
      onSubmit={async (data) => {
        await api.post('/auth/register', data)
        navigate('/login')
      }}
      onLoginClick={() => navigate('/login')}
    />
  )
}

function Forgot() {
  return (
    <ForgotPasswordPage
      onSubmit={async (data) => {
        await api.post('/auth/forgot-password', data)
      }}
      onLoginClick={() => navigate('/login')}
    />
  )
}

function Profile() {
  return (
    <AccountProfilePage
      defaultValues={{ fullName: user.name, email: user.email, phone: user.phone }}
      onSubmit={async (data) => {
        await api.put('/me', data)
      }}
      onCancel={() => navigate(-1)}
    />
  )
}
```

Nếu API lỗi: `throw new Error('Sai email hoặc mật khẩu')` — package hiện Alert.

## Build package

```bash
cd frameworks/frontend
npm install
npm run build
```

Output: `dist/` (ESM + CJS + `.d.ts`)

## Craft PDF

Module thiết kế template PDF (CraftMyPDF-style): list templates + editor kéo-thả (`react-rnd`) + preview (`react-pdf`).

```tsx
import {
  CraftPdfTemplateListPage,
  CraftPdfEditorPage,
  configureCraftPdfNavigate,
  CRAFT_PDF_ROUTES,
} from '@jarvis/core'

// Trong router app
configureCraftPdfNavigate((to) => navigate(to))

<Route path={CRAFT_PDF_ROUTES.list} element={<CraftPdfTemplateListPage />} />
<Route
  path={CRAFT_PDF_ROUTES.editor}
  element={<CraftPdfEditorPage templateId={id} />}
/>
```

Peer thêm: `react-rnd`, `react-pdf`, `pdfjs-dist`, `quill`.  
Chưa set `VITE_API_URL_CRAFT_PDF` → dùng mock in-memory. Generate PDF nối backend qua `callback.generate`.

### Vite: `process is not defined` (react-rnd)

`react-draggable` đọc `process.env.DRAGGABLE_DEBUG`. Kit đã polyfill khi load canvas; nếu host vẫn lỗi, thêm vào `vite.config.ts`:

```ts
export default defineConfig({
  define: {
    'process.env.DRAGGABLE_DEBUG': 'undefined',
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV ?? 'development'),
  },
})
```

Rồi xóa cache Vite: xóa `node_modules/.vite` và restart `npm run dev`.

## Dashboard

Canvas full-page dùng **GridStack** (như [gridstackjs.com](https://gridstackjs.com/) demo): kéo / resize trên lưới 12 cột. Chart.js vẽ trong từng cell. Backend chỉ trả catalog (data + `settingsForm`); layout lưu FE (`localStorage` v2).

```tsx
import { DashboardPage, DASHBOARD_ROUTES } from '@jarvis/core'
import 'gridstack/dist/gridstack.css'
import '@jarvis/core/styles.css'

<Route path={DASHBOARD_ROUTES.home} element={<DashboardPage />} />
```

Peer thêm: `gridstack`, `chart.js`, `react-chartjs-2`.  
Chưa set `VITE_API_URL_DASHBOARD` → 4 chart fake. Min cell `2×2`, default `4×4`.

## Roadmap

- [x] PrimeReact re-exports + ConfirmDialog / Loading / Toaster / FormActions / PageHeader
- [x] CrudPage (biểu mẫu + validate) + CrudPageShell
- [x] Login / Register / Forgot / Profile pages
- [x] CraftPdf (template list + drag-drop editor + preview)
- [x] Dashboard (GridStack canvas + Chart.js catalog)
- [ ] Generic CrudList (config-driven)
