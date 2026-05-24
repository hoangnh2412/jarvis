# Template: Skill Jarvis .NET (OpenCode)

Chuẩn tạo skill trong `.opencode/skills/<tên-skill>/` cho framework Jarvis. **Không** dùng một file SKILL dài kiểu Purpose / Role / Process chung chung.

**Tham chiếu đầy đủ:** [healthcheck-dotnet](../.opencode/skills/healthcheck-dotnet/)  
**Bản đồ skill:** [.opencode/README.md](../.opencode/README.md)

---

## 1. Khi nào tạo skill riêng

| Tạo skill `*-dotnet` riêng | Giữ trong `jarvis-dotnet/modules/` |
|----------------------------|-------------------------------------|
| Module Jarvis có workflow init/add phức tạp | Module nhỏ, chỉ vài dòng bootstrap |
| Nhiều biến thể (providers, patterns) | Chưa cần tách — link từ orchestrator |
| Agent cần discover qua `description` (health, OTEL, EF, cache) | Chỉ dùng khi scaffold/add toàn solution |

Ví dụ skill độc lập: `healthcheck-dotnet`, `caching-dotnet`, `telemetry-dotnet`, `entityframework-dotnet`, `swashbuckle-dotnet`.

---

## 2. Cây thư mục chuẩn

```text
.opencode/skills/<skill-name>/
├── SKILL.md                 # Orchestrator — agent đọc file này trước
├── README.md                # Hướng dẫn người dùng — prompt @, khi nào dùng
├── workflows/
│   ├── init.md              # Chưa có module Jarvis trên project
│   └── add.md               # Đã có core, thêm provider/pattern
├── providers/               # Biến thể atomic (health, cache, OTEL, swagger security)
│   └── <provider-name>/
│       └── SKILL.md
├── patterns/                # (tùy chọn) Thay providers — EF multitenancy
│   └── <pattern-name>/
│       └── SKILL.md
├── templates/               # Snippet copy vào solution (cs, json)
│   ├── program-setup.cs
│   └── appsettings-*.json
└── reference/               # (tùy chọn) Doc dài, không load mặc định
    └── *.md
```

**Quy ước tên**

| Thành phần | Quy tắc | Ví dụ |
|------------|---------|--------|
| Thư mục skill | `kebab-case`, hậu tố `-dotnet` | `caching-dotnet` |
| `name` frontmatter | trùng tên thư mục | `caching-dotnet` |
| Provider skill `name` | `<skill>-<provider>` | `healthcheck-dotnet-postgresql` |
| Workflow | `init` / `add` — không đổi tên | `workflows/init.md` |

---

## 3. Phân vai file

