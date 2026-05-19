# code-review

Skill review code **C#/.NET** trước khi mở PR hoặc trước khi human reviewer xem. Áp dụng cho mọi solution backend; checklist đầy đủ trong [SKILL.md](./SKILL.md).

## Khi nào dùng

- Trước khi tạo PR trên GitHub
- Sau khi implement xong feature/fix, muốn bắt bug và rủi ro production sớm
- Khi cần ý kiến thứ hai về async/EF/DI/security/concurrency trên diff hiện tại

**Không dùng cho:** format code, đổi naming cosmetic, hoặc refactor lớn không liên quan PR.

## Cách gọi skill

Trong OpenCode / agent chat, làm một trong các cách sau:

1. **Tham chiếu file skill** (khuyến nghị):

   ```text
   Review PR theo skill @.opencode/skills/code-review/SKILL.md
   ```

2. **Gọi theo tên skill** (nếu tool/agent đã index skill `code-review`):

   ```text
   Dùng skill code-review để review thay đổi trên branch hiện tại
   ```

3. **Đính kèm file cần review** kèm lệnh rõ ràng:

   ```text
   @src/MyApp/Application/Orders/CreateOrderHandler.cs
   Review file này theo .opencode/skills/code-review/SKILL.md
   ```

Agent sẽ đọc [SKILL.md](./SKILL.md) và làm theo **Workflow** trong đó.

## Chuẩn bị trước khi review

| Việc nên làm | Lý do |
|--------------|--------|
| Commit hoặc stage thay đổi cần review | Agent lấy diff từ git |
| Biết branch gốc (`main`, `develop`, …) | Mặc định diff là `<base>...HEAD` |
| Ở đúng repo / working tree | Diff đúng phạm vi PR |

Nếu chưa có git hoặc chưa commit, nêu rõ file/path — agent sẽ review theo file bạn chỉ định thay vì full PR diff.

## Prompt mẫu

### Review toàn bộ thay đổi trên branch (thường dùng nhất)

```text
Review code trước PR theo @.opencode/skills/code-review/SKILL.md
Base branch: main
```

### Review với base branch khác

```text
Review thay đổi so với develop theo skill code-review
```

### Review một vài file cụ thể

```text
Review các file sau theo skill code-review:
- src/App.Api/Controllers/OrdersController.cs
- src/App.Infrastructure/Repositories/OrderRepository.cs
```

### Kèm draft commit message (optional)

Mặc định skill **không** xuất section Commit Message. Chỉ thêm khi bạn yêu cầu:

```text
Review PR theo skill code-review và draft commit message (Conventional Commits)
Base: main
```

### Solution dùng kiến trúc Jarvis

Skill vẫn là review C# chung. Nếu repo theo layer Jarvis, agent tự bổ sung kiểm tra cấu trúc/DI theo [jarvis-dotnet](../jarvis-dotnet/SKILL.md) — bạn có thể nhắc thêm trong prompt:

```text
Review PR theo code-review; solution theo Jarvis layered architecture
```

## Agent sẽ làm gì

Theo [SKILL.md](./SKILL.md):

1. Lấy phạm vi (`git diff <base>...HEAD` hoặc file bạn chỉ định)
2. Đọc diff + call chain một cấp nếu cần
3. Duyệt toàn bộ checklist (C#/.NET, validation, security, …)
4. Chỉ báo issue có **execution path cụ thể** — không nitpick style
5. Trả kết quả theo format chuẩn bên dưới

## Đọc kết quả review

### Các section trong output

| Section | Ý nghĩa |
|---------|---------|
| **Critical Issues** | Phải xử lý trước merge (security, data, concurrency, logic production, breaking API) |
| **Suggestions** | Nên sửa — rủi ro trung bình, validation, perf có bằng chứng, thiếu test quan trọng |
| **Best Practices & Improvements** | Cải thiện có giá trị, không khẩn cấp — không phải lời khuyên chung chung |
| **Summary** | Tóm tắt theo file + `overall`: `merge-ready` / `needs changes` / `blocked` |
| **Commit Message** | Chỉ có khi bạn yêu cầu trong prompt |

### Một suggestion điển hình

```markdown
### src/App.Api/Controllers/OrdersController.cs

Issue: ...
Impact: ...
Suggested Fix: ...
```

### Khi không có vấn đề đáng kể

Agent sẽ ghi: **No significant issues found** (vẫn có Summary).

### Khi thiếu context

Agent ghi **Need more context** và nêu cần thêm (base branch, danh sách file, v.v.).

## Sau review

1. Sửa **Critical Issues** trước
2. Cân nhắc **Suggestions** theo thời gian/ risk
3. **Best Practices** — làm nếu ROI rõ, không bắt buộc trước merge
4. Chạy lại review sau khi sửa lớn (đặc biệt EF, auth, multitenancy, transaction)

## Tài liệu liên quan

| File | Nội dung |
|------|----------|
| [SKILL.md](./SKILL.md) | Quy tắc đầy đủ, checklist, workflow, format output |
| [jarvis-dotnet/SKILL.md](../jarvis-dotnet/SKILL.md) | Reference kiến trúc layer khi repo dùng Jarvis (optional) |

## Lưu ý

- Skill ưu tiên **production risk**, không thay human reviewer hoàn toàn
- Checklist dài là **cố ý** — agent tự đối chiếu từng hạng mục với PR, không bắt buộc phải có issue ở mọi mục
- Không mong đợi comment về formatting, EditorConfig, hay refactor lớn không cần thiết
