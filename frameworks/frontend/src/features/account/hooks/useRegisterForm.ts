import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  registerFormDefaultValues,
  registerSchema,
  type RegisterFormData,
} from '../validation'

export type UseRegisterFormOptions = {
  defaultValues?: Partial<RegisterFormData>
}

export function useRegisterForm(
  options: UseRegisterFormOptions = {},
): UseFormReturn<RegisterFormData> {
  return useRHFForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: 'onChange',
    defaultValues: {
      ...registerFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
