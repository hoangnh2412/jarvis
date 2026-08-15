import { z } from 'zod'
import { REGEX } from './regex'

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập email')
    .max(255, 'Email quá dài')
    .refine((val) => REGEX.EMAIL.test(val), 'Email không hợp lệ'),
  password: z
    .string()
    .min(1, 'Mật khẩu là bắt buộc')
    .min(6, 'Mật khẩu tối thiểu 6 ký tự'),
})

export type LoginFormData = z.infer<typeof loginSchema>

export const loginFormDefaultValues: LoginFormData = {
  email: '',
  password: '',
}
