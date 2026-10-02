/** Dữ liệu mẫu — khớp `mocks/get-lla-idas-template.json` */
export const LLA_IDAS_SAMPLE_DATA: Record<string, string> = {
  documentTitle: 'NỘI DUNG TRAO ĐỔI LLA – IDAS',
  introParagraph:
    'Nhằm đảm bảo tính an toàn, bảo mật dữ liệu tuyệt đối và tuân thủ các tiêu chuẩn kỹ thuật trong quá trình vận hành, LLA xin gửi đến IDAS phương án kiến trúc và Giao thức tích hợp bảo mật giữa hệ thống IDAS eOffice và nền tảng LLA eContract & LLA e-Sign API.',
  partnerOrganization: 'IDAS',
  platformName: 'LLA eContract & LLA e-Sign API',
  flow1Title: 'Luồng Khởi tạo & Đồng bộ nhân sự (eOffice <-> LLA e-Sign API)',
  flow1Purpose:
    'eOffice gửi lệnh cấp quyền hoặc thu hồi quyền sử dụng Dấu xác nhận điện tử cho nhân sự lên máy chủ LLA e-Sign API.',
  flow1Environment: 'Giao tiếp Server-to-Server.',
  flow1Security: 'HMAC-SHA256 Payload Signature kết hợp IP Whitelisting.',
  flow1ApiEndpoint: '/api/v1/users/register',
  flow1Mechanism:
    'Khi có lệnh cấp quyền/thu hồi quyền, Backend eOffice gọi API /api/v1/users/register kèm header X-Api-Key và X-Signature (HMAC payload). LLA e-Sign chỉ nhận request từ Whitelisted IPs và xác thực HMAC.',
  flow2Title: 'Luồng Chuyển hướng & Định danh phiên ký (eOffice -> LLA eContract)',
  flow2Purpose:
    'Người dùng bấm "Ký tài liệu" trên eOffice và được redirect sang module LLA eContract.',
  flow2Security: 'Short-lived JWE (JSON Web Encryption) hoặc Opaque Token.',
  flow2TokenTtl: '60',
  flow2TokenFields: 'document_id, user_id, return_url',
  flow2Mechanism:
    'eOffice sinh token mã hóa chứa document_id, user_id, return_url (TTL 60 giây). Trình duyệt redirect sang LLA eContract kèm token. Token hết hạn hoặc sai định dạng sẽ bị từ chối.',
  flow3Title: 'Luồng Trích xuất & Cập nhật tài liệu (LLA eContract <-> eOffice API)',
  flow3Purpose:
    'Module LLA eContract gọi API eOffice để lấy nội dung tài liệu và đẩy file đã ký trả lại hệ thống.',
  flow3Security: 'OAuth2 Client Credentials Flow kết hợp Mutual TLS (mTLS).',
  flow3Mechanism:
    'LLA eContract là Machine Client, gọi API eOffice với Client ID + Client Secret. Kết nối được chứng thực bằng mTLS ở tầng network.',
  flow4Title: 'Luồng Thực thi Ký & Đóng dấu xác nhận (LLA eContract <-> LLA e-Sign API)',
  flow4Purpose:
    'eContract gửi thông tin phiên làm việc lên LLA để sinh Dấu xác nhận điện tử và ghi nhận Audit Log.',
  flow4Security: 'HMAC-SHA256 Payload Signature + IP Whitelisting + Rate Limiting.',
  flow4Mechanism:
    'eContract gửi hash tài liệu gốc, IP thiết bị và UserID lên LLA e-Sign API (không truyền file vật lý). Request bảo vệ bằng X-Signature; LLA trả về thông tin Dấu xác nhận để nhúng vào tài liệu.',
}

export function getLlaIdasSampleData(): Record<string, string> {
  return { ...LLA_IDAS_SAMPLE_DATA }
}
