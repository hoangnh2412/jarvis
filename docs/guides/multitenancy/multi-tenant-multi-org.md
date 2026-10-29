# Multi-tenant, multi-org

Mô hình phân cấp của nền tảng Jarvis: **Root → Tenant → Org → OrgStructure**.

Tài liệu này là mô hình nghiệp vụ (cập nhật 2026-10-29: Org catalog `ParentId` / tập đoàn). ADR Tenants, Orgs và OrgStructure cần đồng bộ khi chốt slice `platform_core`. Trên code VNA Host: `ParentTenantId` catalog tenant đã có; Org catalog `ParentId` và schema `platform_core` đang theo kế hoạch refactor.

## Bốn lớp

| Lớp | Vai trò | Dữ liệu nghiệp vụ (hóa đơn) |
|-----|---------|------------------------------|
| **Root** | Node sau lần cài đặt đầu. Quản trị toàn bộ Tenant và Org. | Không giữ |
| **Tenant** | Vùng dữ liệu của một khách nền tảng. Có thể có Tenant con. | Không giữ bản ghi hóa đơn. Quản lý cây Tenant con và các Org trong vùng của mình |
| **Org** | Chủ thể sở hữu resource / người bán trên hóa đơn. Tenant tạo Org. Trong **một tenant**, Org có thể **đa cấp** (`ParentId`) — ví dụ tập đoàn: công ty mẹ và công ty thành viên đều là Org catalog, mỗi node có thể phát hành HĐ. | Giữ bộ hóa đơn của **Org đó** (và theo quy tắc scope nghiệp vụ của host) |
| **OrgStructure** | Cây tổ chức **bên trong một Org** (một `org_id`): phòng ban; với VNA có thể là chi nhánh, phòng vé, đại lý **khi không tạo Org khách riêng** | Không giữ. Hóa đơn vẫn gắn Org catalog (issuer), OU dùng cho ABAC / phạm vi thao tác |

Một node chỉ tạo node ở cấp kế dưới:

```text
Root tạo Tenant
Tenant tạo Tenant con hoặc Org (Org gốc trong tenant)
Org tạo Org con (cùng tenant, ParentId) hoặc OrgStructure
OrgStructure không tạo Org catalog
```

**Phân tầng Org catalog vs OrgStructure**

| Câu hỏi | Trả lời |
|---------|---------|
| Công ty thành viên trong **tập đoàn** (VNA) hoặc **Khu** EPAY cần xuất HĐ? | **Org con** catalog (`ParentId` → Org mẹ), cùng `tenant_id` — EPAY: Org EPAY + Org Khu I/II/III. |
| Phòng ban / chi nhánh **trong** một công ty đã chọn? | **OrgStructure** (`system.organizational_units`), `org_id` = Org đó. |
| Chi nhánh Minvoice tự bán và tạo Org khách? | **Tenant con** + Org khách phẳng — **không** dùng OrgStructure thay Tenant. |

OrgStructure không tạo Org catalog. Đơn vị phải tự tạo Org cho khách thì đơn vị đó là **Tenant** (có thể là Tenant con), trừ khi khách là **Org con** trong cùng tenant tập đoàn (issuer nội bộ).

## Khi nào có Tenant con

Tenant con xuất hiện khi một bộ phận phải có vùng Org riêng và tự tạo Org khách, trong khi Tenant cha vẫn quản trị nó.

Ví dụ: chi nhánh HCM của Minvoice tạo Org cho khách của HCM. HCM là Tenant con của Tenant Minvoice. Khách của HCM là Org nằm trong Tenant HCM.

Chi nhánh VNA **không** tạo Org khách riêng theo mô hình kênh Minvoice. Hóa đơn gắn Org VNA (hoặc Org thành viên nếu triển khai tập đoàn). Chi nhánh vận hành nội bộ thường là **OrgStructure** dưới một Org.

**Tập đoàn (cùng tenant):** Tenant VNA có thể có nhiều Org catalog — Org TCT (gốc, `parent_id` null) và các Org công ty thành viên (`parent_id` → Org TCT hoặc cây sâu hơn). Mỗi Org thành viên có OrgStructure riêng. Khác với Tenant con: thành viên **không** tách connection string tenant.

Đại lý bán cho khách dưới chi nhánh HCM là Tenant con của Tenant HCM. Đại lý do Minvoice quản lý trực tiếp là Tenant con của Tenant Minvoice.

## Sơ đồ

