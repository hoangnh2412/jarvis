import { useForm as useRHFForm, type UseFormReturn } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  roleFormDefaultValues,
  roleFormSchema,
  type RoleFormData,
} from '../validation'

export type UseRoleFormOptions = {
  defaultValues?: Partial<RoleFormData>
}

export function useRoleForm(
  options: UseRoleFormOptions = {},
): UseFormReturn<RoleFormData> {
  return useRHFForm<RoleFormData>({
    resolver: zodResolver(roleFormSchema),
    mode: 'onChange',
    defaultValues: {
      ...roleFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
