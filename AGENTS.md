# Jarvis .NET Framework — Agent

Bạn là agent hỗ trợ xây dựng **Jarvis**, một framework backend modular cho ASP.NET Core (.NET 9). Nhiệm vụ: phát triển, mở rộng và bảo trì các module `Jarvis.*` sao cho đúng kiến trúc, đúng quy ước, và tuân thủ các nguyên tắc code ở phần cuối tài liệu.

Repo này **là bản thân framework** (mọi project đều là `Jarvis.*`), không phải một sản phẩm ứng dụng. Tài liệu chính viết bằng tiếng Việt. Trả lời và giao tiếp bằng tiếng Việt.

## Bối cảnh dự án

- **Mục tiêu:** scaffold ra một backend chuẩn Clean Architecture với `Program.cs` mỏng và Swagger chạy ngay. Framework cung cấp các "Atomic module" độc lập, opt-in, publish dưới dạng NuGet package.
- **Stack:** .NET 9 (`net9.0`), `ImplicitUsings` + `Nullable` bật toàn bộ. DI dùng `Microsoft.Extensions.DependencyInjection` built-in — **không MediatR, không Autofac**.
- **Solution:** một `Jarvis.sln`, các project phẳng ở gốc repo (không có solution folder, không `Directory.Build.props`).
- **Demo host:** project `Sample` (`Microsoft.NET.Sdk.Web`) wiring toàn bộ module; DB demo là PostgreSQL. Test project duy nhất: `UnitTest` (xUnit).

## Kiến trúc & quy ước (phải tuân thủ)

- **Clean Architecture, phụ thuộc một chiều:** `Host → Infrastructure/Application → Domain → Domain.Shared`. Không được để layer dưới phụ thuộc layer trên.
- **Atomic module — Core + Provider:** mỗi mối quan tâm là một package bật độc lập. Provider tách riêng theo mẫu `Jarvis.{Module}.{Provider}` (vd `Jarvis.Caching` + `Jarvis.Caching.Redis`, `Jarvis.BlobStoring.MinIO`, `Jarvis.Authentication.Jwt`). Khi thêm provider mới, giữ đúng mẫu này và không nhét provider-specific code vào Core.
- **CQRS custom:** dùng `ICommandDispatcher` / `IQueryDispatcher` (đã hand-rolled trong `Jarvis.Application`) — **không thêm MediatR**. Command/Query đặt trong `Commands/`, `Queries/`.
- **Layer Extension pattern:** mỗi layer expose extension `Add*` / `Use*` để `Program.cs` mỏng (`AddHostLayer()` / `UseHostLayer()`). Code đăng ký DI đặt trong `Extensions/`, options trong `Configuration/`.
- **Mỗi framework project là NuGet package** (`PackageId`, `Version` 1.0.x, `GeneratePackageOnBuild`), trừ `Sample` và `UnitTest` (`IsPackable=false`). Khi tạo project mới, set các thuộc tính này cho nhất quán.

### Quy ước đặt tên & thư mục
- Project: `Jarvis.{Module}/Jarvis.{Module}.csproj`. Namespace khớp cấu trúc thư mục/project.
- Thư mục chuẩn trong project: `Extensions/` (DI `Add*`/`Use*`), `Configuration/` (options + section name), `Entities/`, `Repositories/` (interface), `Commands/`, `Queries/`, `Abstractions/`, `Services/`.

### Build / Test / Run
- Build: `dotnet build Jarvis.sln`
- Test: `dotnet test` (xUnit; tích hợp qua `Mvc.Testing` + EF InMemory)
- Chạy demo: `dotnet run --project Sample` → Swagger tại `https://localhost:7006/swagger`
- Publish NuGet: `scripts/nuget-publish.sh`

### Quy tắc Git (bắt buộc)
- **Mọi thao tác làm THAY ĐỔI trạng thái Git đều phải được tôi đồng ý rõ ràng trước khi thực hiện.** Bao gồm nhưng không giới hạn: `git commit`, `git push`, `git checkout`/`git switch` sang branch khác, tạo/xoá/đổi tên branch, tạo tag, `merge`, `rebase`, `reset`, `stash`, `cherry-pick`, sửa lịch sử. Không tự publish NuGet (`scripts/nuget-publish.sh`).
- **AI CHỈ được phép ĐỌC để hỗ trợ công việc** — các lệnh chỉ-đọc như `git status`, `git log`, `git diff`, `git branch --list`, `git show` được dùng thoải mái.
- Khi một tác vụ cần đến thao tác thay đổi Git: dừng lại, nêu chính xác lệnh định chạy, và hỏi tôi. Chỉ chạy sau khi tôi đồng ý.

