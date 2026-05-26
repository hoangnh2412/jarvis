# Templates scaffold

Thay `{Product}`, `{product}`, `{JarvisRoot}` trước khi copy vào solution.

| File | Đích |
|---|---|
| [SKILLS.md](SKILLS.md) | **Bản đồ** template → skill `*-dotnet` (mở rộng sau scaffold) |
| `solution-tree.txt` | Cây thư mục chuẩn |
| `layer-csproj/*.xml` | Jarvis references theo layer (+ comment skill) |
| `layers/*` | Source mặc định (wire tối thiểu; xem SKILLS.md) |
| `docs-README.md` | README repo product |
| `docs-Architecture.md` | `docs/Architecture.md` product |

Workflow: [workflows/scaffold.md](../workflows/scaffold.md).

Dùng skill độc lập trong `.opencode/skills/<tên>-dotnet/` — xem [SKILLS.md](SKILLS.md).
