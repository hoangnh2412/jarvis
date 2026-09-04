import { z } from 'zod'

export const roleFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập tên vai trò')
    .max(64, 'Tên vai trò tối đa 64 ký tự')
    .regex(/^[a-z][a-z0-9._-]*$/i, 'Tên vai trò chỉ gồm chữ, số, . _ -'),
  displayName: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập tên hiển thị')
    .max(128, 'Tên hiển thị tối đa 128 ký tự'),
  description: z
    .string()
    .trim()
    .max(512, 'Mô tả tối đa 512 ký tự')
    .optional()
    .nullable(),
  isDefault: z.boolean(),
  isPublic: z.boolean(),
})

export type RoleFormData = z.infer<typeof roleFormSchema>

export const roleFormDefaultValues: RoleFormData = {
  name: '',
  displayName: '',
  description: '',
  isDefault: false,
  isPublic: true,
}