| File | Đối tượng | Nội dung |
|------|-----------|----------|
| **SKILL.md** | Agent | Frontmatter, bảng workflow, quy tắc cốt lõi, catalog providers, output bắt buộc |
| **README.md** | Developer | Khi nào dùng, cách gọi `@.opencode/skills/...`, link nhanh |
| **workflows/** | Agent | Checklist từng bước, package, validate |
| **providers/** / **patterns/** | Agent | Chỉ load **một** file cần cho task hiện tại |
| **templates/** | Agent | Code mẫu thay placeholder `{Product}`, `{App}` |

**Không** copy nguyên SKILL.md sang README — README link và tóm tắt.

---

## 4. Mẫu `SKILL.md` (orchestrator)

```markdown
---
name: <skill-name>
description: <WHAT — một câu> <WHEN — trigger: package, API, config key>. Viết ngôi thứ ba, có từ khóa discovery.
metadata:
  audience: hoangnh
  workflow: github
---

# <Jarvis.Module> — Orchestrator

Skill điều phối `<PackageId>` trên ASP.NET Core.

Kiến trúc / hướng dẫn người: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có <module> | [workflows/init.md](workflows/init.md) |
| Đã có core, cần thêm <variant> | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- ...
- Không hardcode secret / connection string
- Thứ tự DI nếu phụ thuộc module khác (vd. Caching trước EF)

## Packages

| PackageId | Version* | Layer |
|---|---|---|
| `Jarvis.*` | x.x.x | Host / Infrastructure |

## Providers (atomic)

Chỉ đọc provider cần dùng:

| Provider | Path |
|---|---|
| <Tên> | [providers/<name>/SKILL.md](providers/<name>/SKILL.md) |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs)
- [templates/appsettings-*.json](templates/appsettings-*.json)

## Tham chiếu (tùy chọn)

- [reference/...](reference/...)

## Output bắt buộc

- Thay đổi `Program.cs` hoặc `*LayerExtension.cs`
- `appsettings.json` section `...`
- `dotnet build` thành công
- Checklist validate (endpoint, probe, …)
```

**`description` frontmatter** — quan trọng cho agent chọn skill:

- ✅ `Thiết lập Jarvis.HealthChecks — init liveness/readiness, thêm provider PostgreSQL. Dùng khi /health/ready, healthcheck .NET.`
- ❌ `Giúp bạn làm healthcheck` (mơ hồ, không có trigger)

---

## 5. Mẫu `README.md` (người dùng)

```markdown
# <skill-name>

Skill tích hợp **<Jarvis.Module>**. Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| ... | [workflows/init.md](./workflows/init.md) |
| ... | [workflows/add.md](./workflows/add.md) |

**Không dùng cho:** ... (vd. scaffold solution → jarvis-dotnet)

Scaffold `jarvis-dotnet` đã ... — dùng skill này khi ...

## Cách gọi

\`\`\`text
@.opencode/skills/<skill-name>/workflows/add.md

<Mô tả task cụ thể, config path, tên project>
\`\`\`

## Quy tắc (tóm tắt)

- ...

## Providers / Patterns

| ... | SKILL |
|-----|-------|
| ... | [providers/.../SKILL.md](./providers/.../SKILL.md) |

## Liên quan

- [jarvis-dotnet/README.md](../jarvis-dotnet/README.md)
- [skill-phụ-thuộc](../other-skill/README.md)
```

---

## 6. Mẫu `workflows/init.md`

```markdown
# Workflow: Khởi tạo <Module>

Áp dụng khi project **chưa** có `<bootstrap-method>()`.

## Checklist

\`\`\`text
- [ ] 1. Phân tích layer (Host / Infrastructure)
- [ ] 2. Package Jarvis.*
- [ ] 3. Đăng ký DI (thứ tự đúng)
- [ ] 4. appsettings section
- [ ] 5. Validate
\`\`\`

## Bước 1 — Package

\`\`\`xml
<PackageReference Include="Jarvis.*" Version="*" />
\`\`\`

## Bước 2 — Registration

Dùng [templates/program-setup.cs](../templates/program-setup.cs):

\`\`\`csharp
// snippet
\`\`\`

## Bước 3 — appsettings

[templates/appsettings-*.json](../templates/appsettings-*.json)

## Bước 4 — Validate

- dotnet build
- ...

## Sau init

Cần thêm biến thể → [workflows/add.md](add.md).
```

---

## 7. Mẫu `workflows/add.md`

```markdown
# Workflow: Thêm <provider/pattern>

Áp dụng khi **đã có** core và cần thêm một dependency / biến thể.

## Checklist

\`\`\`text
- [ ] 1. Chọn provider trong providers/ (hoặc patterns/)
- [ ] 2. Đọc providers/<name>/SKILL.md — chỉ một file
- [ ] 3. NuGet từ frontmatter dependencies
- [ ] 4. appsettings
- [ ] 5. Registration
- [ ] 6. Validate
\`\`\`

## Bước 1 — Chọn provider

Không load toàn bộ thư mục providers — chỉ file cần dùng.

## Anti-patterns

- ...
```

---

## 8. Mẫu `providers/<name>/SKILL.md`

```markdown
---
name: <skill-name>-<provider-name>
description: <Một câu WHAT + WHEN cho provider này>
dependencies:
  - Package.ThirdParty
  - Jarvis.*
---

# <Provider display name>

## appsettings

\`\`\`json
{
  "Section": { }
}
\`\`\`

## Registration

\`\`\`csharp
// snippet copy vào extension / Program.cs
\`\`\`

## Validate

- ...
```

**Nguyên tắc provider:** một file = một cách cấu hình; snippet ngắn; không lặp toàn bộ orchestrator.

---

## 9. `patterns/` thay `providers/` (EF)

Dùng cho **entityframework-dotnet** — mô hình multitenancy, không phải infrastructure probe:

```text
patterns/
├── single-db/SKILL.md
├── separate-tenant-db/SKILL.md
├── hybrid/SKILL.md
└── custom-di/SKILL.md
reference/
└── setup.md          # DbContext, UoW, luồng resolve — đọc trước patterns
```

Orchestrator trỏ: `patterns/` + `reference/setup.md` thay vì bảng providers.

---

## 10. Quan hệ với `jarvis-dotnet`

| Vai trò | Skill |
|---------|--------|
| Scaffold / init / add **cả solution** | `jarvis-dotnet` |
| Chỉ health / cache / EF / OTEL / Swagger | skill `*-dotnet` tương ứng |

Trong `jarvis-dotnet/SKILL.md` — bảng Modules:

```markdown
| Module | Path trong jarvis-dotnet | Skill chuyên sâu |
|--------|--------------------------|------------------|
| Health checks | — | [healthcheck-dotnet](../healthcheck-dotnet/README.md) |
| Caching | — | [caching-dotnet](../caching-dotnet/README.md) |
| Authentication | modules/authentication/SKILL.md | — |
```

**Không** duplicate: sau khi tách skill độc lập, xóa `jarvis-dotnet/modules/<module>/` và chỉ giữ link.

---

## 11. Checklist tạo skill mới

```text
- [ ] Tên thư mục <name>-dotnet, frontmatter name khớp
- [ ] description có WHAT + WHEN (tiếng Việt hoặc Anh, ngôi thứ ba)
- [ ] SKILL.md — orchestrator (workflow table, rules, output)
- [ ] README.md — prompt @ mẫu, khi nào / không khi nào
- [ ] workflows/init.md + add.md (checklist)
- [ ] providers/ hoặc patterns/ — mỗi biến thể một SKILL.md + dependencies
- [ ] templates/ — program + appsettings mẫu
- [ ] Cập nhật .opencode/README.md
- [ ] Cập nhật jarvis-dotnet (link, bỏ module trùng)
- [ ] Không tham chiếu docs/ cũ — mọi doc nằm trong skill hoặc README repo
```

---

## 12. Ví dụ map từ template cũ → chuẩn mới

| Template cũ (1 file) | Chuẩn mới |
|----------------------|-----------|
| Purpose | `description` + đoạn mở SKILL.md |
| Scope | README «Khi nào dùng» / «Không dùng» |
| Role | *(bỏ)* |
| Input | Workflow bước 1 «Phân tích» + prompt user |
| Output | `## Output bắt buộc` trong SKILL.md |
| Rules | `## Quy tắc cốt lõi` |
| Process | `workflows/init.md`, `workflows/add.md` |
| Constraints | Trong rules hoặc provider |
| Examples | `templates/` + `providers/*/SKILL.md` |
| Anti-patterns | Cuối `workflows/add.md` |

---

## 13. Skill tham chiếu theo độ phức tạp

| Mức | Skill | Đặc điểm |
|-----|-------|----------|
| Đầy đủ | [healthcheck-dotnet](../.opencode/skills/healthcheck-dotnet/) | Nhiều providers, 2 templates |
| Trung bình | [caching-dotnet](../.opencode/skills/caching-dotnet/) | 3 providers + reference |
| Trung bình | [telemetry-dotnet](../.opencode/skills/telemetry-dotnet/) | 6 providers, OTEL templates |
| Patterns | [entityframework-dotnet](../.opencode/skills/entityframework-dotnet/) | `patterns/` + `reference/setup.md` |
| Review | [code-review-dotnet](../.opencode/skills/code-review-dotnet/) | Không workflows/providers — checklist trong SKILL |
