import { z } from 'zod'
import { REGEX } from './regex'

export const registerSchema = z
  .object({
    fullName: z
      .string()
      .min(1, 'Họ tên là bắt buộc')
      .min(2, 'Họ tên tối thiểu 2 ký tự'),
    email: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập email')
      .max(255, 'Email quá dài')
      .refine((val) => REGEX.EMAIL.test(val), 'Email không hợp lệ'),
    password: z
      .string()
      .min(6, 'Mật khẩu tối thiểu 6 ký tự')
      .regex(/[A-Z]/, 'Cần ít nhất 1 chữ hoa')
      .regex(/[a-z]/, 'Cần ít nhất 1 chữ thường')
      .regex(/[0-9]/, 'Cần ít nhất 1 chữ số'),
    confirmPassword: z.string().min(1, 'Xác nhận mật khẩu là bắt buộc'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Mật khẩu xác nhận không khớp',
    path: ['confirmPassword'],
  })

export type RegisterFormData = z.infer<typeof registerSchema>

export const registerFormDefaultValues: RegisterFormData = {
  fullName: '',
  email: '',
  password: '',
  confirmPassword: '',
}
