# Multi-tenant, multi-org

Mô hình phân cấp của nền tảng Jarvis: **Root → Tenant → Org → OrgStructure**.

Tài liệu này là mô hình nghiệp vụ đã thống nhất (2026-10-25). ADR Tenants, Orgs và OrgStructure đã được sửa cho khớp cây. Code platform **chưa** có `ParentTenantId` và **chưa** có module OrgStructure.

## Bốn lớp

| Lớp | Vai trò | Dữ liệu nghiệp vụ (hóa đơn) |
|-----|---------|------------------------------|
| **Root** | Node sau lần cài đặt đầu. Quản trị toàn bộ Tenant và Org. | Không giữ |
| **Tenant** | Vùng dữ liệu của một khách nền tảng. Có thể có Tenant con. | Không giữ bản ghi hóa đơn. Quản lý cây Tenant con và các Org trong vùng của mình |
| **Org** | Người bán đứng trên hóa đơn. Tenant tạo Org. | Giữ bộ hóa đơn của chính Org đó |
| **OrgStructure** | Cây tổ chức bên trong một Org: phòng ban, và với VNA là chi nhánh, phòng vé, đại lý khi các node đó không tạo Org khách | Không giữ. Hóa đơn vẫn thuộc Org cha |

Một node chỉ tạo node ở cấp kế dưới:

```text
Root tạo Tenant
Tenant tạo Tenant con hoặc Org
Org tạo OrgStructure
```

OrgStructure không tạo Org. Đơn vị phải tự tạo Org cho khách thì đơn vị đó là **Tenant** (có thể là Tenant con), không phải node OrgStructure.

## Khi nào có Tenant con

Tenant con xuất hiện khi một bộ phận phải có vùng Org riêng và tự tạo Org khách, trong khi Tenant cha vẫn quản trị nó.

Ví dụ: chi nhánh HCM của Minvoice tạo Org cho khách của HCM. HCM là Tenant con của Tenant Minvoice. Khách của HCM là Org nằm trong Tenant HCM.

Chi nhánh VNA không tạo Org khách. Hóa đơn vẫn thuộc Org VNA. Chi nhánh VNA là node OrgStructure.

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
│   ├── Org EPAY
│   │     └── OrgStructure: phòng ban EPAY
│   └── Org Khách cuối
│         └── OrgStructure: phòng ban của khách
│
└── Tenant VNA
      └── Org VNA                           ← mọi hóa đơn VNA thuộc Org này
            └── OrgStructure
                  ├── Chi nhánh
                  │     └── Phòng ban của chi nhánh
                  ├── Phòng vé
                  └── Đại lý
```

Tenant Minvoice, EPAY và VNA ngang nhau dưới Root. Tenant HCM và HN ngang nhau dưới Tenant Minvoice.

Hóa đơn của khách thuộc Org khách. Tenant cha cung cấp dịch vụ và quản trị cây Tenant, không giữ bản ghi hóa đơn của Org trong Tenant con.

## Bảy tình huống Minvoice / EPAY / VNA

| # | Tình huống | Vị trí |
|---|------------|--------|
| 1 | HCM, HN là kênh bán; hóa đơn khách do Minvoice cung cấp dịch vụ | **Tenant con** của Tenant Minvoice. Không phải OrgStructure của Org Minvoice |
| 2 | Khách của HCM, HN hoặc của đại lý | **Org** trong Tenant của kênh đó (Tenant HCM, Tenant HN hoặc Tenant đại lý) |
| 3 | Khách do Minvoice bán trực tiếp | **Org** trong Tenant Minvoice, cạnh Org Minvoice |
| 4 | Minvoice, HCM, HN có phòng ban | OrgStructure của **Org Minvoice**, **Org Chi nhánh HCM**, **Org Chi nhánh HN** — mỗi Org một cây |
| 5 | EPAY có phòng ban | OrgStructure của **Org EPAY** |
| 6 | Khách cuối có phòng ban | OrgStructure của **Org khách** |
| 7 | Chi nhánh VNA có phòng ban | OrgStructure của **Org VNA**: phòng ban là con của node chi nhánh |

## Hiện trạng so với ADR

| Việc trong mô hình | ADR | Code |
|--------------------|-----|------|
| Tenant gốc và Tenant con | [ADR Tenants](../../ADRs/2026-08-07_platform_tenants-module.md) T11 | Chưa có `ParentTenantId` |
| Org là người bán, không có Org cha | [ADR Orgs](../../ADRs/2026-10-17_platform_orgs-module.md) | Catalog Org một cấp đã có |
| Cây trong một Org; VNA dùng `Branch` | [ADR OrgStructure](../../ADRs/2026-10-17_platform_orgstructure-module.md) | Module chưa code |
| Sự kiện sau khi ghi Org | [ADR catalog events](../../ADRs/2026-10-21_platform_orgs-catalog-events.md) | Type và publish trong `OrgManager` đã có; test publish chưa đủ |
