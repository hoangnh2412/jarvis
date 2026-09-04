import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  tenantDomainFormDefaultValues,
  tenantDomainFormSchema,
  type TenantDomainFormData,
} from '../validation'

export type UseTenantDomainFormOptions = {
  defaultValues?: Partial<TenantDomainFormData>
}

export function useTenantDomainForm(
  options: UseTenantDomainFormOptions = {},
): UseFormReturn<TenantDomainFormData> {
  return useRHFForm<TenantDomainFormData>({
    resolver: zodResolver(tenantDomainFormSchema),
    mode: 'onChange',
    defaultValues: {
      ...tenantDomainFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
