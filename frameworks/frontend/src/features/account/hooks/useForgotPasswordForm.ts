import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  forgotPasswordFormDefaultValues,
  forgotPasswordSchema,
  type ForgotPasswordFormData,
} from '../validation'

export type UseForgotPasswordFormOptions = {
  defaultValues?: Partial<ForgotPasswordFormData>
}

export function useForgotPasswordForm(
  options: UseForgotPasswordFormOptions = {},
): UseFormReturn<ForgotPasswordFormData> {
  return useRHFForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
    mode: 'onChange',
    defaultValues: {
      ...forgotPasswordFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
