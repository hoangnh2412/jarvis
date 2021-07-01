namespace Jarvis.Core.Errors
{
    public class ErrorDefaults : IError
    {
        public static ErrorInfo ChuaXacDinh = new ErrorInfo(-1, "Chưa xác định");
        public static ErrorInfo ThanhCong = new ErrorInfo(0, "Thành công");
        public static ErrorInfo KhongCoQuyen = new ErrorInfo(2, "Không có quyền");
        public static ErrorInfo KhongCoThongTinCongTy = new ErrorInfo(16, "Không có thông tin công ty");
        public static ErrorInfo KhongTimThayServer = new ErrorInfo(19, "Không tìm thấy Server");
        public static ErrorInfo DinhDangDuLieu = new ErrorInfo(20, "Định dạng dữ liệu không đúng");

        public static ErrorInfo BanChuaDangNhap = new ErrorInfo(1006, "Bạn chưa đăng nhập, vui lòng đăng nhập lại!");
    }
}
