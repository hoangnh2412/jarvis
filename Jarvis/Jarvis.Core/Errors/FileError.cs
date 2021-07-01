
namespace Jarvis.Core.Errors
{
    public class FileError : IError
    {
        public static ErrorInfo KhongTimThayFile = new ErrorInfo(30001, "Không tìm thấy file");
        public static ErrorInfo FileKhongCoDuLieu = new ErrorInfo(30002, "File không có dữ liệu");
        public static ErrorInfo KhongTimPhaiFilePdf = new ErrorInfo(30003, "Không phải file pdf");
        public static ErrorInfo KhongTimThayFileVatLy = new ErrorInfo(30004, "Không tìm thấy file vật lý");
    }
}
