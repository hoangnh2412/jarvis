import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, type UseFormReturn } from 'react-hook-form'
import {
  orgFormDefaultValues,
  orgFormSchema,
  type OrgFormData,
} from '../validation'

export type UseOrgFormOptions = {
  defaultValues?: Partial<OrgFormData>
}

export function useOrgForm(
  options: UseOrgFormOptions = {},
): UseFormReturn<OrgFormData> {
  return useForm<OrgFormData>({
    resolver: zodResolver(orgFormSchema),
    mode: 'onChange',
    defaultValues: {
      ...orgFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
