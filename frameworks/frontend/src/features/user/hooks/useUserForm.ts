import { useForm as useRHFForm, type Resolver, type UseFormReturn } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  createUserFormSchema,
  editUserFormSchema,
  userFormDefaultValues,
  type UserFormData,
} from '../validation'

export type UseUserFormOptions = {
  mode: 'create' | 'edit'
  defaultValues?: Partial<UserFormData>
}

export function useUserForm({
  mode,
  defaultValues,
}: UseUserFormOptions): UseFormReturn<UserFormData> {
  const schema = mode === 'create' ? createUserFormSchema : editUserFormSchema
  return useRHFForm<UserFormData>({
    resolver: zodResolver(schema) as Resolver<UserFormData>,
    mode: 'onChange',
    defaultValues: {
      ...userFormDefaultValues,
      ...defaultValues,
    },
  })
}
