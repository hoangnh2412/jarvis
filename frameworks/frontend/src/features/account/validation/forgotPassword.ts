import { z } from 'zod'
import { REGEX } from './regex'

export const forgotPasswordSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập email')
    .max(255, 'Email quá dài')
    .refine((val) => REGEX.EMAIL.test(val), 'Email không hợp lệ'),
})

export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>

export const forgotPasswordFormDefaultValues: ForgotPasswordFormData = {
  email: '',
}
