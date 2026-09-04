import { z } from 'zod'
import { TenantStatus } from '../types'

export const tenantFormSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập mã tenant')
    .max(64, 'Mã tenant tối đa 64 ký tự'),
  name: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập tên tenant')
    .max(256, 'Tên tenant tối đa 256 ký tự'),
  status: z.union([
    z.literal(TenantStatus.Inactive),
    z.literal(TenantStatus.Active),
  ]),
  parentId: z.string().uuid('ParentId không hợp lệ').nullable().optional(),
})

export type TenantFormData = z.infer<typeof tenantFormSchema>

export const tenantFormDefaultValues: TenantFormData = {
  code: '',
  name: '',
  status: TenantStatus.Active,
  parentId: null,
}
