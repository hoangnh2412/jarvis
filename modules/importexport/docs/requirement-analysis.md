# Phân tích yêu cầu và traceability

## Phạm vi GĐ1 đã triển khai

| Yêu cầu | Hiện thực |
|---|---|
| BR-001 Import tối ưu RAM | `MiniExcelImportProcessor` dùng `QueryAsync` và duyệt từng row, không nạp workbook vào model riêng của engine. |
| BR-002 Export tối ưu RAM | `MiniExcelExportProcessor` project từng item `IEnumerable<T>` sang dictionary trong iterator; không gọi `ToList()` đầu vào. Kết quả cuối cùng vẫn là `byte[]` theo contract GĐ1. |
| BR-003 Mapping động | `ColumnMapping(ColumnName, FieldName)`, validate rỗng/trùng; cùng mapping dùng cho import và export. |
| BR-004 StartRow/RowCount | `StartRow`, `HeaderRow`, `RowCount`, `SheetName` nằm trong `TemplateConfiguration`. |
| BR-005 API + DLL | API gọi `IImportExportFacade`; DLL phát hành tách Contracts/Core/Infrastructure. |
| BR-006 Sync | `SyncExecutionExecutor` là executor duy nhất được đăng ký. Kết quả import/export trả ngay trong request. |
| BR-007 Design-ready | `IExecutionExecutor` trong Application tách khỏi processor/facade; processor không biết sync/async. |
| NFR logging/bảo mật | API yêu cầu API key. Logging host có sẵn qua ASP.NET; triển khai production nên thêm structured audit log/metrics theo hạ tầng. |

## Quyết định kỹ thuật quan trọng

1. Không dùng reflection để import sang model nghiệp vụ. Import trả `IReadOnlyDictionary<string, object?>`, nên DLL không phụ thuộc model của từng consumer. Consumer tự map dictionary sang DTO/entity theo domain của mình.
2. Không trả `Stream` qua REST. `ExportResult` dùng `byte[]` đúng yêu cầu GĐ1; controller trả `FileContentResult`. Nếu file rất lớn, GĐ2 chuyển sang object storage/job result thay vì giữ byte[] trong process.
3. `StartRow` là dòng dữ liệu đầu tiên, theo số dòng Excel bắt đầu từ 1. Nếu template có dòng mô tả, truyền rõ `HeaderRow`; nếu bỏ trống thì header mặc định là `StartRow - 1`.
4. Với mapping/header không tìm thấy, GĐ1 ghi `ImportRowError`. `Continue` trả các row hợp lệ còn lại; `Stop` dừng ngay tại lỗi. Đây là chuẩn xử lý lỗi thống nhất cho API và DLL.

## GĐ2 không phá vỡ contract

Thêm các thành phần sau, không đổi `IImportProcessor`, `IExportProcessor`, mapping hoặc endpoint sync:

```
Async API/DLL -> AsyncExecutor -> JobStore -> BackgroundWorker -> existing Processor -> MiniExcel
                              |-> Job status/progress/result API
```

Khuyến nghị thêm API riêng `POST /api/v1/excel/jobs/import|export` trả `jobId`; `GET /api/v1/excel/jobs/{jobId}`; `GET /api/v1/excel/jobs/{jobId}/result`. Không chuyển endpoint sync hiện hữu sang async để tránh thay đổi hành vi consumer.

## Việc cần quyết định trước production

* Ngưỡng tối đa file, timeout request và quota đồng thời (NFR-002/NFR-003).
* Cơ chế xác thực thật: API key hiện là adapter tối thiểu; có thể thay bằng JWT/service-to-service identity của.
* Chính sách dữ liệu rỗng export: phiên bản này để MiniExcel quyết định file rỗng; nếu nghiệp vụ bắt buộc "headers only", cần thống nhất mẫu file/format và thêm test acceptance.
* Kiểu dữ liệu/validation theo từng nghiệp vụ (Price decimal, Code bắt buộc, v.v.) nên đặt ở consumer/domain validator hoặc một `IRowValidator` extension, không hard-code vào generic engine.
