# @platform/setting

UI feature Setting — tổ chức giống `@platform/core` (`pages` / `components` / `services` / …).

Host app (vd. `Sample/clients/web`) import và gắn route, không clone UI kit vào đây.

```tsx
import { SettingPage, TestEmailPage, SETTING_ROUTES } from '@platform/setting'
```

## Cấu trúc

```
src/
  index.ts
  features/settings/
    pages/
    components/
    services/
    types/
    validation/
    utils/
    routes/
    menu/
```
