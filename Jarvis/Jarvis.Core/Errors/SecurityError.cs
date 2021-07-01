using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Errors
{
    public class SecurityError : IError
    {
        public static ErrorInfo KhongCoQuyen = new ErrorInfo(2, "Không có quyền");

        public static ErrorInfo TaiKhoanKhongTonTai = new ErrorInfo(2001, "Tài khoản không tồn tại");
        public static ErrorInfo TaiKhoanHoacMatKhauKhongDung = new ErrorInfo(2002, "Tài khoản hoặc mật khẩu không đúng");
        public static ErrorInfo TaiKhoanBiKhoa = new ErrorInfo(2003, "Tài khoản đã bị khóa vui lòng thử lại sau 30 phút");
        public static ErrorInfo ChuSoHuuKhongTonTai = new ErrorInfo(2004, "Chủ sở hữu không tồn tại");
        public static ErrorInfo TaiKhoanChuaDangNhap = new ErrorInfo(2005, "Tài khoản chưa đăng nhập");

        public static ErrorInfo KhongThayThongTinCongTy = new ErrorInfo(2006, "Không thấy thông tin công ty");
        public static ErrorInfo DomainDaBiTrung = new ErrorInfo(2007, "Domain đã bị trùng");
        public static ErrorInfo MaSoThueDaDuocSuDungBoiCongTyKhac = new ErrorInfo(2008, "Mã số thuế đã được sử dụng bởi công ty khác");
        public static ErrorInfo EmailDaDuocSuDungBoiCongTyKhac = new ErrorInfo(2009, "Email đã được sử dụng bởi công ty khác");
        public static ErrorInfo KhongThayQuyen = new ErrorInfo(2010, "Không thấy quyền");

        public static ErrorInfo DoiMatKhauThatBai = new ErrorInfo(2011, "Đổi mật khẩu thất bại");
        public static ErrorInfo MaBaoMatKhongDung = new ErrorInfo(2012, "Mã bảo mật không đúng");
        public static ErrorInfo KhongThayMauEmailResetPassword = new ErrorInfo(2014, "Không tìm thấy mẫu email thiết lập mật khẩu");
        public static ErrorInfo KhongCoEmailNguoiNhan = new ErrorInfo(2015, "Không có email người nhận");
        public static ErrorInfo MatKhauKhongKhop = new ErrorInfo(2016, "Mật khẩu cũ không đúng");

    }
}
