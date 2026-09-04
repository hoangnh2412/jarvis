---
name: tabler-ui-kit
description: >-
  Guides applying the Tabler theme and UI kit (@tabler/core) to applications:
  new projects and existing codebases (migration, reskin, new modules). Covers
  npm/CDN, layout shell, CSS/JS bundles, SCSS overrides, icons, theme toggles,
  and removing duplicate Bootstrap. Use when adopting Tabler on greenfield or
  brownfield apps, planning UI consistency, or generating Tabler-styled UI from a
  brief—without cloning the Tabler monorepo. Invoke in Cursor as @tabler-ui-kit.
  Vietnamese: theme Tabler cho dự án mới và cũ; skill tabler-ui-kit.
---

# Theme Tabler cho dự án (mới & hiện có)

> Hướng dẫn sử dụng skill trong Cursor: [README.md](README.md)

**Mục đích:** áp dụng **theme / UI kit Tabler** qua **`@tabler/core`** cho **dự án mới** lẫn **dự án đang vận hành** (tái skin, module mới, dần thay layout cũ). **Không** mô tả monorepo phát triển Tabler (Eleventy).

## Không cần clone monorepo — vẫn đúng format

Repo **https://github.com/tabler/tabler** chỉ cần khi **sửa core** hoặc **copy nguyên** Liquid preview. Mọi dự án tiêu thụ Tabler (mới hay cũ) chỉ cần:

| Nguồn | Việc dùng |
|--------|-----------|
| **https://docs.tabler.io** | Markup, component, utilities, theme, a11y — **ưu tiên đối chiếu khi sinh code** |
| **https://preview.tabler.io** | HTML thật: DevTools / View Source (không phụ thuộc Liquid) |
| **`npm i @tabler/core`** | `node_modules/.../dist/` CSS+JS build; `scss/` để override biến |
| **CDN (ghim `<version>`)** | `https://cdn.jsdelivr.net/npm/@tabler/core@<version>/dist/` — xem cây file như package |

**Copy skill:** mang `.cursor/skills/tabler-ui-kit/` sang repo app — **không** cần kèm source Tabler.

**Agent (đúng format):** class chỉ từ **docs** hoặc **Bootstrap 5** đã Tabler style; **không** thêm Bootstrap CSS/JS tách riêng (chi tiết mục *Nguyên tắc kiến trúc*). Màu/theme: tra docs *Theme / SCSS* + `node_modules/@tabler/core/scss/_config.scss` & partial `ui/`, override **trước** `@import "@tabler/core/scss/tabler"`. **Đăng nhập / màn tập trung:** `.page.page-center`. **App có sidebar:** bám demo preview/docs — `navbar`, `navbar-vertical`, `page`, `page-wrapper` (đúng tên class, không tự đặt).

**Khung chuẩn** (bỏ navbar/sidebar nếu màn đơn):

```text
body → [navbar/sidebar theo demo] → .page (flex column, min-height)
  → .page-wrapper → [.page-header] → .page-body [.page-body-card]
  → .container-xl | .container-fluid → .row > .col-*
```

- Tiêu đề: `.page-header`, `.page-pretitle`, `.page-title` (+ biến thể border — xem docs).
- Lưới/spacing: `container-xl`, `row`, `col-*`, `g-*`, `mb-*`, `d-flex`, …
- Khối: `card` / `card-body`, `table-responsive`, form controls Tabler/Bootstrap.

**Theme:** biến **`--tblr-…`** + map Bootstrap; **sâu** = SCSS trước import `tabler`; **nhanh** = `:root` / `html[data-bs-theme="dark"]` (theo docs). **Toggle sáng/tối:** `tabler-theme.min.js` **sớm** (thường ngay sau mở `body`).

**Icon & asset:** `@tabler/icons` (SVG); **không** Liquid `{% include %}` của repo Tabler. Ảnh demo trên preview **không** có trong npm → dùng asset/placeholder dự án.

## Khi nào dùng & sinh code trong Cursor

