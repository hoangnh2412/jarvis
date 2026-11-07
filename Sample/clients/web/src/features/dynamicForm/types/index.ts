import type { RJSFSchema, UiSchema } from '@rjsf/utils'

export type DynamicFormFieldType =
  | 'text'
  | 'number'
  | 'email'
  | 'password'
  | 'textarea'
  | 'select'
  | 'radio'
  | 'date'
  | 'checkbox'
  /** Nested form section (RJSF object). */
  | 'group'
  /** Editable row table (RJSF array of objects + PrimeReact DataTable). */
  | 'table'

/** Column types allowed inside a `table` field. */
export type DynamicFormTableColumnType = Exclude<
  DynamicFormFieldType,
  'group' | 'table' | 'password' | 'textarea' | 'radio'
>

export type DynamicFormSelectOption = {
  id: string
  label: string
  value: string
  enabled?: boolean
}

export type DynamicFormSelectConfig = {
  quickEdit?: boolean
  sortAlphabetical?: boolean
}

/** Canvas grid width. Default `full` = stacked vertically like before. */
export type DynamicFormLayoutWidth = 'full' | 'half'

export type DynamicFormField = {
  id: string
  key: string
  type: DynamicFormFieldType
  label: string
  required?: boolean
  placeholder?: string
  helpText?: string
  defaultValue?: string | number | boolean
  remindWhenEmpty?: boolean
  options?: DynamicFormSelectOption[]
  selectConfig?: DynamicFormSelectConfig
  /** Nested fields when type === 'group' */
  children?: DynamicFormField[]
  /** Grid column span on the form canvas (default full). */
  layoutWidth?: DynamicFormLayoutWidth
  /** string validation */
  minLength?: number
  maxLength?: number
  /** number validation */
  minimum?: number
  maximum?: number
  multipleOf?: number
}

export type DynamicFormDefinition = {
  id: string
  name: string
  description?: string
  version: number
  fields: DynamicFormField[]
  schema: RJSFSchema
  uiSchema?: UiSchema
  formData?: Record<string, unknown>
  updatedAt: string
  createdAt: string
}

export type DynamicFormListItem = Pick<
  DynamicFormDefinition,
  'id' | 'name' | 'description' | 'version' | 'updatedAt' | 'createdAt'
> & {
  fieldCount: number
}

export type CreateDynamicFormPayload = {
  name: string
  description?: string
  fields?: DynamicFormField[]
  schema?: RJSFSchema
  uiSchema?: UiSchema
  formData?: Record<string, unknown>
}

export type UpdateDynamicFormPayload = {
  id: string
  name?: string
  description?: string
  fields?: DynamicFormField[]
  schema?: RJSFSchema
  uiSchema?: UiSchema
  formData?: Record<string, unknown>
}

export type DynamicFormSubmission = {
  id: string
  formId: string
  data: Record<string, unknown>
  createdAt: string
}

export type CreateDynamicFormSubmissionPayload = {
  formId: string
  data: Record<string, unknown>
}

export type { RJSFSchema, UiSchema }
