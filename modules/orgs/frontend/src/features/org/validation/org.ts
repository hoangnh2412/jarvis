import { z } from 'zod'
import type { Org } from '../types'

export const orgFormSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập mã organization')
    .max(64, 'Mã organization tối đa 64 ký tự'),
  name: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập tên organization')
    .max(256, 'Tên organization tối đa 256 ký tự'),
})

export type OrgFormData = z.infer<typeof orgFormSchema>

export const orgFormDefaultValues: OrgFormData = {
  code: '',
  name: '',
}

export function toOrgFormValues(org: Pick<Org, 'code' | 'name'>): OrgFormData {
  return {
    code: org.code,
    name: org.name,
  }
}
