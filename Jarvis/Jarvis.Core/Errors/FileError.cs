
namespace Jarvis.Core.Errors
{
    public class FileError : IError
    {
        public static ErrorInfo KhongTimThayFile = new ErrorInfo(3001, "Không tìm thấy file");
        public static ErrorInfo FileKhongCoDuLieu = new ErrorInfo(3002, "File không có dữ liệu");
        public static ErrorInfo KhongTimPhaiFilePdf = new ErrorInfo(3003, "Không phải file pdf");
        public static ErrorInfo KhongTimThayFileVatLy = new ErrorInfo(3004, "Không tìm thấy file vật lý");
    }
}
