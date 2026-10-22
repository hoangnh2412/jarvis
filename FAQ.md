# FAQ — Hướng dẫn thiết lập sẵn

Bộ **câu hỏi–trả lời thiết lập sẵn** cho người dùng khi:

- Không biết **nên làm gì** hoặc **làm gì tiếp theo**
- Không biết **làm thế nào** (workflow, phase, skill, lệnh)
- Không biết **cần cung cấp thêm thông tin gì** cho AI

**Agent:** khi user hỏi dạng meta / hướng dẫn / "bước tiếp theo" → đọc file này trước, trả lời theo mục khớp; không bịa nếu chưa có mục tương ứng.

## Không phải

| File | Vai trò |
|------|---------|
| **`FAQ.md`** (file này) | FAQ **cố định** do maintainer/PM thiết lập |
| `memory/open-questions.md` | Câu hỏi dự án **chưa trả lời** — sổ nợ tiền đề đội |
| `memory/memory.md` | Sổ **cá nhân** (gitignore) — hiện trạng tôi + nhắc việc |
| `brainstorm/` | Ghi chú phiên làm việc, phân tích thô |
| `docs/` | Artifact chính thức của dự án |

**Không** ghi Q&A elicitation với khách hàng, assumption, hay nội dung chưa chốt vào file này.

---

## Cách thêm mục (maintainer)

Mỗi mục một cặp **H / Đ** — câu hỏi đại diện (user có thể hỏi khác lời) và câu trả lời chuẩn.

```markdown
### H: *(câu hỏi mẫu — user có thể diễn đạt khác)*
**Đ:** *(hướng dẫn / bước tiếp / thông tin cần cung cấp)*
```

---

## Danh sách

### H: Trong quá trình discovery, brainstorm, architecture tôi cần tạo file để lưu trữ tạm thời. Nếu chưa rõ lưu vào đâu thì làm thế nào?

**Đ:** **Mặc định:** ghi vào `brainstorm/YYYY-MM-DD.md` hoặc `YYYY-MM-DD-<mo-ta>.md`.

```text
assets/  →  brainstorm/  →  docs/  →  02-baseline/
 (thô)      (làm việc)      (chính thức)   (đã ký)
```

**Cây quyết định:**

```text
Có phải file gốc từ khách / họp?
  ├─ Có → assets/public/ hoặc assets/internal/ (không sửa bản gốc)
  └─ Không
       ├─ Đã chốt đủ để ghi DOC?
       │    ├─ Có → docs/ (01-project, 03-modules, 04-platform, …)
       │    └─ Chưa → brainstorm/
       └─ Chỉ tiến độ / nhắc việc của tôi?
            └─ memory/memory.md (+ link DEC / open-questions nếu cần đội)
```

| Bạn đang có gì? | Lưu vào |
|-----------------|---------|
| File **nhận từ ngoài** (email khách, biên bản, checklist gốc) | `assets/public/` hoặc `assets/internal/` — **không sửa file gốc** |
| Ghi chú phiên, phân tích, so sánh phương án, draft chưa chốt | `brainstorm/YYYY-MM-DD*.md` |
| Hiện trạng / tiến độ / nhắc việc **cá nhân** | `memory/memory.md` (gitignore) |
| Câu hỏi dự án **chưa** trả lời (đội) | `memory/open-questions.md` |
| Quyết định + phương án bị loại | `memory/decision-log.md` |
| Nội dung **đủ rõ** cho artifact chính thức | `docs/` |

**Theo phase (chốt vào docs — không còn folder memory theo phase):**

| Phase | Làm việc tạm | Chốt vào |
|-------|--------------|----------|
| Discovery / brainstorm | `brainstorm/` | `docs/01-project/` (DOC-01–03) |
| Requirements | `brainstorm/` | `docs/03-modules/` (DOC-04–07) |
| Architecture | `brainstorm/` (+ TBD trong open-questions nếu thiếu FR) | `docs/04-platform/` (DOC-08–12 / ADR) |

**Đặt tên file `brainstorm/`:**

```text
brainstorm/
├── 2026-07-25.md
├── 2026-07-25-phan-tich-stakeholder.md
└── 2026-07-25-so-sanh-kien-truc-02.md
```

**Không** tạo folder con trong `brainstorm/` — mọi file nằm phẳng ở đó.

**Không nên:**

- Nhồi DEC / SRS dài vào `memory/memory.md` — chỉ hiện trạng + nhắc việc + link
- Tạo folder tùy ý (`notes/`, `temp/`, `drafts/`) hay `memory/{phase}/`
- Sửa file trong `assets/` — copy sang `brainstorm/` rồi phân tích
- Ghi thẳng vào `docs/02-baseline/` — chỉ snapshot sau khi ký

*(Thêm mục H/Đ theo dự án bên dưới.)*
