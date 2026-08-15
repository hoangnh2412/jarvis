import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  changePasswordFormDefaultValues,
  changePasswordSchema,
  type ChangePasswordFormData,
} from '../validation'

export type UseChangePasswordFormOptions = {
  defaultValues?: Partial<ChangePasswordFormData>
}

export function useChangePasswordForm(
  options: UseChangePasswordFormOptions = {},
): UseFormReturn<ChangePasswordFormData> {
  return useRHFForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    mode: 'onChange',
    defaultValues: {
      ...changePasswordFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
