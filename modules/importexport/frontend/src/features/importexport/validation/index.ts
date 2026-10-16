import { z } from 'zod'

export const columnMappingSchema = z.object({
  columnName: z.string().trim().min(1, 'Column name bắt buộc'),
  fieldName: z.string().trim().min(1, 'Field name bắt buộc'),
  required: z.boolean().optional(),
  unique: z.boolean().optional(),
  regex: z.string().nullable().optional(),
  minLength: z.number().int().positive().nullable().optional(),
  maxLength: z.number().int().positive().nullable().optional(),
  min: z.number().nullable().optional(),
  max: z.number().nullable().optional(),
})

export const templateMetadataSchema = z.object({
  code: z.string().trim().min(1),
  name: z.string().trim().min(1, 'Tên bắt buộc'),
})

export const templateConfigurationSchema = z
  .object({
    sheetName: z.string().trim().optional().or(z.literal('')),
    headerRow: z.number().int().min(1),
    startRow: z.number().int().min(1),
    rowCount: z.number().int().positive().nullable().optional(),
    mappings: z.array(columnMappingSchema).min(1, 'Cần ít nhất 1 mapping'),
    fileName: z.string().optional(),
  })
  .superRefine((values, ctx) => {
    const ext = (values.fileName ?? '').toLowerCase()
    if (ext.endsWith('.xlsx') && !values.sheetName?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Sheet name bắt buộc với file Excel (.xlsx)',
        path: ['sheetName'],
      })
    }
  })

export const templateFormSchema = z
  .object({
    code: z
      .string()
      .trim()
      .min(1, 'Code bắt buộc')
      .regex(/^[A-Za-z0-9_.-]+$/, 'Code chỉ gồm chữ, số, _ . -'),
    name: z.string().trim().min(1, 'Tên bắt buộc'),
    sheetName: z.string().trim().optional().or(z.literal('')),
    headerRow: z.number().int().min(1),
    startRow: z.number().int().min(1),
    rowCount: z.number().int().positive().nullable().optional(),
    mappings: z.array(columnMappingSchema).min(1, 'Cần ít nhất 1 mapping'),
    fileName: z.string().optional(),
  })
  .superRefine((values, ctx) => {
    const ext = (values.fileName ?? '').toLowerCase()
    if (ext.endsWith('.xlsx') && !values.sheetName?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Sheet name bắt buộc với file Excel (.xlsx)',
        path: ['sheetName'],
      })
    }
  })

export type TemplateFormValues = z.infer<typeof templateFormSchema>
export type TemplateMetadataValues = z.infer<typeof templateMetadataSchema>
export type TemplateConfigurationValues = z.infer<typeof templateConfigurationSchema>

export const templateFormDefaultValues: TemplateFormValues = {
  code: '',
  name: '',
  sheetName: '',
  headerRow: 1,
  startRow: 2,
  rowCount: null,
  fileName: '',
  mappings: [
    {
      columnName: '',
      fieldName: '',
      required: false,
      unique: false,
      regex: null,
      minLength: null,
      maxLength: null,
      min: null,
      max: null,
    },
  ],
}

export const templateConfigurationDefaultValues: TemplateConfigurationValues = {
  sheetName: '',
  headerRow: 1,
  startRow: 2,
  rowCount: null,
  fileName: '',
  mappings: templateFormDefaultValues.mappings,
}
