import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  tenantFormDefaultValues,
  tenantFormSchema,
  type TenantFormData,
} from '../validation'

export type UseTenantFormOptions = {
  defaultValues?: Partial<TenantFormData>
}

export function useTenantForm(
  options: UseTenantFormOptions = {},
): UseFormReturn<TenantFormData> {
  return useRHFForm<TenantFormData>({
    resolver: zodResolver(tenantFormSchema),
    mode: 'onChange',
    defaultValues: {
      ...tenantFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