```text
Root: Minipower
│
├── Tenant Minvoice                         ← cung cấp dịch vụ hóa đơn
│   ├── Org Minvoice
│   │     └── OrgStructure: phòng ban Minvoice
│   ├── Org Khách do Minvoice bán trực tiếp
│   │     └── OrgStructure: phòng ban của khách
│   │
│   ├── Tenant Chi nhánh HCM
│   │     ├── Org Chi nhánh HCM
│   │     │     └── OrgStructure: phòng ban HCM
│   │     └── Org Khách của HCM
│   │           └── OrgStructure: phòng ban của khách
│   │
│   ├── Tenant Chi nhánh HN
│   │     ├── Org Chi nhánh HN
│   │     │     └── OrgStructure: phòng ban HN
│   │     └── Org Khách của HN
│   │           └── OrgStructure: phòng ban của khách
│   │
│   └── Tenant Đại lý                       ← đại lý trực tiếp của Minvoice
│         ├── Org Đại lý
│         │     └── OrgStructure: phòng ban đại lý
│         └── Org Khách của đại lý
│               └── OrgStructure: phòng ban của khách
│
├── Tenant EPAY
│   ├── Org EPAY                                ← parent_id null; quản trị tập đoàn EPAY (không bắt buộc issuer HĐ)
│   │     ├── OrgStructure: phòng ban / bộ máy EPAY (gắn Org EPAY)
│   │     ├── Org Khu I                         ← Org con; ParentId → Org EPAY; đơn vị xuất hóa đơn
│   │     │     └── OrgStructure: phòng ban Khu I
│   │     ├── Org Khu II                        ← Org con; ParentId → Org EPAY; đơn vị xuất hóa đơn
│   │     │     └── OrgStructure: phòng ban Khu II
│   │     └── Org Khu III                       ← Org con; ParentId → Org EPAY; đơn vị xuất hóa đơn
│   │           └── OrgStructure: phòng ban Khu III
│   └── Org Khách cuối                          ← Org anh em Org EPAY (cùng tenant; parent_id null hoặc TBD host)
│         └── OrgStructure: phòng ban của khách
│
└── Tenant VNA
      └── Org TCT VNA                           ← parent_id null. *Hoặc* một Org VNA duy nhất + OrgStructure (không Org con) — chọn một mô hình
            ├── Org Công ty thành viên (con)    ← chỉ mô hình tập đoàn; ParentId → Org TCT; mỗi CT xuất HĐ
            │     └── OrgStructure: phòng ban CT
            └── OrgStructure (gắn Org issuer đang dùng)
                  ├── Chi nhánh
                  │     └── Phòng ban
                  ├── Phòng vé
                  └── Đại lý (node OU, không Org khách)
```

**Ghi chú sơ đồ:** **EPAY:** Khu I / II / III là **Org con** của **Org EPAY** (cùng `tenant_id` Tenant EPAY), không phải node catalog “con của tenant”. **VNA:** ghi chú *Hoặc một Org VNA…* nằm trên dòng `Org TCT VNA` — không phải node thứ hai dưới tenant. Minvoice: Org phẳng trong tenant; phân cấp kênh = **Tenant con**.

Tenant Minvoice, EPAY và VNA ngang nhau dưới Root. Tenant HCM và HN ngang nhau dưới Tenant Minvoice.

Hóa đơn của khách thuộc Org khách. Tenant cha cung cấp dịch vụ và quản trị cây Tenant, không giữ bản ghi hóa đơn của Org trong Tenant con.

## Bảy tình huống Minvoice / EPAY / VNA

| # | Tình huống | Vị trí |
|---|------------|--------|
| 1 | HCM, HN là kênh bán; hóa đơn khách do Minvoice cung cấp dịch vụ | **Tenant con** của Tenant Minvoice. Không phải OrgStructure của Org Minvoice |
| 2 | Khách của HCM, HN hoặc của đại lý | **Org** trong Tenant của kênh đó (Tenant HCM, Tenant HN hoặc Tenant đại lý) |
| 3 | Khách do Minvoice bán trực tiếp | **Org** trong Tenant Minvoice, cạnh Org Minvoice |
| 4 | Minvoice, HCM, HN có phòng ban | OrgStructure của **Org Minvoice**, **Org Chi nhánh HCM**, **Org Chi nhánh HN** — mỗi Org một cây |
| 5 | EPAY: Khu I, II, III xuất hóa đơn | **Org con** của **Org EPAY**; Org EPAY có **OrgStructure** riêng; mỗi Khu có OrgStructure riêng |
| 6 | Khách cuối có phòng ban | OrgStructure của **Org khách** |
| 7 | Chi nhánh VNA có phòng ban | OrgStructure của **Org VNA** (hoặc Org thành viên): phòng ban là con của node chi nhánh |
| 8 | Tập đoàn: công ty thành viên xuất HĐ | **Org con** catalog (`ParentId`), cùng Tenant VNA; OrgStructure riêng từng Org |

## Hiện trạng so với ADR

| Việc trong mô hình | ADR | Code |
|--------------------|-----|------|
| Tenant gốc và Tenant con | [ADR Tenants](../../ADRs/2026-08-07_platform_tenants-module.md) T11 | `ParentTenantId` trên Master catalog (đổi tên `ParentId` theo kế hoạch) |
| Org issuer; Org con trong tenant (tập đoàn) | [ADR Orgs](../../ADRs/2026-10-17_platform_orgs-module.md) — **cần change log** | Catalog một cấp; chưa `ParentId` |
| Catalog trên Master | [ADR-009](../../../docs/04-platform/DOC-09-adr/ADR-009-master-app-database-topology-platform-catalog.md) — **→ `platform_core`** | Hiện `public.tenants` / `public.orgs` |
| Cây trong một Org; VNA dùng `Branch` | [ADR OrgStructure](../../ADRs/2026-10-17_platform_orgstructure-module.md) | Module chưa code |
| Sự kiện sau khi ghi Org | [ADR catalog events](../../ADRs/2026-10-21_platform_orgs-catalog-events.md) | Type và publish trong `OrgManager` đã có; test publish chưa đủ |
