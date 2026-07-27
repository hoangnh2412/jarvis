import { z } from 'zod'

export const tenantDomainFormSchema = z.object({
  domain: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập domain')
    .max(256, 'Domain tối đa 256 ký tự'),
  isPrimary: z.boolean(),
})

export type TenantDomainFormData = z.infer<typeof tenantDomainFormSchema>

export const tenantDomainFormDefaultValues: TenantDomainFormData = {
  domain: '',
  isPrimary: false,
}
