import type { AccountAuthVariant, AuthPreset } from '../types'

export const AUTH_PRESETS: Record<AccountAuthVariant, AuthPreset> = {
  login: {
    title: 'Đăng nhập',
    description: 'Nhập email và mật khẩu để tiếp tục.',
    submitLabel: 'Đăng nhập',
    panelEyebrow: 'Secure access',
    panelLine: 'Một cổng vào hệ thống — nhanh, rõ, đáng tin.',
  },
  register: {
    title: 'Tạo tài khoản',
    description: 'Điền thông tin để bắt đầu sử dụng hệ thống.',
    submitLabel: 'Tạo tài khoản',
    panelEyebrow: 'Get started',
    panelLine: 'Tài khoản mới, cùng một chuẩn bảo mật và trải nghiệm.',
  },
  forgot: {
    title: 'Quên mật khẩu',
    description: 'Nhập email để nhận liên kết đặt lại mật khẩu.',
    submitLabel: 'Gửi liên kết',
    panelEyebrow: 'Account recovery',
    panelLine:
      'Chúng tôi gửi liên kết đặt lại — không lộ email có tồn tại hay không.',
  },
}

export const viMessages = {
  auth: {
    processing: 'Đang xử lý…',
    checkInbox: 'Kiểm tra hộp thư của bạn để tiếp tục.',
    securityFooter: 'Bảo mật · Xác thực · Hồ sơ',
    brandName: 'FE Component',
    brandSubtitle: 'Account',
  },
  routes: {
    login: 'Đăng nhập',
    register: 'Đăng ký',
    forgotPassword: 'Quên mật khẩu',
    profile: 'Hồ sơ',
    changePassword: 'Đổi mật khẩu',
  },
  menu: {
    profile: 'Hồ sơ',
    changePassword: 'Đổi mật khẩu',
  },
}
