using System.Collections.Generic;

namespace Jarvis.Core.Errors
{
    public static class ErrorStore
    {
        //public static Dictionary<ErrorCodes.ErrorDefault, string> VI = new Dictionary<ErrorCodes.ErrorDefault, string>
        //{
        //{ ErrorCodes.ErrorDefault.ChuaXacDinh, "Chưa xác định" },
        //{ ErrorCodes.ErrorDefault.ThanhCong, "Thành công" },
        //{ ErrorCodes.ErrorDefault.IdHoaDonDaTonTai, "Id hóa đơn đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.TrangThaiHoaDonKhongTonTai, "Trạng thái hóa đơn không tồn tại" },
        //{ ErrorCodes.ErrorDefault.ChuaXacDinhDuocLoiKhiHuyHoaDon, "Chưa xác định được lỗi khi hủy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChuaXacDinhDuocLoiKhiSuaHoaDon, "Chưa xác định được lỗi khi sửa hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChuaXacDinhDuocLoiKhiTaoHoaDon, "Chưa xác định được lỗi khi tạo hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChuaXacDinhDuocLoiKhiThayTheHoaDon, "Chưa xác định được lỗi khi thay thế hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChuaXacDinhDuocLoiKhiXoaHuyHoaDon, "Chưa xác định được lỗi khi xóa hủy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongDieuChinhTangGiamDuocHoaDon, "Không điều chỉnh tăng giảm được hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongDieuChinhTangGiamDuocHoaDonChuaKy, "Không điều chỉnh tăng giảm được hóa đơn chưa kí" },
        //{ ErrorCodes.ErrorDefault.KhongDuocThayTheHoaDon, "Không được thay thế hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheHoaDonChuaKy, "Không thể thay thế hóa đơn chưa kí" },
        //{ ErrorCodes.ErrorDefault.KhongHuyDuocHoaDonDaKy, "Không hủy được hóa đơn đã kí" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon, "Không tìm thấy mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy, "Không có thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.MaBaoMatKhongDung, "Mã bảo mật không đúng" },
        //{ ErrorCodes.ErrorDefault.DinhDangDuLieu, "Định dạng dữ liệu không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayServer, "Không tìm thấy Server" },
        //{ ErrorCodes.ErrorDefault.KhongDuocDuyetHoaDon, "Không được duyệt hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HinhThucThanhToanKhongDung, "Phương thức thanh toán không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongTheDuyetVaKyHoaDon, "Không thể duyệt và ký hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongCoHoaDonSanSangDuyetVaKi, "Không có hóa đơn sẵn sàng duyệt và ký" },

        //{ ErrorCodes.ErrorDefault.HoaDonDaDuocTaoTrongHeThong , "Hóa đơn đã được tạo trong hệ thống" },


        //{ ErrorCodes.ErrorDefault.GiaTriIdFieldExtraKhongTonTai, "Giá trị id field extra không tồn tại" },
        //{ ErrorCodes.ErrorDefault.GiaTriFieldNameExtraKhongTonTai, "Giá trị fieldName extra không tồn tại" },

        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTyHienTai, "Không tìm thấy thông tin công ty hiện tại" },
        //{ ErrorCodes.ErrorDefault.DonViBanHangChuaCaiDatNguyenTe, "Đơn vị bán hàng chưa cài đặt nguyên tệ" },

        //{ ErrorCodes.ErrorDefault.KhongDuocSuaHoaDon, "Không được sửa hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongSuaHoaDonDaKy, "Không được sửa hóa đơn đã ký" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDungDinhDang, "Email không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung, "Ngày hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.NgayBienBanKhongDung, "Ngày biên bản không đúng" },

        //{ ErrorCodes.ErrorDefault.HoaDonDaBiHuy, "Hóa đơn đã bị hủy" },
        //{ ErrorCodes.ErrorDefault.KhongChoPhepHuyHoaDon, "Không cho phép hủy hóa đơn" },

        //{ ErrorCodes.ErrorDefault.KhongTheTaoHoaDonChoDonViKhac, "Không thể tạo hóa đơn cho đơn vị khác" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheChoMauHoaDonKhacMauSo, "Không thể thay thế cho mẫu hóa đơn khác mẫu số" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheChoMauCoKyHieuLonHon, "Không thể thay thế cho mẫu có ký hiệu năm lớn hơn" },