- **Dùng khi:** **dự án mới** (chọn stack, shell từ đầu) **hoặc dự án cũ** (thay Bootstrap/custom CSS trùng lặp, bọc trang hiện có bằng `.page` / `card`, thêm màn Tabler song song code legacy, đồng bộ theme sáng/tối).
- **Chung:** MPA/SPA/SSR; CDN vs npm; CSS build vs SCSS; bundle JS; shell `page-wrapper` + navbar/sidebar; gỡ **Bootstrap CSS/JS thừa** nếu đã có trước khi thêm Tabler; tránh thiếu Popper / sai thứ tự script.
- **Sinh code:** skill = **ngữ cảnh** (không phải tool tự chạy). Gõ `@tabler-ui-kit` + brief (càng rõ stack/ràng buộc càng tốt); Cursor có thể auto-load khi mô tả khớp `description`. Agent map brief → class/component Tabler-Bootstrap (vd. `card`, `btn-primary`, `navbar`), theme (CSS variables hoặc SCSS), **một** bundle JS đúng; tránh double Bootstrap.

**Mẫu brief (dán vào chat):**

```text
@tabler-ui-kit
## Dự án: [ ] mới (greenfield)  [ ] hiện có — mô tả stack/CSS hiện tại: ___
## Stack: [ ] HTML tĩnh  [ ] Vite+React  [ ] Vue  [ ] Laravel Blade  [ ] khác: ___
## Trang: [ ] Dashboard  [ ] Đăng nhập  [ ] Danh sách+lọc  [ ] Form chi tiết  |  khác: ___
## Shell: sidebar [có/không, trái/phải, thu gọn mobile]; topbar [có/không: logo, user, notif]; footer [có/không]
## Màu & theme: sáng | tối | cả hai+toggle; primary (#…); danger/success…; [utility only | SCSS $primary+build]
## Nội dung từng trang: (cards / bảng / field form…)
## Output: đường file/route…; [chỉ markup | + SCSS entry | + tabler-theme]
```

### Giới hạn thực tế

- **Không** cam kết “giống pixel-perfect” một trang trên preview.tabler.io nếu bạn không gửi link/screenshot hoặc markup mẫu.
- Màu sắc: với tùy biến sâu, agent nên sinh **SCSS override** hoặc `:root` theo biến Tabler; luôn **ghim version** `@tabler/core` khi copy vào dự án thật.
- Nếu dự án nằm **ngoài** workspace hiện tại, nói rõ đường dẫn hoặc nhờ agent chỉ xuất patch/snippet để bạn dán.

## Nguyên tắc kiến trúc

- **`@tabler/core` đã nhúng bản Bootstrap mà Tabler quản lý** trong một file CSS (ví dụ `tabler.min.css`). **Không** thêm thêm file `bootstrap.min.css` song song — dễ trùng lệnh và lệch phiên bản.
- Tabler là **UI tĩnh + JS tương tác** (dropdown, modal, …). “System design” của bạn = **routing, state, API, auth** do framework/backend của dự án quyết định; Tabler chỉ bọc presentation.
- **Hai lớp tùy biến:**
  - **Nhanh:** chỉnh class Bootstrap/Tabler + biến CSS runtime (`:root` / theme) nếu phù hợp.
  - **Sâu:** cài `sass`, import `scss/tabler.scss` từ package và override biến SCSS **trước** khi build.

## Cài đặt & phiên bản

**npm/pnpm/yarn:**

```sh
npm install @tabler/core
```

**CDN (prototype hoặc MPA đơn giản):** cố định version trong URL production thay vì `@latest`.

- CSS: `https://cdn.jsdelivr.net/npm/@tabler/core@<version>/dist/css/tabler.min.css`
- JS: `https://cdn.jsdelivr.net/npm/@tabler/core@<version>/dist/js/tabler.min.js`

**Ghim version** trên mọi kênh (lockfile, URL CDN) để tránh vỡ giao diện khi Tabler bump minor.

## HTML shell tối thiểu (MPA / template chung)

1. Trong `<head>`: một link tới **`tabler.min.css`** (hoặc file build từ SCSS của bạn thay thế).
2. Nếu dùng **theme switcher** như bản preview: script **`tabler-theme.min.js`** sớm trong `body` (xem docs Theme).
3. Trước `</body>`: **`tabler.min.js`** (thường `defer` nếu không phụ thuộc inline script chạy trước).
4. Bọc nội dung theo **khung `.page` → `.page-wrapper` → `.page-body` → `.container-xl`** (mục “Khung trang” phía trên); bổ sung navbar/sidebar theo đúng ví dụ docs/preview.

