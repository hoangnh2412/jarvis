import { z } from 'zod'

export const craftPdfTemplateFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Tên template bắt buộc')
    .max(120, 'Tối đa 120 ký tự'),
  description: z.string().trim().max(500, 'Tối đa 500 ký tự').optional(),
})

export type CraftPdfTemplateFormData = z.infer<typeof craftPdfTemplateFormSchema>

export const craftPdfTemplateFormDefaultValues: CraftPdfTemplateFormData = {
  name: '',
  description: '',
}
