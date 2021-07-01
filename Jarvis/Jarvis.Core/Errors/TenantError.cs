namespace Jarvis.Core.Errors
{
    public class TenantError : IError
    {
        public static ErrorInfo MaSoThueDaBiTrung = new ErrorInfo(4001, "Mã số thuế đã bị trùng");
        public static ErrorInfo TenMienDaBiTrung = new ErrorInfo(4002, "Tên miền đã bị trùng");
        public static ErrorInfo TenDangNhapKhongHopLe = new ErrorInfo(4003, "Tên tài khoản không hợp lệ. Vui lòng nhập tên tài khoản không phải là admin");
        public static ErrorInfo KhongSuaChiNhanhDaHoatDong = new ErrorInfo(4004, "Công ty/Chi nhánh đã có đăng ký phát hành không thể sửa");
        public static ErrorInfo KhongXoaChiNhanhDaHoatDong = new ErrorInfo(4005, "Công ty/Chi nhánh đã có đăng ký phát hành không thể xóa");

    }
}
