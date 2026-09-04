import {
  useForm as useRHFForm,
  type UseFormReturn,
} from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  profileFormDefaultValues,
  profileSchema,
  type ProfileFormData,
} from '../validation'

export type UseProfileFormOptions = {
  defaultValues?: Partial<ProfileFormData>
}

export function useProfileForm(
  options: UseProfileFormOptions = {},
): UseFormReturn<ProfileFormData> {
  return useRHFForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    mode: 'onChange',
    defaultValues: {
      ...profileFormDefaultValues,
      ...options.defaultValues,
    },
  })
}
