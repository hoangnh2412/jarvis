import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, type DefaultValues, type UseFormReturn } from 'react-hook-form'
import {
  craftPdfTemplateFormDefaultValues,
  craftPdfTemplateFormSchema,
  type CraftPdfTemplateFormData,
} from '../validation'

export type UseCraftPdfTemplateFormOptions = {
  defaultValues?: Partial<CraftPdfTemplateFormData>
}

export function useCraftPdfTemplateForm(
  options: UseCraftPdfTemplateFormOptions = {},
): UseFormReturn<CraftPdfTemplateFormData> {
  return useForm<CraftPdfTemplateFormData>({
    resolver: zodResolver(craftPdfTemplateFormSchema),
    defaultValues: {
      ...craftPdfTemplateFormDefaultValues,
      ...options.defaultValues,
    } as DefaultValues<CraftPdfTemplateFormData>,
  })
}
