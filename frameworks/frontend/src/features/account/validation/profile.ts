import { z } from 'zod'
import { REGEX } from './regex'

export const profileSchema = z.object({
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
  phone: z.string().optional(),
})

export type ProfileFormData = z.infer<typeof profileSchema>

export const profileFormDefaultValues: ProfileFormData = {
  fullName: '',
  email: '',
  phone: '',
}
