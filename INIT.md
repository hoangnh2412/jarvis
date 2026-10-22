# Project skeleton — Hướng dẫn maintainer

Khung khởi tạo dự án mặc định. Agent/user copy vào `{project}/` theo [minipower-router-init](../skills/minipower-router-init/SKILL.md).

## Lệnh khởi tạo

```bash
PROJECT=my-project
MINIPOWER=/path/to/minipower/src/router

mkdir -p "$PROJECT"
cp -R "$MINIPOWER/project-skeleton/"* "$PROJECT/"
cp -R "$MINIPOWER/docs-skeleton" "$PROJECT/docs"
# Entry cá nhân (gitignore) — copy khuôn
cp "$PROJECT/memory/memory.md.example" "$PROJECT/memory/memory.md"
```

## Vai trò từng thư mục

| Thư mục | Vai trò |
|---------|---------|
| `assets/` | Giữ **bản gốc** khảo sát, checklist, biên bản — không sửa file gốc |
| `brainstorm/` | Phân tích, trao đổi theo ngày; chốt → distill vào `docs/` |
| `docs/` | Tài liệu baseline (Vision, BRD, kiến trúc, traceability, CR…) |
| `memory/` | Entry cá nhân (`memory.md` gitignore) + sổ đội phẳng (DEC, open-Q, doc-debt) + SQLite |
| `FAQ.md` | FAQ hướng dẫn thiết lập sẵn (làm gì / làm thế nào / thiếu gì) |

## Nội dung skeleton (ngoài `docs/`)

| Path | Mô tả |
|------|--------|
| `README.md` | Entry dự án |
| `FAQ.md` | FAQ hướng dẫn thiết lập sẵn |
| `memory/profile.json` | *(tạo lúc init)* — cấu hình **dự án** v3 |
| `memory/profile.user.json` | *(tạo lúc init, gitignore)* — identity máy |
| `memory/profile.user.json.example` | Khuôn identity |
| `memory/memory.md.example` | Khuôn entry agent (ba khối + con trỏ) — commit |
| `memory/memory.md` | *(copy từ .example)* — cá nhân, **gitignore** (ADR-035) |
| `memory/decision-log.md` | DEC đội — **một file**, không theo phase |
| `memory/open-questions.md` | Câu hỏi / nợ tiền đề đội |
| `memory/doc-debt.md` | Sổ nợ tài liệu — điều kiện lên `standard` |
| `memory/trace.sql` | Khuôn SQLite; DB `trace.db` gitignore — việc đội khi `tasks=none` |
| `assets/public/`, `internal/` | Tài liệu thô |
| `assets/archive/` | Tài liệu cũ, rời rạc — nguồn tham chiếu, *không* phải artifact |
| `brainstorm/` | File trao đổi theo ngày — **không** chia folder con |

`docs/` — copy từ [`docs-skeleton/`](../docs-skeleton/). **Không** còn `memory/memory.md` (ADR-035).

### Migrate dự án cũ (ADR-035)

1. Gộp mọi `memory/*/decision-log.md` → `memory/decision-log.md` (giữ heading `### DEC-…`).
2. Gộp `memory/*/open-questions.md` → `memory/open-questions.md`.
3. Copy nội dung hữu ích từ `docs/05-traceability/overview.md` (nếu còn) vào `memory/memory.md` khối Hiện trạng / Nhắc việc — rồi **xoá** `overview.md`.
4. Xoá 6 folder phase dưới `memory/` và thư mục `memory/tasks/` (việc đội → `artifact` trong `trace.db`).
5. Copy `memory.md.example` → `memory.md` nếu chưa có.

## Bề mặt code (ADR-038)

`project_mode` không cắt folder tài liệu. Trục khác là **bề mặt**, ghi trong `memory/profile.json` → `surfaces`.

| id | Folder | Mặc định |
|----|--------|----------|
| `docs` | `docs/` + `assets/` + `brainstorm/` + README/FAQ | bật |
| `backend` | `backend/` | tắt |
| `frontend` | `frontend/` | tắt |
| `mobile` | `mobile/` | tắt |
| `autotest` | `autotest/` | tắt |

Init copy cây của bề mặt được chọn (`backend/src|tests|build`, `frontend/src`, `mobile/src`, `autotest/src`). Không ghi `AGENTS.md` — in prompt để người dán vào chat. Không sinh `.sln` hay framework. Agent đặt file mới trong cây đó.

`.minipower/` và `memory/` luôn có, kể cả khi bỏ `docs`.

## Copy đủ khung ở MỌI chế độ

Init copy **nguyên** skeleton bất kể `project_mode` là `mvp`, `standard` hay `maintain`. **Không** cắt folder theo chế độ.

Lý do: `mvp`/`maintain` rồi sẽ phải bổ sung tài liệu. Cắt folder hôm nay là tạo việc di trú ngày mai. Giữ nguyên khung thì lên `standard` chỉ là **điền tiếp**.

Hệ quả: ở `mvp`/`maintain` sẽ có folder `docs/` rỗng có chủ đích — nói ra bằng README.

### README chuẩn cho folder chưa dùng

Agent đặt file `README.md` với nội dung sau vào **mỗi** folder `docs/` nằm ngoài `docs_focus` của chế độ hiện tại:

```markdown
# (chưa điền)

Folder này **cố ý để trống** vì dự án đang ở chế độ **`{mode}`** — `docs_focus` của chế độ
này chưa gồm {DOC-NN…}.

Không phải lỗi cài đặt, và **không** xoá folder: khung giữ nguyên để khi lên `standard`
chỉ cần điền tiếp, không phải di trú cấu trúc.

Món nợ tương ứng ghi ở [`memory/doc-debt.md`](../../memory/doc-debt.md).
```

Thay `{mode}` và `{DOC-NN…}` bằng giá trị thật. Bảng `docs_focus`: [SKILL.md § Chế độ dự án](../SKILL.md#chế-độ-dự-án-project_mode).
