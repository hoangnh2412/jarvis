# Chuyển Workflow Core Persistence từ PostgreSQL sang SQL Server

Tài liệu này mô tả các bước chuyển persistence database của Elsa Workflow Core trong project.

## Phạm vi hiện tại

Workflow Core đang cấu hình PostgreSQL trực tiếp trong:

`frameworks/Platform.Workflow/Extensions/ElsaWorkflowHostBuilderExtensions.cs`

Provider được sử dụng cho cả hai phần:

- Workflow Management
- Workflow Runtime

## Các bước chuyển sang SQL Server

### 1. Kiểm tra package Elsa SQL Server

Project hiện đang dùng Elsa `3.7.1` và package PostgreSQL:

```xml
<PackageReference Include="Elsa.Persistence.EFCore" Version="3.7.1" />
<PackageReference Include="Elsa.Persistence.EFCore.PostgreSql" Version="3.7.1" />
```

Package khi chuyển sang SQL Server:

```xml
<PackageReference Include="Elsa.Persistence.EFCore.SqlServer" Version="3.7.1" />
```
Tên package thực tế cần xác nhận theo package/API của phiên bản Elsa đang sử dụng.

### 2. Thay provider trong `ElsaWorkflowHostBuilderExtensions.cs`

Hiện tại cả Management và Runtime dùng PostgreSQL:

```csharp
PostgreSqlProvidersExtensions.UsePostgreSql(ef, connectionString, dbContextOptions);
```

Thay cả hai lời gọi bằng API SQL Server tương ứng, ví dụ:

```csharp
SqlServerProvidersExtensions.UseSqlServer(ef, connectionString, dbContextOptions);
```

Cần thay ở cả hai vị trí:
```csharp
.UseWorkflowManagement(...)
```

và:

```csharp
.UseWorkflowRuntime(...)
```

Không được chỉ thay một trong hai phần.

### 3. Cập nhật connection string

Giữ nguyên tên connection string đang được cấu hình bởi:

```json
"Elsa": {
  "ConnectionStringName": "MasterDbContext"
}
```

Chỉ thay giá trị connection string:

```json
"ConnectionStrings": {
  "MasterDbContext": "Server=localhost;Database=ElsaWorkflow;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
}
```

Hoặc dùng Windows Authentication:

```json
"ConnectionStrings": {
  "MasterDbContext": "Server=localhost;Database=ElsaWorkflow;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 4. Tạo database và schema trên SQL Server

- Tạo database `ElsaWorkflow` trên SQL Server.
- Đảm bảo user trong connection string có quyền tạo bảng, index và migration.
- Kiểm tra schema Elsa đang dùng trong cấu hình:

```json
"Elsa": {
  "PersistenceSchema": "elsa"
}
```

### 5. Tạo migration riêng cho SQL Server

Không dùng trực tiếp migration PostgreSQL cho SQL Server.

Cần tạo hoặc chạy bộ migration riêng cho SQL Server vì có thể khác nhau về:

- Kiểu dữ liệu.
- Identity/sequence.
- JSON column.
- Index.
- Default value.
- Cách tạo schema.

Nếu Elsa provider tự quản lý migration, bật/chạy migration theo hướng dẫn của package SQL Server. Nếu project quản lý migration riêng, tạo migration mới với SQL Server provider.

### 6. Kiểm tra các phần còn phụ thuộc PostgreSQL

Sau khi đổi Elsa persistence, kiểm tra thêm:

- Các lệnh `UseNpgsql` hoặc `UsePostgreSql` khác.
- Package `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Migration của các DbContext nghiệp vụ.
- Health check PostgreSQL.
- SQL raw query dùng cú pháp riêng của PostgreSQL như `jsonb`, `ILIKE`, `ON CONFLICT` hoặc `uuid`.

Việc thay provider trong Elsa chỉ ảnh hưởng Workflow Management và Workflow Runtime; các DbContext khác cần chuyển riêng nếu chúng vẫn dùng PostgreSQL.

## Kết quả mong muốn

Sau khi hoàn tất, code Elsa vẫn giữ nguyên cấu trúc:

```csharp
.UseWorkflowManagement(...)
.UseWorkflowRuntime(...)
```

Chỉ thay package provider, hai lời gọi provider và connection string. Cấu hình Embedded và Standalone của Workflow Core không thay đổi; trong mô hình Standalone, chỉ Workflow Server cần kết nối tới SQL Server.

## Kiểm tra sau khi chuyển

1. Restore package thành công.
2. Build solution thành công.
3. Elsa tạo hoặc chạy được migration trên SQL Server.
4. Tạo được workflow definition.
5. Chạy được workflow instance.
6. Kiểm tra được workflow runtime data như bookmark, inbox và execution log.
7. Kiểm tra cả Workflow Management và Workflow Runtime đều dùng cùng SQL Server database/schema theo thiết kế.
