# Archive — Tài liệu cũ, rời rạc

Nơi chứa tài liệu **từ trước khi dự án dùng minipower**: file Word/PDF cũ, spec thất lạc ngữ cảnh, wiki nội bộ, ảnh chụp whiteboard, tài liệu bàn giao từ đội cũ.

**Đây là NGUỒN THAM CHIẾU, không phải artifact.** Không tài liệu nào ở đây được coi là đúng cho tới khi có người xác nhận. Đường đi đúng là:

```text
assets/archive/  →  (khảo sát + xác nhận)  →  docs/
```

Không copy thẳng nội dung từ đây vào `docs/`. Mỗi mục kéo sang phải có người xác nhận còn đúng, và có ID để trace.

> Chủ yếu dùng ở chế độ **`maintain`** (tiếp quản hệ cũ). Chế độ khác vẫn dùng được khi khách hàng gửi tài liệu cũ.

## Chỉ mục nguồn

Cột **Độ tin cậy** là thứ quyết định thứ tự khai quật — đọc cái *"còn đúng"* trước, cái *"đã lỗi thời"* chỉ đọc khi cần hiểu vì sao hệ hiện tại như vậy.

| File | Nguồn / ai đưa | Ngày | Độ tin cậy | Ghi chú |
|------|----------------|------|------------|---------|
| | | | ☐ còn đúng · ☐ nghi ngờ · ☐ đã lỗi thời | |

**Quy ước độ tin cậy:**

| Mức | Nghĩa | Dùng thế nào |
|-----|-------|--------------|
| **Còn đúng** | Có người xác nhận khớp hệ đang chạy | Khai quật trước, trích được ngay |
| **Nghi ngờ** | Không ai chắc; chưa đối chiếu code | Đọc được, nhưng **mọi** điều rút ra phải kiểm lại với code/người |
| **Đã lỗi thời** | Biết chắc đã sai so với hệ hiện tại | **Không** trích vào `docs/`; chỉ dùng để hiểu lịch sử quyết định |

Chưa rõ thì ghi **nghi ngờ** — mặc định an toàn. Nhận nhầm "còn đúng" là đưa thông tin sai vào tài liệu chính thức.

## Quy ước

- **Không sửa file gốc.** Sai thì ghi chú ở cột Ghi chú, đừng chữa vào file.
- Giữ nguyên tên gốc nếu còn ý nghĩa; tên vô nghĩa (`final_v3_thatsu.docx`) thì thêm tiền tố ngày: `2019-05-ten-mo-ta.docx`.
- File quá lớn / nhị phân không đọc được → ghi vào bảng kèm chỗ lưu thật, đừng nhét vào repo.
