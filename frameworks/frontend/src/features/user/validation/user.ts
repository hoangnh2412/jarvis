import { z } from 'zod'

const username = z
  .string()
  .trim()
  .min(1, 'Vui lòng nhập tên đăng nhập')
  .max(64, 'Tên đăng nhập tối đa 64 ký tự')
  .regex(/^[a-zA-Z0-9._-]+$/, 'Chỉ gồm chữ, số, . _ -')

const email = z
  .string()
  .trim()
  .min(1, 'Vui lòng nhập email')
  .max(255, 'Email tối đa 255 ký tự')
  .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Email không hợp lệ')

const roleId = z.string().trim().min(1, 'Vui lòng chọn vai trò')

const optionalId = z
  .string()
  .trim()
  .max(64, 'Tối đa 64 ký tự')
  .optional()
  .or(z.literal(''))

export const createUserFormSchema = z.object({
  username,
  email,
  password: z
    .string()
    .min(8, 'Mật khẩu tối thiểu 8 ký tự')
    .max(128, 'Mật khẩu tối đa 128 ký tự'),
  roleId,
  tenantId: optionalId,
  orgId: optionalId,
  isLocked: z.boolean(),
})

export const editUserFormSchema = z.object({
  username,
  email,
  password: z.string().optional(),
  roleId,
  tenantId: optionalId,
  orgId: optionalId,
  isLocked: z.boolean(),
})

export type UserFormData = z.infer<typeof createUserFormSchema>

export const userFormDefaultValues: UserFormData = {
  username: '',
  email: '',
  password: '',
  roleId: '',
  tenantId: '',
  orgId: '',
  isLocked: false,
}
