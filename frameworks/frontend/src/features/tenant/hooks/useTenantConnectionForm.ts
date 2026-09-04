import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  tenantConnectionFormDefaultValues,
  tenantConnectionFormSchema,
  type TenantConnectionFormData,
} from '../validation'

export type UseTenantConnectionFormOptions = {
  defaultValues?: Partial<TenantConnectionFormData>
}

export function useTenantConnectionForm(
  options: UseTenantConnectionFormOptions = {},
): UseFormReturn<TenantConnectionFormData> {
  return useRHFForm<TenantConnectionFormData>({
    resolver: zodResolver(tenantConnectionFormSchema),
    mode: 'onChange',
    defaultValues: {
      ...tenantConnectionFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
