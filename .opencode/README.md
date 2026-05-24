# OpenCode skills — Jarvis .NET

Bản đồ skill AI cho repo Jarvis framework. Mỗi skill có **README** (người dùng) và **SKILL.md** (agent).

## Skills

| Skill | README | Mô tả |
|-------|--------|--------|
| **jarvis-dotnet** | [skills/jarvis-dotnet/README.md](./skills/jarvis-dotnet/README.md) | Scaffold / init / add solution Jarvis |
| **foundation-dotnet** | [skills/foundation-dotnet/README.md](./skills/foundation-dotnet/README.md) | Json, CORS, WebApi, ApiResponseWrapper |
| **application-dotnet** | [skills/application-dotnet/README.md](./skills/application-dotnet/README.md) | CQRS Application layer |
| **authentication-dotnet** | [skills/authentication-dotnet/README.md](./skills/authentication-dotnet/README.md) | JWT, API Key, Cognito |
| **notification-dotnet** | [skills/notification-dotnet/README.md](./skills/notification-dotnet/README.md) | Email SMTP Mailkit |
| **caching-dotnet** | [skills/caching-dotnet/README.md](./skills/caching-dotnet/README.md) | Memory + Redis cache |
| **entityframework-dotnet** | [skills/entityframework-dotnet/README.md](./skills/entityframework-dotnet/README.md) | EF multitenancy |
| **swashbuckle-dotnet** | [skills/swashbuckle-dotnet/README.md](./skills/swashbuckle-dotnet/README.md) | Swagger / OpenAPI |
| **healthcheck-dotnet** | [skills/healthcheck-dotnet/README.md](./skills/healthcheck-dotnet/README.md) | Health endpoints |
| **telemetry-dotnet** | [skills/telemetry-dotnet/README.md](./skills/telemetry-dotnet/README.md) | OpenTelemetry |
| **analyze-metric-dotnet** | [skills/analyze-metric-dotnet/README.md](./skills/analyze-metric-dotnet/README.md) | Đọc Grafana Dotnet Runtime Metrics |
| **blobstoring-dotnet** | [skills/blobstoring-dotnet/README.md](./skills/blobstoring-dotnet/README.md) | FileSystem / MinIO blob |
| **code-review-dotnet** | [skills/code-review-dotnet/README.md](./skills/code-review-dotnet/README.md) | Review PR C#/.NET |

## Prompt nhanh

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md
Scaffold backend .NET 9: Product=Acme, product=acme
```

```text
@.opencode/skills/entityframework-dotnet/workflows/init.md
Init Jarvis EF + single DB cho MyApp
```

```text
@.opencode/skills/analyze-metric-dotnet/workflows/analyze.md
Giải thích panel GC và thread pool trên Dotnet Runtime Metrics, job=myapp
```

Framework overview: [README.md](../README.md) (repo gốc).

## Publish skill sang repo consumer

**Nguồn chính (source of truth):** thư mục `.opencode/` trong repo **Jarvis framework** (repo này). Repo product (`{product}-backend`) **không** fork/sửa skill — chỉ nhận bản cập nhật từ Jarvis hoặc PR upstream.

### Cấu trúc bắt buộc trên consumer

```text
{product}-backend/
└── .opencode/
    ├── README.md          # copy hoặc symlink từ Jarvis (file này)
    └── skills/
        ├── jarvis-dotnet/
        ├── caching-dotnet/
        └── ...
```

Agent/Cursor gọi skill bằng path **tương đối repo product**:

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md
```

### Cách đưa `.opencode/` vào repo product

| Cách | Khi nào dùng | Ghi chú |
|------|----------------|---------|
| **Git submodule** | Nhiều team, cần pin version skill | Submodule trỏ repo Jarvis; consumer chỉ mount/copy `.opencode` (xem script dưới) |
| **Symlink** | Dev local, Jarvis clone cạnh product repo | `ln -s ../../Jarvis/.opencode .opencode` — không commit symlink lên Windows CI |
| **Copy (script/CI)** | Pin release, không phụ thuộc submodule path | Script copy tree `.opencode/` từ tag Jarvis — **khuyến nghị cho CI** |
| **Monorepo** | Product và Jarvis cùng workspace | Một `.opencode/` ở root monorepo hoặc symlink như trên |

**Không** commit nội dung skill đã chỉnh tay trong repo product — sửa tại repo Jarvis rồi sync lại.

### Submodule (khuyến nghị team)

```bash
# Trong repo {product}-backend (root)
git submodule add <url-repo-jarvis> vendor/jarvis
git submodule update --init --recursive

# Đồng bộ .opencode từ submodule (chạy sau mỗi lần update submodule)
rsync -a --delete vendor/jarvis/.opencode/ .opencode/
```

Hoặc chỉ submodule thư mục skills (sparse) nếu host Git hỗ trợ — mặc định submodule cả repo Jarvis rồi `rsync` `.opencode/`.

Pin version: checkout tag/commit cố định trong `vendor/jarvis`, commit SHA submodule, chạy lại `rsync`.

### Symlink (dev local)

```bash
cd /path/to/acme-backend
ln -snf /path/to/Jarvis_2/.opencode .opencode
```

Thêm `.opencode` vào `.gitignore` nếu symlink chỉ dùng local; CI dùng copy/submodule.

### Copy một lần / release script

```bash
JARVIS_ROOT=/path/to/Jarvis_2
PRODUCT_ROOT=/path/to/acme-backend

rsync -a --delete \
  "$JARVIS_ROOT/.opencode/" \
  "$PRODUCT_ROOT/.opencode/"
```

Chạy trong pipeline khi bump `JARVIS_SKILLS_REF=v1.2.0` (tag trên repo Jarvis).

### Quy ước cập nhật

1. Thay đổi skill → PR trên **repo Jarvis** (review + merge `develop` / tag release).
2. Repo product: `git submodule update` hoặc chạy script `rsync` theo tag mới.
3. PR product ghi dòng: `chore: sync Jarvis skills @ <tag hoặc commit short>` — không trộn thay đổi skill với feature app.
4. Breaking skill (đổi workflow, package version bắt buộc): ghi trong PR Jarvis + tag semver skill (`skills-v1.3.0`) — consumer bump có chủ đích.

### Product repo sau khi có `.opencode/`

- README product: một dòng link `Skill AI: [.opencode/README.md](.opencode/README.md)`.
- Không duplicate bảng skill — link hub Jarvis hoặc copy README hub khi `rsync` (file này đi kèm).
- Scaffold mới: luôn dùng `@.opencode/skills/jarvis-dotnet/workflows/scaffold.md`.

### Checklist publish (maintainer Jarvis)

```text
- [ ] Tag hoặc commit trên develop ổn định
- [ ] rsync/submodule update trên ít nhất một repo product thử
- [ ] Smoke: @jarvis-dotnet scaffold, @authentication-dotnet jwt, @code-review-dotnet
- [ ] Ghi tag/release note nếu breaking
```

Chi tiết changelog skill: mục roadmap repo gốc [README.md](../README.md) (Việc cần làm tiếp theo).
