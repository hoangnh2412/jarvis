/** Dữ liệu mẫu — khớp `mocks/get-sample-brief-template.json` */
export const SAMPLE_BRIEF_SAMPLE_DATA: Record<string, string> = {
  documentTitle: 'NỘI DUNG TRAO ĐỔI SAMPLE',
  introParagraph:
    'Nhằm đảm bảo tính an toàn, bảo mật dữ liệu tuyệt đối và tuân thủ các tiêu chuẩn kỹ thuật trong quá trình vận hành, Sample xin gửi phương án kiến trúc và Giao thức tích hợp bảo mật giữa hệ thống nghiệp vụ và nền tảng Sample.',
  partnerOrganization: 'Sample',
  platformName: 'Sample',
  flow1Title: 'Luồng Khởi tạo & Đồng bộ nhân sự (hệ thống nghiệp vụ <-> Sample)',
  flow1Purpose:
    'Hệ thống nghiệp vụ gửi lệnh cấp quyền hoặc thu hồi quyền sử dụng Dấu xác nhận điện tử cho nhân sự lên máy chủ Sample.',
  flow1Environment: 'Giao tiếp Server-to-Server.',
  flow1Security: 'HMAC-SHA256 Payload Signature kết hợp IP Whitelisting.',
  flow1ApiEndpoint: '/api/v1/users/register',
  flow1Mechanism:
    'Khi có lệnh cấp quyền/thu hồi quyền, Backend gọi API /api/v1/users/register kèm header X-Api-Key và X-Signature (HMAC payload). Sample chỉ nhận request từ Whitelisted IPs và xác thực HMAC.',
  flow2Title: 'Luồng Chuyển hướng & Định danh phiên ký (hệ thống nghiệp vụ -> Sample)',
  flow2Purpose:
    'Người dùng bấm "Ký tài liệu" trên hệ thống nghiệp vụ và được redirect sang module ký của Sample.',
  flow2Security: 'Short-lived JWE (JSON Web Encryption) hoặc Opaque Token.',
  flow2TokenTtl: '60',
  flow2TokenFields: 'document_id, user_id, return_url',
  flow2Mechanism:
    'Hệ thống nghiệp vụ sinh token mã hóa chứa document_id, user_id, return_url (TTL 60 giây). Trình duyệt redirect sang Sample kèm token. Token hết hạn hoặc sai định dạng sẽ bị từ chối.',
  flow3Title: 'Luồng Trích xuất & Cập nhật tài liệu (Sample <-> API hệ thống nghiệp vụ)',
  flow3Purpose:
    'Module ký của Sample gọi API hệ thống nghiệp vụ để lấy nội dung tài liệu và đẩy file đã ký trả lại hệ thống.',
  flow3Security: 'OAuth2 Client Credentials Flow kết hợp Mutual TLS (mTLS).',
  flow3Mechanism:
    'Sample là Machine Client, gọi API hệ thống nghiệp vụ với Client ID + Client Secret. Kết nối được chứng thực bằng mTLS ở tầng network.',
  flow4Title: 'Luồng Thực thi Ký & Đóng dấu xác nhận (Sample)',
  flow4Purpose:
    'Module ký gửi thông tin phiên làm việc lên Sample để sinh Dấu xác nhận điện tử và ghi nhận Audit Log.',
  flow4Security: 'HMAC-SHA256 Payload Signature + IP Whitelisting + Rate Limiting.',
  flow4Mechanism:
    'Module ký gửi hash tài liệu gốc, IP thiết bị và UserID lên Sample (không truyền file vật lý). Request bảo vệ bằng X-Signature; Sample trả về thông tin Dấu xác nhận để nhúng vào tài liệu.',
}

export function getSampleBriefSampleData(): Record<string, string> {
  return { ...SAMPLE_BRIEF_SAMPLE_DATA }
}