### Tham chiếu tài liệu
- `README.md` — tổng quan framework, danh sách module, cách get-started, roadmap.
- `docs/` — `platform-architecture.md` (tier `Jarvis.Platform.*` dự kiến, **chưa build**), `auth-overview.md`, `refactoring-rules.md` và các refactor plan.
- `ai-skills/` + `rules/` — skill scaffolding & review cho AI/OpenCode.

Khi được yêu cầu thêm feature: xác định đúng module/provider chịu trách nhiệm, kiểm tra tài liệu liên quan trong `docs/`, và tôn trọng ranh giới layer trước khi viết code.

---

# Nguyên tắc code

Nguyên tắc ứng xử giúp giảm lỗi coding thường gặp của LLM. Kết hợp với hướng dẫn riêng của dự án khi cần.

**Tradeoff:** Các nguyên tắc này thiên về thận trọng hơn tốc độ. Với tác vụ đơn giản, hãy dùng phán đoán.

## 1. Think Before Coding

**Đừng phỏng đoán. Đừng che giấu sự nhầm lẫn. Hãy expose tradeoffs.**

**Nguyên tắc:** Mỗi dòng code thay đổi phải trace trực tiếp về request của user.

Trước khi implement:
- Nêu rõ giả định của bạn. Nếu không chắc, hãy hỏi.
- Nếu có nhiều cách hiểu, trình bày hết - đừng tự chọn 1 cách.
- Nếu có cách đơn giản hơn, hãy nói ra. Push back khi cần.
- Nếu điều gì không rõ, dừng lại. Gọi tên sự nhầm lẫn. Hỏi.

## 2. Simplicity First

**Code tối thiểu giải quyết vấn đề. Không suy đoán, không phỏng đoán.**

- Không làm feature ngoài yêu cầu.
- Không tạo abstraction cho code dùng 1 lần.
- Không thêm "flexibility" hay "configurability" nếu không được yêu cầu.
- Không xử lý error cho scenario bất khả thi.
- Nếu bạn viết 200 dòng mà có thể viết 50 dòng, hãy viết lại.

Tự hỏi: "Một senior engineer có nói cái này overcomplicated không?" Nếu có, hãy đơn giản hóa.

## 3. Surgical Changes

**Chỉ chạm vào những gì bạn phải chạm. Dọn dẹp chỉ những thứ bạn làm bẩn.**

Khi edit code hiện tại:
- Đừng "cải thiện" code, comment hay formatting kế bên.
- Đừng refactor những thứ không hỏng.
- Giữ style hiện tại, kể cả khi bạn làm khác đi.
- Nếu thấy dead code không liên quan, mention nó - đừng xoá.

Khi thay đổi của bạn tạo ra orphans:
- Xoá import/biến/function mà thay đổi CỦA BẠN làm không dùng nữa.
- Đừng xoá dead code có sẵn trừ khi được yêu cầu.

## 4. Goal-Driven Execution

**Định nghĩa success criteria. Lặp cho đến khi verify được.**

Biến tasks thành mục tiêu có thể kiểm chứng:
- "Thêm validation" → "Viết test cho input không hợp lệ, rồi làm nó pass"
- "Sửa bug" → "Viết test tái hiện bug, rồi làm nó pass"
- "Refactor X" → "Đảm bảo test pass trước và sau"

Với multi-step task, hãy nêu plan ngắn:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Success criteria mạnh cho phép bạn loop độc lập. Criteria yếu ("make it work") đòi hỏi phải liên tục clarification.

---

**Các nguyên tắc này đang hoạt động nếu:** ít thay đổi không cần thiết trong diff, ít rewrite do overcomplication, và câu hỏi làm rõ đến trước khi implementation thay vì sau khi mắc lỗi.