Luôn đối chiếu: **https://docs.tabler.io** và **https://preview.tabler.io** — không cần clone repo.

## JavaScript

- **UMD/browser:** `dist/js/tabler.min.js` (phổ biến cho MPA, Blade, PHP, …).
- **ESM:** `dist/js/tabler.esm.min.js` khi bundler (Vite/Webpack) import module.
- **`tabler-theme`:** `tabler-theme.js` / `.min.js` khi cần **chuyển theme / chế độ** theo cơ chế Tabler (tách với bundle chính).

Đảm bảo **một** nơi khởi tạo behavior (tránh load hai bản Tabler hoặc hai bản Bootstrap JS).

## SCSS trong dự án có pipeline build

- Entry gợi ý: file `src/styles/tabler-app.scss` của bạn:

```scss
// Override biến Tabler/Bootstrap TRƯỚC import (xem docs biến SCSS)
// $primary: ...;

@import "@tabler/core/scss/tabler";
```

- `sass` cần **load path** tới `node_modules` (tuỳ CLI/bundler). Output là **một** CSS cho app → vẫn không cần file Bootstrap riêng.

## Icon

- **Tách với core:** cài `@tabler/icons` và dùng SVG/sprite theo hướng dẫn icon package — phù hợp React/Vue/Blade.
- **Không** copy nguyên cú pháp Liquid `{% include "ui/icon.html" %}` của repo preview sang stack khác; đó là convention Eleventy nội bộ. Thay bằng component/template tương đương của framework bạn.

## CSS bổ sung (tùy nghiệp vụ)

Trong package còn có các lớp tách (ví dụ flags, payments, socials, vendors) — chỉ link khi thật sự cần để giữ bundle nhỏ.

## Gợi ý cấu trúc thư mục

Áp dụng linh hoạt (dự án mới hoặc sau khi tách layout); mục tiêu là **layout / partial / trang** rõ ràng.

```text
src/
  styles/           # SCSS app hoặc import CSS đã build
  components/         # Khối UI tái dùng (theo React/Vue/Blade/…)
  layouts/            # Shell: header, sidebar, footer
  pages/              # Hoặc routes/views
public/
  assets/             # Ảnh, font bổ sung (nếu không qua bundler)
```

## Gợi ý theo loại dự án

- **Dự án hiện có (brownfield):** kiểm tra đã load **Bootstrap hoặc CSS framework** nào — thường **gỡ** Bootstrap tách rời rồi chỉ giữ **một** lớp `tabler(.min).css`; trang chưa migrate có thể tạm giữ layout cũ, ưu tiên **layout chung** + **trang mới** theo khung `.page` trước.
- **MPA (Rails, Laravel, Django templates):** layout master + partial; CSS/JS từ Vite/Mix hoặc CDN; tránh nhân đôi Bootstrap.
- **SPA (React/Vue/Svelte):** import CSS Tabler một lần ở root layout; bọc route trong shell giống preview; icon dùng package `@tabler/icons` hoặc component SVG.
- **SSR (Next/Nuxt):** import CSS toàn cục ở layout gốc; kiểm tra FOUC/hydration; chỉ dùng `tabler-theme` nếu có toggle theme rõ ràng.

## Checklist triển khai

- [ ] Một nguồn CSS chính (Tabler), không thêm Bootstrap CSS riêng.
- [ ] Một bundle JS Tabler phù hợp (UMD hoặc ESM), thêm **tabler-theme** nếu có toggle sáng/tối.
- [ ] Version đã ghim (lockfile / CDN).
- [ ] Shell **`.page` / `.page-wrapper` / `.page-body` / `container-xl`** khớp docs hoặc preview (không tự đặt tên lạ).
- [ ] Class component đã kiểm tra trên **docs.tabler.io** (hoặc SCSS trong `node_modules/@tabler/core`).
- [ ] Accessibility cơ bản (nút icon có `aria-label`, heading hợp lý).
- [ ] Icon qua **`@tabler/icons`**; không phụ thuộc Liquid/includes của monorepo Tabler.

## Liên quan skill khác

- **`tabler-system-design`** mô tả **monorepo phát triển Tabler** (core/preview/docs/build). Dùng khi sửa **bản thân** Tabler; skill **này** dùng khi áp **theme Tabler (`@tabler/core`)** vào **product** (mới hoặc cũ).