        //{ ErrorCodes.ErrorDefault.KhongTimThayMauVaKiHieuHoaDon,  "Không tìm thấy mẫu và ký hiệu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhDinhDanhChoMauHoaDonKhacMauSo,  "Không thể điều chỉnh định danh cho mẫu hóa đơn khác mẫu số" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhDinhDanhChoMauCoKyHieuLonHon,  "Không thể điều chỉnh định danh cho mẫu hóa đơn có năm ký hiệu lớn hơn" },

        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhTangGiamChoMauHoaDonKhacMauSo,  "Không thể điều chỉnh tăng giảm cho mẫu hóa đơn khác mẫu số" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhTangGiamChoMauCoKyHieuLonHon,  "Không thể điều chỉnh tăng giảm cho mẫu hóa đơn có năm ký hiệu lớn hơni" },

        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonGoc,  "Không tìm thấy hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongDuocDieuChinhDinhDanhHoaDon,  "Trạng thái hóa đơn không tồn tại" },

        //{ ErrorCodes.ErrorDefault.KhongTimThayPhanTramThue,  "Không tìm thấy phần trăm thuế" },
        //{ ErrorCodes.ErrorDefault.MaTienTeKhongTonTai,  "Mã tiền tệ không tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonDeDuyet,  "Không tìm thấy hóa đơn để duyệt" },

        //{ ErrorCodes.ErrorDefault.QuaTrinhTaoSoHoaDonXayRaLoi,  "Quá trình tạo số hóa đơn xảy ra lỗi. Vui lòng kiểm tra lại ngày hóa đơn hoặc số đăng ký phát hành." },
        //{ ErrorCodes.ErrorDefault.FileBienBanKhongDuocDeTrong,  "File biên bản không được để trống" },

        //{ ErrorCodes.ErrorDefault.KhongThaoTacHoaDonKhongSo, "Không thao tác hóa đơn không số." },
        //{ ErrorCodes.ErrorDefault.InKhongQua10HoaDon, "Không tìm thấy hóa đơn" },

        //{ ErrorCodes.ErrorDefault.LicenHetHanHoacChuaDangKyLicense, "License hết hạn hoặc chưa đăng ký license" },


        //{ ErrorCodes.ErrorDefault.MauHoaDonKhongPhai01GTKT, "Mẫu hóa đơn không phải 01GTKT" },
        //{ ErrorCodes.ErrorDefault.IdTenantPhaiGiongNhau, "IdTenant của các hóa đơn phải giống nhau" },

        //{ ErrorCodes.ErrorDefault.GiaTriIdCongTyIdTenantKhongDung, "Giá trị Id công ty(IdTenant) không đúng" },
        //{ ErrorCodes.ErrorDefault.MauHoaDonKhongPhai02GTTT, "Mẫu hóa đơn không phải 02GTTT" },
        //{ ErrorCodes.ErrorDefault.MauHoaDonKhongPhai03XKNB, "Mẫu hóa đơn không phải 03XKNB" },

        ////dang nhap
        //{ ErrorCodes.ErrorDefault.BanChuaDangNhap, "Bạn chưa đăng nhập, đăng nhập lại." },
        //{ ErrorCodes.ErrorDefault.DuLieuDauVaoKhongDung, "Dữ liệu đầu vào không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanDaBiKhoa, "Tài khoản đã bị khóa" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanHoacMatKhauKhongDung, "Tài khoản hoặc mật khẩu không đúng" },
        //{ ErrorCodes.ErrorDefault.DomainDangNhapKhongDuocDeTrong1003, "Domain name đăng nhập không được để tr" },

        ////tạo hóa đơn
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiTaoHoaDon, "Có lỗi xảy ra trong quá trình tạo hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon4000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTyHienTai4000, "Không tìm thấy thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon4000, "Không tìm thấy mẫu hóa đơn và kí hiệu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon4000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DonViBanHangChuaCaiDatNguyenTe4000, "Đơn vị bán hàng chưa cài đặt nguyên tệ" },
        //{ ErrorCodes.ErrorDefault.KhongCoKhachHangTrongNhom4000, "Không có khách hàng trong nhóm" },
        //{ ErrorCodes.ErrorDefault.DuLieuErpDaTonTai4000,"Dữ liệu đã tồn tại ở erp" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung4000, "Ngày hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayLoaiHoaDon4000, "Không Tìm thấy loại hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaTonTai4000, "Hóa đơn đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy4013, "Không có thông tin công ty" },

        ////sửa hóa đơn
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen5000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon5000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon5000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongDuocSuaHoaDon5000, "Không được sửa hóa đơn" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiSuaHoaDon, "Có lỗi xảy ra trong quá trình sửa hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DonViBanHangChuaCaiDatNguyenTe5000, "Đơn vị bán hàng chưa cài đặt nguyên tệ" },
        //{ ErrorCodes.ErrorDefault.KhongXacDinhDuocTrangThaiHoaDon5000, "Không xác định được trạng thái hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTyHienTai5000, "Không tìm thấy thông tin công ty hiện tại" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDungDinhDang5009, "Email không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung5010, "Ngày hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.NgayBienBanKhongDung5011, "Ngày biên bản phải nhỏ hơn hoặc bằng ngày hóa đơn sửa, lớn hơn hoặc bằng ngày hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongSuaHoaDonDaKy5012, "Không được sửa hóa đơn đã ký" },

        ////hủy hóa đơn
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen6000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon6000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon6000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongChoPhepHuyHoaDon6000, "Không cho phép hủy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiXoaHuyHoaDon6000, "Có lỗi xảy ra trong quá trình xóa hủy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaBiHuy6000, "Hóa đơn đã bị hủy, không thể hủy tiếp" },

        ////thay thế hóa đơn
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen9000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiThayTheHoaDon9000, "Có lỗi xảy ra trong quá trình thay thế hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon9000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon9000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonGoc9000, "Không tìm thấy hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon9000, "Không tìm thấy mẫu hóa đơn và kí hiệu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DuLieuErpDaTonTai9000, "Dữ liệu đã tồn tại ở erp" },
        //{ ErrorCodes.ErrorDefault.HoaDonChuaKiKhongDuocThayThe9000, "Không thay thế được hóa đơn chưa kí" },
        //{ ErrorCodes.ErrorDefault.KhongTheTaoHoaDonChoDonViKhac9000, "Không thể thay thế hóa đơn cho đơn vị khác" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTyHienTai9000, "Không tìm thấy thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung9012, "Ngày hóa đơn thay thế không được nhỏ hơn ngày hóa đơn lớn nhất(cùng mẫu) và không được lớn hơn ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDungDinhDang9013, "Email không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheHoaDonKhongPhaiHoaDonGoc9014, "Không thể thay thế cho hóa đơn không phải hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheChoMauCoKyHieuLonHon9015, "Không thể thay thế cho mẫu hóa đơn có năm khởi tạo lớn hơn" },
        //{ ErrorCodes.ErrorDefault.KhongTheThayTheChoMauHoaDonKhacMauSo9016, "Không thể thay thế cho mẫu hóa đơn khác mẫu sơ" },

        ////điều chỉnh định danh
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen10000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon10000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTy10000, "Không tìm thấy thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon10000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonGoc10000, "Không tìm thấy hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauVaKiHieuHoaDon10000, "Không tìm thấy mẫu hóa đơn và kí hiệu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiDieuChinhDinhDanhHoaDon10000, "Có lỗi xảy ra trong quá trình điều chỉnh định danh hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DuLieuDaTonTaiOErp10000, "Dữ liệu đã tồn tại ở erp" },
        //{ ErrorCodes.ErrorDefault.HoaDonChuaKiKhongDuocDieuChinh10000, "Không điều chỉnh được hóa đơn chưa kí" },
        //{ ErrorCodes.ErrorDefault.KhongTheTaoHoaDonChoDonViKhac10000, "Không thể điều chỉnh hóa đơn cho đơn vị khác" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung10011, "Ngày hóa đơn điều chỉnh không được nhỏ hơn ngày hóa đơn lớn nhất(cùng mẫu) và không được lớn hơn ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDungDinhDang10012, "Email không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonPhaiNhoHoNgayHienTai10013, "Ngày hóa đơn phải nhỏ hơn ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhDinhDanhHoaDonKhongPhaiHoaDonGoc10014, "Không thể điều chỉnh định danh cho hóa đơn không phải là hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhDinhDanhChoMauCoKyHieuLonHon10015, "Không thể điều chỉnh định danh cho mẫu hóa đơn có năm khởi tạo lớn hơn" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhDinhDanhChoMauHoaDonKhacMauSo10016, "Không thể điều chỉnh định danh cho mẫu hóa đơn khác mẫu sơ" },

        ////điều chỉnh tăng giảm
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen11000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon11000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTy11000, "Không tìm thấy thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon11000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonGoc11000, "Không tìm thấy hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon11000, "Không tìm thấy mẫu hóa đơn và kí hiệu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiDieuChinhTangGiamHoaDon11000, "Có lỗi xảy ra trong quá trình điều chỉnh tăng giảm hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DuLieuErpDaTonTai11000, "Dữ liệu đã tồn tại ở erp" },
        //{ ErrorCodes.ErrorDefault.HoaDonChuaKiKhongDuocDieuChinh11000, "Không điều chỉnh được hóa đơn chưa kí" },
        //{ ErrorCodes.ErrorDefault.KhongTheTaoHoaDonChoDonViKhac11000, "Không thể điều chỉnh hóa đơn cho đơn vị khác" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDung11011, "Email không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonKhongDung11012, "Ngày hóa đơn điều chỉnh không được nhỏ hơn ngày hóa đơn lớn nhất(cùng mẫu) và không được lớn hơn ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhTangGiamHoaDonKhongPhaiHoaDonGoc11013, "Không thể điều chỉnh tăng giảm cho hóa đơn không phải là hóa đơngốc" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhTangGiamChoMauCoKyHieuLonHon11014, "Không thể điều chỉnh tăng giảm cho mẫu hóa đơn có năm khởi tạo lớn hơn" },
        //{ ErrorCodes.ErrorDefault.KhongTheDieuChinhTangGiamChoMauHoaDonKhacMauSo11015, "Không thể điều chỉnh tăng giảm cho mẫu hóa đơn khác mẫu sơ" },

        ////xoa bo hoa don
        //{ ErrorCodes.ErrorDefault.KhongCoQuyen12000, "Không có quyền" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaKhiXoaBoHoaDon12000, "Có lỗi xảy ra trong quá trình xóa bỏ hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon12000, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThaySoHoaDon12000, "Không tìm thấy số hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongDuocXoaHoaDon12005, "Không được xóa bỏ hóa đơn" },
        //{ ErrorCodes.ErrorDefault.NgayBienBanPhaiNhoHonNgayHoaDon12006, "Ngày biên bản phải nhỏ hơn hoặc bằng ngày hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonChuaKyKhongDuocXoaBo12007, "Hóa đơn chưa ký không được xóa bỏ" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaXoaBo12008, "Hóa đơn đã bị xóa bỏ" },

        ////tao moi loai san pham (productType)
        //{ ErrorCodes.ErrorDefault.MaLoaiSanPhamDaTonTai13001, "Mã loại sản phẩm đã tồn tại" },

        ////xoa loai san pham
        //{ ErrorCodes.ErrorDefault.DaCoDuLieuSanPhamThamChieuToiLoaiSanPhamNay13003, "Đã có dữ liệu sản phẩm tham chiếu tới loại sản phẩm này" },
        //{ ErrorCodes.ErrorDefault.LoaiSanPhamDangDuocSuDung13004, "Loại sản phẩm đang được sử dụng" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy13006, "Không có thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayLoaiSanPham13007, "Khong tim thay loai san pham" },

        ////Sua  loai san phẩm
        //{ ErrorCodes.ErrorDefault.KhongTimThayMaLoaiSanPham13002, "Không tìm thấy mã loại sản phẩm" },

        ////san pham
        //{ ErrorCodes.ErrorDefault.KhongTimThaySanPham14000, "Không tìm thấy sản phẩm" },
        //{ ErrorCodes.ErrorDefault.MaSanPhamDaTonTai14000, "Mã sản phẩm đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.DonViThueKhongTonTai14000, "Đơn vị thuế không tồn tại" },
        //{ ErrorCodes.ErrorDefault.DaCoDuLieuSanPhamThamChieuToiSanPhamNay14000, "Đã có dữ liệu tham chiếu tới sản phẩm này" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy14005, "Không có thông tin công ty hiện tại" },

        ////sign
        //{ ErrorCodes.ErrorDefault.KyLoi, "Ký lỗi"},
        //{ ErrorCodes.ErrorDefault.KyKhongCoChungThuSo, "Không có chứng thư số" },
        //{ ErrorCodes.ErrorDefault.KyUsbBusy, "Usb bận" },
        //{ ErrorCodes.ErrorDefault.CoLoiTrongQuaTrinhKy, "Có lỗi trong quá trình ký" },
        //{ ErrorCodes.ErrorDefault.ChungThuSoChuaDuocGanSuDungMauHoaDon, "Chứng thư số chưa được gán sử dụng mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.MaSoThueNguoiBanKhacMaSoThueUsbToken, "Mã số thuế người bán khác mã số thuế UsbToken" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon15007, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.DinhDangDuLieuKhongDung15008, "Định dạng dữ liệu không đúng" },

        ////sign 03
        //{ ErrorCodes.ErrorDefault.HoaDonDaDuocKyTrongHeThong15013, "Hóa đơn đã được ký trong hệ thống" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanChuaDuocGanSuDungMauHoaDon15014, "Tài khoản chưa được gán sử dụng mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChungThuSoChuaDuocGanSuDungMauHoaDon15015, "Chứng thư số chưa được gán sử dụng mẫu hóa đơn" },


        ////sign04
        //{ ErrorCodes.ErrorDefault.KhongCoHoaDonDeKy15016, "Không có hóa đơn để ký" },
        //{ ErrorCodes.ErrorDefault.KhongTheDangNhapVaoServer15017, "Không thể đăng nhập vào server ký" },
        //{ ErrorCodes.ErrorDefault.KhongCoCauHinhUrlServerKy15018, "Không có cấu hình url server ký" },
        //{ ErrorCodes.ErrorDefault.MatKhauServerKySai15019, "Mật khẩu server ký sai" },
        //{ ErrorCodes.ErrorDefault.KhongCoCauHinhThongTinServerKy15020, "Không có cấu hình thông tin server ký" },


        //{ ErrorCodes.ErrorDefault.KhongCoQuyen2002, "Không có quyền cài đặt hệ thống" },
        //{ ErrorCodes.ErrorDefault.HeThongDaDuocCaiDat2001, "Hệ thống đã được cài đặt" },


        ////tao hoa don khi tich hop
        //{ ErrorCodes.ErrorDefault.KhongCoChiTietHoaDon15001, "Không có chi tiết hóa đơn" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonLonHonNgayHienTai15002, "Ngày hóa đơn phải nhỏ hơn hoặc bằng ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.MauHoaDonDaHetHan15005, "Mẫu hóa đơn đã hết hạn" },
        //{ ErrorCodes.ErrorDefault.KhongDuocLapHoaDonTruocNgayDangKyPhatHanh15006, "Không được lập hóa đơn trước ngày đăng kí phát hành" },

        ////tao mau hoa don
        //{ ErrorCodes.ErrorDefault.MauHoaDonDaTonTai16001, "Mẫu hóa đơn đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.LoaiHoaDonKhongTonTai16002, "Loại hóa đơn không tồn tại" },
        //{ ErrorCodes.ErrorDefault.NamCuaKiHieuMauHoaDonPhaiLaNamHienTaiHoacNamHienTaiCong1NeuThangHienTaiLa1216003, "Năm của kí hiệu mẫu hóa đơn phải là năm hiện tại hoặc năm hiện tại cộng 1 nếu tháng hiện tại là 12." },
        //{ ErrorCodes.ErrorDefault.KhongTheTaoMa16004, "Không thể tạo mã, có thể dữ liệu mã bản ghi trước sai định dạng (001)" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongty16005, "Không có thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.FileMauHoaDonQuaLon16006, "File mẫu hóa đơn quá lớn. Không quá 5MB" },

        ////sua mua hoa don
        //{ ErrorCodes.ErrorDefault.ChuaCoMauIn17001, "Chưa có mẫu in" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon17002, "Không tìm thấy mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.MauHoaDonDaThongBaoPhatHanhKhongTheSua17003, "Mẫu hóa đơn đã thông báo phát hành, không thể sửa" },
        //{ ErrorCodes.ErrorDefault.NamCuaKiHieuMauHoaDonPhaiLaNamHienTaiHoacNamHienTaiCong1NeuThangHienTaiLa1217004, "Năm của kí hiệu mẫu hóa đơn phải là năm hiện tại hoặc năm hiện tại cộng 1 nếu tháng hiện tại là 12." },
        //{ ErrorCodes.ErrorDefault.MauHoaDonDaTonTai17005, "Mẫu hóa đơn đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongty17006, "Không có thông tin công ty" },

        ////xoa mau hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon18001, "Không tìm thấy mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.MauHoaDonDaThongBaoPhatHanhKhongTheSua18002, "Mẫu hóa đơn đã thông báo phát hành, không thể xóa" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongty18003, "Không có thông tin công ty" },

        ////tim kiem loai hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayLoaiHoaDon19000, "Không tìm thấy loại hóa đơn" },


        ////sua loai hoa don
        //{ ErrorCodes.ErrorDefault.LoaiHoaDonDaTonTai19001, "Loại hóa đơn đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayLoaiHoaDon19002, "Không tìm thấy loại hóa đơn" },
        //{ ErrorCodes.ErrorDefault.LoaiHoaDonDaCoMauDuocSuDung19003, "Loại hóa đơn đã có mẫu được sử dụng!" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy19004, "Không có thông tin công ty" },

        ////tao dang ky phat hanh hoa don
        //{ ErrorCodes.ErrorDefault.ChuaChonMauHoaDonCanDangKy20001, "Chưa chọn mẫu hóa đơn cần đăng ký" },
        //{ ErrorCodes.ErrorDefault.TuSoPhaiNhoHonDenSo20002, "Từ số phải nhỏ hơn đến số" },
        //{ ErrorCodes.ErrorDefault.NgayLapDangKyPhatHanhHoaDonKhongDung, "Ngày tạo đăng ký phát hành hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.SoMauHoaDonKhongDung20004, "Số mẫu hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy20005, "Không có thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanKhongCoQuyenSuDungMauHoaDon20006, "Tài khoản này không được gán quyền sử dụng mẫu hóa đơn đăng ký phát hành" },
        //{ ErrorCodes.ErrorDefault.SoHoaDonDangKyKhongLienTiep20007, "Số hóa đơn đăng ký phát hành phải liên tiếp với lần đăng ký phát hành trước đó" },
        //{ ErrorCodes.ErrorDefault.NgaySuDungMauHoaDonKhongDung20008, "Ngày sử dụng mẫu hóa đơn không đúng" },
        //{ ErrorCodes.ErrorDefault.NgayDangKyPhatHanhHoaDonKhongDung20009, "Ngày đăng ký phát hành hóa đơn không đúng" },


        ////sua dang ky phat hanh hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayDangKyPhatHanh21001, "Không tìm thấy đăng ký phát hành" },
        //{ ErrorCodes.ErrorDefault.ThongBaoPhatHanhNayDaTaoHoaDonKhongTheChinhSua21002, "Thông báo phát hành này đã tạo hóa đơn, không thể chỉnh sửa" },
        //{ ErrorCodes.ErrorDefault.TuSoPhaiNhoHonDenSo21003, "Từ số phải nhỏ hơn đến số" },

        ////xoa dang ky phat hanh hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayDangKyPhatHanh22001, "Không tìm thấy đăng ký phát hành" },
        //{ ErrorCodes.ErrorDefault.ThongBaoPhatHanhNayDaTaoHoaDonKhongTheXoa22002, "Thông báo phát hành này đã tạo hóa đơn, không thể xóa" },

        ////tim kiem thong tin cong ty, chi nhanh
        //{ ErrorCodes.ErrorDefault.KhongThayThongTinCongTy23000, "Không tìm thấy thông tin công ty" },

        ////tao cong ty, chi nhanh
        //{ ErrorCodes.ErrorDefault.KhongThayThongTinCongTyCha23001, "Không thấy thông tin công ty cha" },
        //{ ErrorCodes.ErrorDefault.MaSoThueDaDuocSuDungBoiCongTyKhac23002, "Mã số thuế đã được sử dụng bởi công ty khác, vui lòng nhập mã số thuế khác" },
        //{ ErrorCodes.ErrorDefault.EmailDaDuocSuDungBoiCongTyKhac23003, "Email đã được sử dụng bởi công ty khác, vui lòng nhập email khác" },
        //{ ErrorCodes.ErrorDefault.TenMienKhongDuocDeTrong23000, "Tên miền không được để trống" },
        //{ ErrorCodes.ErrorDefault.DomainDaBiTrung23005, "Domain đã bị trùng" },

        ////sua cong ty, chi nhanh
        //{ ErrorCodes.ErrorDefault.MaSoThueDaDuocSuDungBoiCongTyKhac24001, "Mã số thuế đã được dùng bởi công ty khác" },
        //{ ErrorCodes.ErrorDefault.EmailDaDuocSuDungBoiCongTyKhac24002, "Email đã được dùng bởi công ty khác" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTy24003, "Không tìm thấy thông tin công ty" },

        ////xoa cong ty, chi nhanh
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinCongTy25001, "Không tìm thấy thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.CoDuLieuRangBuocKhongTheXoaThongTinCongTy25002, "Có dữ liệu ràng buộc không thể xóa thông tin công ty!" },
        //{ ErrorCodes.ErrorDefault.KhongTheXoaCongTyCuaTaiKhoanHienTai25003, "Không thể xóa công ty của tài khoản hiện tại" },

        ////in hoa don ban the hien
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon26001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn26002, "Hóa đơn không có mẫu in" },
        //{ ErrorCodes.ErrorDefault.ChuaCoHoaDonNaoDuocChon26003, "Chưa có hóa đơn nào được chọn" },

        ////in hoa don ban chuyen doi
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon27001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaInChuyenDoi27002, "Hóa đơn đã in chuyển đổi, không thể in lại lần 2" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn27003, "Hóa đơn không có mẫu in" },

        ////in nhieu hoa don ban the hien (kem bien ban tung hoa don neu co)
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon28001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn28002, "Hóa đơn không có mẫu in" },

        ////in nhieu hoa don ban chuyen doi (kem bien ban tung hoa don neu co)
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon29001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn29002, "Hóa đơn không có mẫu in" },

        ////in hoa don theo bang ke ban the hien
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon30001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn30002, "Hóa đơn không có mẫu in" },

        ////in hoa don theo bang ke ban chuyen doi
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon31001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaInChuyenDoi31002, "Hóa đơn đã in chuyển đổi, không thể in lại lần 2" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn31003, "Hóa đơn không có mẫu in" },

        ////lay 1 hoa don dang cho ky de bat dau ky
        //{ ErrorCodes.ErrorDefault.KhongTimThayChungThuSo32001, "Không tìm thấy chứng thư số" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanChuaDuocGanSuDungMauHoa32002, "Tài khoản chưa được gán sử dụng mẫu hóa đơn nào!" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanChuaDuocGanSuDungChungThuSoNay32003, "Tài khoản chưa được gán sử dụng chứng thư số này!" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanChuaDuocGanSuDungMauHoaDonVaChungThuSoNay32004, "Tài khoản chưa được gán sử dụng mẫu hóa đơn và chứng thư số này!" },
        //{ ErrorCodes.ErrorDefault.ChungThuSoDaHetHan32005, "Chứng thư số đã hết hạn sử dụng!" },

        ////post 1 hoa don sau khi ky
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon33001, "Không tìm thấy hóa đơn" },

        ////sync CA tu sign server
        //{ ErrorCodes.ErrorDefault.KhongCoChungThuSoTrenServer34001, "Không có chứng thư số trên server" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMaSoThueNguoiMua34002, "Không tìm thấy mã số thuế người mưa" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayChungThuSoNaoCoMaSoThue34003, "Không tìm thấy chứng thư số nào có mã số thuế như đã đăng ký" },

        ////nhap khach hang tu file excel
        //{ ErrorCodes.ErrorDefault.CoLoiXayRa35001, "Có lỗi xảy ra! vui lòng thử lại" },
        //{ ErrorCodes.ErrorDefault.HeThongChiHoTroDinhDangExcel2007TroLen35002, "Hệ thống chỉ hỗ trợ định dạng Excel 2007 trở lên (xlsx)" },

        ////nhap hang hoa tu file excel
        //{ ErrorCodes.ErrorDefault.CoLoiXayRa36001, "Có lỗi xảy ra! vui lòng thử lại" },
        //{ ErrorCodes.ErrorDefault.HeThongChiHoTroDinhDangExcel2007TroLen36002, "Hệ thống chỉ hỗ trợ định dạng Excel 2007 trở lên (xlsx)" },

        ////tao khach hang 37000
        //{ ErrorCodes.ErrorDefault.KhongTheDuaKhachHangVaoNhomChaCuaKhachHangHoacKhongTimThayNhomKhachHang37001, "Không thể đưa khách hàng vào nhóm cha của khách hàng hoặc không tìm thấy nhóm khách hàng" },
        //{ ErrorCodes.ErrorDefault.MaSoThueDaDuocSuDungBoiKhachHangKhac37002, "Mã số thuế đã được sử dụng bởi khách hàng trước. Nhập mã số thuế khác" },
        //{ ErrorCodes.ErrorDefault.MaKhachHangDaTonTai37003, "Mã khách hàng đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDung37004, "Email không đúng định dạng" },

        ////sua khach hang
        //{ ErrorCodes.ErrorDefault.KhongTimThayKhachHang38001, "Không tìm thấy khách hàng" },
        //{ ErrorCodes.ErrorDefault.KhongTheDuaKhachHangVaoNhomChaCuaKhachHangHoacKhongTimThayNhomKhachHang38001, "Không thể đưa khách hàng vào nhóm cha của khách hàng hoặc không tìm thấy nhóm khách hàng" },
        //{ ErrorCodes.ErrorDefault.MaKhachHangDaTonTai38003, "Mã khách hàng đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.EmailKhongDung38004, "Email của không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.EmailDaTonTai38005, "Email đã tồn tại" },


        ////xoa khach hang
        //{ ErrorCodes.ErrorDefault.KhongTimThayKhachHang39001, "Không tìm thấy khách hàng" },
        //{ ErrorCodes.ErrorDefault.DaCoDuLieuThamChieuToiKhachHang39002, "Đã có dữ liệu tham chiếu tới khách hàng này. Không thể xóa" },

        ////nhom khach hang vao nhom join-group
        //{ ErrorCodes.ErrorDefault.KhongTheThemKhachHangVaoNhomchaHoacNhomKhachHangKhongTonTai40001, "Không thể thêm khách hàng vào nhóm cha hoặc nhóm khách hàng không tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongTheChuyenNhomKhachHangChuaDuocKichHoat40002, "Không thể chuyển nhóm khách hàng chưa được kích hoạt" },

        ////tao nhom khach hang
        //{ ErrorCodes.ErrorDefault.NhomKhachHangChaDaKhongConTonTai41001, "Nhóm khách hàng cha đã không còn tồn tại, hãy chọn nhóm khách hàng cha khác" },
        //{ ErrorCodes.ErrorDefault.NhomKhachHangChiPhanToiDa2Cap41002, "Nhóm khách hàng chỉ phân tối đa 2 cấp" },

        ////sua nhom khach hang
        //{ ErrorCodes.ErrorDefault.KhongTimThayNhomKhachHang42001, "Không tìm thấy nhóm khách hàng" },
        //{ ErrorCodes.ErrorDefault.NhomChaKhongTheDoiLaiThanhNhomCon42002, "Nhóm cha không thể đổi lại thành nhóm con" },
        //{ ErrorCodes.ErrorDefault.NhomConKhongTheDoiLaiThanhNhomCha42003, "Nhóm con không thể đổi lại thành nhóm cha" },
        //{ ErrorCodes.ErrorDefault.MaNhomKhachHangDaTonTai42004, "Mã nhóm khách hàng đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayNhomChaHoacNhomChaDaLa1NhomConCuaNhomKhac42005, "Không tìm thấy nhóm cha hoặc nhóm cha đã là 1 nhóm con của nhóm khác" },

        ////xoa nhom khach hang
        //{ ErrorCodes.ErrorDefault.KhongTimThayNhomKhachHang43001, "Không tìm thấy nhóm khách hàng" },
        //{ ErrorCodes.ErrorDefault.KhongTheXoaNhomKhachHangDaChuaNhomKhachHangKhac43002, "Không thể xóa nhóm khách hàng đã chứa nhóm khách hàng khác" },
        //{ ErrorCodes.ErrorDefault.KhongTheXoaNhomDaChuaKhachHang43003, "Không thể xóa nhóm đã chứa khách hàng" },

        ////edit-batch-of-name
        //{ ErrorCodes.ErrorDefault.MotSoNhomKhongTonTai44001, "Một số nhóm không tồn tại" },

        ////tao phan tram thue

        ////sua phan tram thue
        //{ ErrorCodes.ErrorDefault.KhongTimThayThue46001, "Không tìm thấy thuế" },

        ////xoa phan tram thue
        //{ ErrorCodes.ErrorDefault.KhongTimThayThue47001, "Không tìm thấy thuế" },

        ////tim kiem don vi tinh
        //{ ErrorCodes.ErrorDefault.KhongTimThayDonViTinh48000, "Không tìm thấy đơn vị tính" },

        ////tao don vi tinh
        //{ ErrorCodes.ErrorDefault.DonViTinhDaTonTai48001, "Đơn vị tính đã tồn tại" },

        ////sua don vi tinh
        //{ ErrorCodes.ErrorDefault.KhongTimThayDonViTinh49001, "Không tìm thấy đơn vị tính" },
        //{ ErrorCodes.ErrorDefault.DonViTinhDaTonTai49002, "Đơn vị tính đã tồn tại" },

        ////xoa don vi tinh
        //{ ErrorCodes.ErrorDefault.KhongTimThayDonViTinh50001, "Không tìm thấy đơn vị tính" },
        //{ ErrorCodes.ErrorDefault.DaCoDuLieuThamChieuToiDonViTinh50002, "Đã có dữ liệu tham chiếu tới đơn vị tính" },

        ////tao tai khoan
        //{ ErrorCodes.ErrorDefault.TaiKhoanDaDuocSuDung51001, "Tài khoản đã được sử dụng" },

        ////sua tai khoan
        //{ ErrorCodes.ErrorDefault.TaiKhoanErpDaTonTai52001, "Tài khoản ERP đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanKhongTonTai52002, "Tài khoản không tồn tại" },

        ////tai khoan
        //{ ErrorCodes.ErrorDefault.KhongTimThayTaiKhoan53000, "Không tìm thấy tài khoản" },

        ////xoa tai khoan
        //{ ErrorCodes.ErrorDefault.KhongTimThayTaiKhoan53001, "Không tìm thấy tài khoản" },
        //{ ErrorCodes.ErrorDefault.TaiKhoanDaCoDuLieuThamChieuKhongTheXoa53002, "Tài khoản đã có dữ liệu tham chiếu! không thể xóa" },

        ////doi mat khau - pacth
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinTaiKhoan54001, "Không tìm thấy thông tin tài khoản" },
        //{ ErrorCodes.ErrorDefault.DoiMatKhauThatBai54002, "Đổi mật khẩu thất bại" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayThongTinSecurityStampTaiKhoan54003, "Không tìm thấy thông tin SecurityStamp của tài khoản" },

        ////Change password for the current login account - post - 55001

        ////tao quyen - 56001
        //{ ErrorCodes.ErrorDefault.QuyenDaTonTai56001, "Quyền đã tồn tại" },

        ////sua quyen - 57001
        //{ ErrorCodes.ErrorDefault.KhongTimThayQuyen57001, "Không tìm thấy quyền" },

        ////xoa quyen
        //{ ErrorCodes.ErrorDefault.KhongTimThayQuyen58001, "Không tìm thấy quyền" },
        //{ ErrorCodes.ErrorDefault.QuyenDangDuocSuDungKhongTheXoa58002, "Quyền đang được sử dụng không thể xóa" },

        ////Chinh sua mau email
        //{ ErrorCodes.ErrorDefault.KhongThayMauEmail59001, "Không thấy mẫu email" },

        ////tim mau email
        //{ ErrorCodes.ErrorDefault.KhongThayMauEmail59002, "Không thấy mẫu email" },

        ////send email
        //{ ErrorCodes.ErrorDefault.KhongThayEmailKhachHang60001, "Không thấy email khách hàng" },
        //{ ErrorCodes.ErrorDefault.KhongThayMauEmail60002, "Không thấy mẫu email" },
        //{ ErrorCodes.ErrorDefault.ThamSoMauKhongDung60003, "Tham sô mẫu không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongCoCauHinhSmtp60004, "Không có cấu hình SMTP" },
        //{ ErrorCodes.ErrorDefault.KhachHangKhongCoHoacChuaCapNhatEmail60005, "Khách hàng không có hoặc chưa cập nhật email" },
        //{ ErrorCodes.ErrorDefault.KhachHangKhongCoMaSoThue60006, "Khách hàng không có mã số thuế" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayTaiKhoanCuaKhachHang60007, "Không tìm thấy tài khoản của khách hàng" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayCongTyChiNhanhBanHangChoKhachHangNay60008, "Không tìm thấy công ty chi nhánh bán hàng cho khách hàng này" },

        ////tich hop hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDocGoc61001, "Không tìm thấy hóa đơn gốc" },
        //{ ErrorCodes.ErrorDefault.KhongCoIdHoaDonGoc61002, "Không có thông tin hóa đơn gốc" },

        ////điều chỉnh định danh thông tin biên bản của hóa đơn đã ký (tích hợp)
        //{ ErrorCodes.ErrorDefault.KhongDuocDieuChinhDinhDanhHoaDon62001, "Không được điều chỉnh định danh hóa đơn chưa ký" },

        ////thue
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy63001, "Không có thông tin công ty" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayPhanTramThue63002, "Không tìm thấy phần trăm thuế" },
        //{ ErrorCodes.ErrorDefault.MaThueDaTonTai63003, "Mã của thuế đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.GiaTriThueDaTonTai63004, "Giá trị của thuế đã tồn tại" },

        ////don vi tinh
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinCongTy64001, "Không có thông tin công ty" },

        ////tiền tệ 65000
        //{ ErrorCodes.ErrorDefault.KhongTimThayTienTe65000, "Không tìm thấy tiền tệ" },
        //{ ErrorCodes.ErrorDefault.MaTienTeDaTonTai65001, "Mã tiền tệ đã tồn tại. Nhập mã tiền tệ khác" },
        //{ ErrorCodes.ErrorDefault.MaTienTeKhongTonTai65002, "Mã tiền tệ không tồn tại trên hệ thống. Nhập mã tiền tệ khác" },

        ////chung thu so
        //{ ErrorCodes.ErrorDefault.ChungThuSoDaTonTai66000, "Chứng thư số đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRaTrongQuaTrinhDongBoChungThuSo, "Có lỗi xảy ra đồng bộ chứng thư số" },

        ////cau hinh
        //{ ErrorCodes.ErrorDefault.KhongTimThayTuKhoa67000, "Không tìm thấy từ khóa" },
        //{ ErrorCodes.ErrorDefault.TuKhoaDaTonTai67001, "Từ khóa đã tồn tại" },

        ////duyet hoa don
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDonDeDuyet68000, "Không tìm thấy hóa đơn để duyệt" },

        ////tao so hoa don
        //{ ErrorCodes.ErrorDefault.ChuaPhatHanhMauHoaDon70001, "Chưa phát hành mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonPhaiLonHonNgayDangKyPhatHanh70002, "Ngày hóa đơn phải lớn hơn ngày đăng ký phát hành" },
        //{ ErrorCodes.ErrorDefault.NgayDangKyPhatHanhLonHonNgayHienTai70003, "Ngày đăng ký phát hành hóa đơn lớn hơn ngày hiện tại. Ngày hóa đơn phải nhỏ hơn ngày đăng ký phát hành" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauHoaDon70004, "Không tìm thấy mẫu hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongThayDuLieuDeTaoHoaDon70005, "Không thấy dữ liệu để tạo hóa đơn" },
        //{ ErrorCodes.ErrorDefault.QuaTrinhTaoSoHoaDonXayRaLoi70006, "Quá trình tạo số hóa đơn xảy ra lỗi. Vui lòng kiểm tra lại ngày hóa đơn hoặc đăng ký phát hành hóa đơn" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonPhaiLonHonHoacBangNgayHoaDonCuoiCungCuaMau70007, "Ngày hóa đơn phải lớn hơn ngày cuối cùng tạo hóa đơn của mẫu" },
        //{ ErrorCodes.ErrorDefault.NgayHoaDonPhaiNhoHonNgayHienTai70008, "Ngày hóa đơn phải nhỏ hơn ngày hiện tại" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayDangKyPhatHanhHoaDon70009, "Không tìm thấy đăng ký phát hành hóa đơn" },

        ////tai len bien ban
        //{ ErrorCodes.ErrorDefault.HeThongChiNhanBienBanPdf71000, "Hệ thống chỉ chấp nhận Biên bản dạng PDF" },
        //{ ErrorCodes.ErrorDefault.FileBienBanKhongDuocDeTrong71001, "File biên bản không được để trống" },

        //// tạo phiếu xuất kho kiêm vận chuyển nội bộ
        //{ ErrorCodes.ErrorDefault.NgayDieuDongPhaiNhoHonHoacBangNgayXuatPhieu72000, "Ngày điều động phải nhỏ hơn hoặc bằng ngày xuất phiếu" },
        //{ ErrorCodes.ErrorDefault.PhieuXuatKhoDaTonTaiTrongHeThong72001, "Phiếu xuất kho đã tồn tại trong hệ thống" },

        //// tạo thay thế phiếu xuất kho kiêm vận chuyển nội bộ
        //{ ErrorCodes.ErrorDefault.NgayDieuDongPhaiNhoHonHoacBangNgayXuatPhieu73000, "Ngày điều động phải nhỏ hơn hoặc bằng ngày xuất phiếu" },

        //// sửa phiếu xuất kho kiêm vận chuyển nội bộ
        //{ ErrorCodes.ErrorDefault.NgayDieuDongPhaiNhoHonHoacBangNgayXuatPhieu74000, "Ngày điều động phải nhỏ hơn hoặc bằng ngày xuất phiếu" },

        ////License
        //{ ErrorCodes.ErrorDefault.KhongTimThayDangKyPhatHanhHoaDon700009, "Không tìm thấy đăng ký phát hành hóa đơn" },
        //{ ErrorCodes.ErrorDefault.ChuaCoGiayPhep700100, "Chưa có License hoặc License đã hết hạn" },
        //{ ErrorCodes.ErrorDefault.KhongDuocPhepSuDungGiayPhep700101, "Công ty của bạn không được phép sử dụng License này" },
        //{ ErrorCodes.ErrorDefault.LicenseDaBiChinhSua700102, "License đã bị hỏng, hãy liên hệ với nhà cung cấp để xử lý" },
        //{ ErrorCodes.ErrorDefault.SaiThongTinLicense700103, "Thông tin License không đúng, hãy kiểm tra lại mã và mật khẩu License" },
        //{ ErrorCodes.ErrorDefault.LicenseKhongDungDinhDang700104, "License không đúng định dạng" },
        //{ ErrorCodes.ErrorDefault.LicenseDaQuaSuDung700105, "License đã qua sử dụng, không thể sử dụng lại" },
        //{ ErrorCodes.ErrorDefault.LicenHetHanHoacChuaDangKyLicense700106, "License hết hạn hoặc chưa đăng ký license" },
        //{ ErrorCodes.ErrorDefault.MaHoaLicenseKhongDung700107, "Mã hóa license không đúng" },
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinLicense700108, "Chưa có license" },
        //{ ErrorCodes.ErrorDefault.KhongCoLicenseHoacLicenseKhongDu700109, "Chưa có license hoặc license hết hạn hoặc license không đủ để tạo hóa đơn" },

        ////Bc26
        //{ ErrorCodes.ErrorDefault.KhongTimThayBc26800001, "Không tìm thấy báo cáo bc26" },
        //{ ErrorCodes.ErrorDefault.Bc26DaTonTai800002, "BC26 đã tồn tại" },
        //{ ErrorCodes.ErrorDefault.CoLoiXayRa810003, "Có lỗi xảy ra" },

        ////portal
        //{ ErrorCodes.ErrorDefault.KhongCoHoaDon820001, "Không tìm thấy hóa đơn" },

        ////invoice 04 api
        //{ ErrorCodes.ErrorDefault.HoaDonDaTonTaiTrongHeThong83001, "Hoá đơn đã tồn tại trong hệ thống" },

        ////dbtg
        //{ ErrorCodes.ErrorDefault.ChuaCaiDatDBTG840001, "Chưa cài đặt link cấu hình dbtg" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayDuLieuTrungGian840002, "Không tìm thấy dữ liệu trung gian" },
        //{ ErrorCodes.ErrorDefault.QuaTrinhXuLyBiLoi840003, "Quá trình xử lý bị lỗi" },

        ////bien ban
        //{ ErrorCodes.ErrorDefault.KhongCoThongTinBienBan850001, "Không có thông tin biên bản" },

        ////send sms

        //{ ErrorCodes.ErrorDefault.KhachhangChuaCoSoDienThoai860001, "Khách hàng chưa có số điện thoại không thể gửi sms" },
        //{ ErrorCodes.ErrorDefault.ChuaCauHinhGuiSMS860002, "Chưa cấu hình gửi sms" },
        //{ ErrorCodes.ErrorDefault.ThamSoSMSKhongDung860003, "Tham số sms không đúng" },

        ////cong ty, chi nhanh
        //{ ErrorCodes.ErrorDefault.KhongThayThongTinCongTy870001, "Không thấy thông tin công ty" },

        ////in
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon880001, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauIn880002, "Không tìm thấy mẫu in" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn880003, "Hóa đơn không có mẫu in" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaInChuyenDoi880004, "Hóa đơn đã in chuyển đổi không thể in thể hiện" },

        ////in 03
        //{ ErrorCodes.ErrorDefault.KhongTimThayPhieuXuatKho890001, "Không tìm thấy phiếu xuất kho" },
        //{ ErrorCodes.ErrorDefault.KhongTimThayMauIn890002, "Không thấy mẫu in phiếu xuất kho" },
        //{ ErrorCodes.ErrorDefault.PhieuXuatKhoKhongCoMauIn890003, "Phiếu xuất kho không có mẫu in" },
        //{ ErrorCodes.ErrorDefault.PhieuXuatKhoDaInChuyenDoi890004, "Phiếu xuất kho đã in chuyển đổi không thể in thể hiện" },


        ////in 04
        //{ ErrorCodes.ErrorDefault.KhongTimThayHoaDon900002, "Không tìm thấy hóa đơn" },
        //{ ErrorCodes.ErrorDefault.HoaDonKhongCoMauIn900001, "Hóa đơn không có mẫu in" },
        //{ ErrorCodes.ErrorDefault.HoaDonDaInChuyenDoiKhongTheInTheHien900003, "Hóa đơn đã in chuyển đổi không thể in thể hiện" },

        ////in api
        //{ErrorCodes.ErrorDefault.KhongTimThayHoaDon100000, "Không tìm thấy hóa đơn" },
        //{ErrorCodes.ErrorDefault.KhongTimThayHoaDonHoacHoaDonLaHoaDonChuaKy100001, "Không tìm thấy hóa đơn hoặc hóa đơn là hóa đơn chưa ký" },
        //{ErrorCodes.ErrorDefault.KhongTimThayHoaDonDaKyTrongHeThong100002, "Không tìm thấy hóa đơn đã ký trong hệ thống" },

        ////xuat xml 04
        //{ ErrorCodes.ErrorDefault.HoaDonChuaKyKhongXuatXml110001, "Hóa đơn chưa ký không được xuất xml" },
        //{ErrorCodes.ErrorDefault.KhongCoXmlHoaDon110002, "Không có xml hóa đơn" },

        ////cấp số
        //{ErrorCodes.ErrorDefault.HoaDonDaCoSoKhongTheCapSo120001, "Hóa đơn đã có số không thể cấp lại số" },
        //{ErrorCodes.ErrorDefault.NgayHoaDonKhongDungKhongTaoDuocSo120002, "Ngày hóa đơn không đúng không tạo được số" },
        //{ErrorCodes.ErrorDefault.NgayHoaDonKhongDungKhongTheTaoSo120003, "Không thể cấp số cho hóa đơn có ngày hóa đơn không đúng" },

        ////Không thao tác hóa đơn k sôs
        //{ErrorCodes.ErrorDefault.KhongThaoTacHoaDonKhongSo130001, "Không thao tác với hóa đơn không số" },

        ////in hóa đơn api
        //{ErrorCodes.ErrorDefault.InKhongQua10HoaDon140001, "Chỉ được in không quá 10 hóa đơn 1 lần" },

        //            //thao tac hoa don qua api khog dung loai hoa don
        //{ErrorCodes.ErrorDefault.MauHoaDonKhongPhai01GTKT150001, "Mẫu hóa đơn không phải là 01GTKT" },
        //{ErrorCodes.ErrorDefault.MauHoaDonKhongPhai02GTTT150002, "Mẫu hóa đơn không phải là 02GTTT" },
        //{ErrorCodes.ErrorDefault.MauHoaDonKhongPhai03XKNB150003, "Mẫu hóa đơn không phải là 03XKNB" },
        //{ErrorCodes.ErrorDefault.MauHoaDonKhongPhai04HGDL150004, "Mẫu hóa đơn không phải là 04HGDL" },

        ////bc26
        //{ErrorCodes.ErrorDefault.BanChuaDangKyPhatHanhKhongTheTaoBaoCao160001, "Mẫu hóa đơn không phải là 04HGDL" },
        //{ErrorCodes.ErrorDefault.ChuaTaoBc26KyTruoc160002, "Chưa tạo bc26 kỳ trước" },

        ////cau hình
        //{ErrorCodes.ErrorDefault.KhongCoNhomCauHinh180001, "Không có nhóm cấu hình" },

        ////đổi mật khẩu
        //{ErrorCodes.ErrorDefault.KhongCoEmail170001, "Không có email" },

        ////phuong thuc thanh toan
        //{ErrorCodes.ErrorDefault.PhuongThucThanhToanKhongTonTai190001, "Phương thức thanh toán không tồn tại" },

        ////validate ký hiệu
        //{ErrorCodes.ErrorDefault.KyHieuHoaDonKhongDung200001, "Ký hiệu hóa đơn không đúng" },

        ////portal api
        //{ErrorCodes.ErrorDefault.MaSoThueNguoiBanKhongDuocDeTrong, "Mã số thuế người bán không được để trống" },
        //{ErrorCodes.ErrorDefault.SoBaoMatKhongDuocDeTrong, "Số bảo mật không được để trống" },

        //};


        public static Dictionary<int, string> VI2 = new Dictionary<int, string>();
    }
}
