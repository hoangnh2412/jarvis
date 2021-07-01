
namespace Jarvis.Core.Errors
{
    public class IdentityError : IError
    {
        public static ErrorInfo EmailKhongDuocDeTrongKhiPasswordTuDong  = new ErrorInfo(1001, "Email tài khoản không được để trống khi chọn password tự động");
        public static ErrorInfo TaiKhoanMatKhauKhongDung  = new ErrorInfo(1002, "Tài khoản hoặc mật khẩu không đúng");
        public static ErrorInfo TaiKhoanBiKhoa  = new ErrorInfo(1003, "Tài khoản bị khóa");
        public static ErrorInfo KhongTimThayCongTy = new ErrorInfo(1004, "Không tìm thấy thông tin công ty");
        public static ErrorInfo TaiKhoanKhongTonTai = new ErrorInfo(1005, "Tài khoản không tồn tại");
        public static ErrorInfo TaiKhoanKhongCoEmail = new ErrorInfo(1006, "Tài khoản không có email. Vui lòng liên hệ công ty/chi nhánh để cấp lại mật khẩu");
        public static ErrorInfo EmailKhongTrungVoiEmailTaiKhoan = new ErrorInfo(1007, "Email không trùng với email của tài khoản. Vui lòng nhập đúng email của tài khoản");
        public static ErrorInfo TaiKhoanDaBiDoiMatKhau = new ErrorInfo(1008, "Tài khoản đã được đổi mật khẩu. Thời gian đổi mật khẩu đã hết hạn");
        public static ErrorInfo TaiKhoanDaTonTai = new ErrorInfo(1009, "Tài khoản đã tồn tại");
        public static ErrorInfo MatKhauCuKhongDung = new ErrorInfo(1010, "Mật khẩu cũ không đúng");
    }
}
