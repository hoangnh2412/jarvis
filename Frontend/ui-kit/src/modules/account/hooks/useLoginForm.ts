import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  loginFormDefaultValues,
  loginSchema,
  type LoginFormData,
} from '../validation'

export type UseLoginFormOptions = {
  defaultValues?: Partial<LoginFormData>
}

export function useLoginForm(
  options: UseLoginFormOptions = {},
): UseFormReturn<LoginFormData> {
  return useRHFForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
    defaultValues: {
      ...loginFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
