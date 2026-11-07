import type { RJSFSchema, UiSchema } from '@rjsf/utils'
import type {
  DynamicFormField,
  DynamicFormFieldType,
  DynamicFormSelectOption,
} from '../types'
import {
  createFieldId,
  createOptionId,
  normalizeSelectOptions,
} from './fields'

function isPlainSchema(
  def: unknown,
): def is RJSFSchema & { type?: string; properties?: Record<string, unknown> } {
  return Boolean(def) && typeof def === 'object' && !Array.isArray(def)
}

function enumOptions(field: DynamicFormField) {
  const options = normalizeSelectOptions(field.options).filter(
    (o) => o.enabled !== false,
  )
  return field.selectConfig?.sortAlphabetical
    ? [...options].sort((a, b) =>
        a.label.localeCompare(b.label, undefined, { sensitivity: 'base' }),
      )
    : options
}

/** True if field itself or any nested child must be filled. */
function fieldHasRequired(field: DynamicFormField): boolean {
  if (field.type === 'table') {
    return Boolean(field.required || field.remindWhenEmpty)
  }
  if (field.type !== 'group' && (field.required || field.remindWhenEmpty)) {
    return true
  }
  if (field.type === 'group') {
    return (field.children ?? []).some(fieldHasRequired)
  }
  return Boolean(field.required || field.remindWhenEmpty)
}

function fieldToProperty(field: DynamicFormField): RJSFSchema {
  if (field.type === 'group') {
    const children = field.children ?? []
    const properties: Record<string, RJSFSchema> = {}
    const required: string[] = []
    for (const child of children) {
      properties[child.key] = fieldToProperty(child)
      // Nested objects must be listed in `required` or AJV skips their children.
      if (fieldHasRequired(child)) required.push(child.key)
    }
    return {
      type: 'object',
      title: field.label,
      description: field.helpText || undefined,
      properties,
      ...(required.length ? { required } : {}),
    }
  }

  if (field.type === 'table') {
    const columns = (field.children ?? []).filter(
      (col) => col.type !== 'group' && col.type !== 'table',
    )
    const properties: Record<string, RJSFSchema> = {}
    const required: string[] = []
    for (const col of columns) {
      properties[col.key] = fieldToProperty(col)
      if (col.required || col.remindWhenEmpty) required.push(col.key)
    }
    return {
      type: 'array',
      title: field.label,
      description: field.helpText || undefined,
      default: [],
      items: {
        type: 'object',
        properties,
        ...(required.length ? { required } : {}),
      },
      ...(field.required || field.remindWhenEmpty ? { minItems: 1 } : {}),
    }
  }

  switch (field.type) {
    case 'number':
      return {
        type: 'number',
        title: field.label,
        ...(typeof field.minimum === 'number' ? { minimum: field.minimum } : {}),
        ...(typeof field.maximum === 'number' ? { maximum: field.maximum } : {}),
        ...(typeof field.multipleOf === 'number'
          ? { multipleOf: field.multipleOf }
          : {}),
        ...(typeof field.defaultValue === 'number'
          ? { default: field.defaultValue }
          : {}),
      }
    case 'checkbox':
      return {
        type: 'boolean',
        title: field.label,
        default:
          typeof field.defaultValue === 'boolean' ? field.defaultValue : false,
        // Required checkbox = must be checked (true), not merely present.
        ...(field.required || field.remindWhenEmpty ? { const: true } : {}),
      }
    case 'date':
      return {
        type: 'string',
        title: field.label,
        format: 'date',
        ...(typeof field.defaultValue === 'string' && field.defaultValue
          ? { default: field.defaultValue }
          : {}),
      }
    case 'email':
      return {
        type: 'string',
        title: field.label,
        format: 'email',
        ...(typeof field.minLength === 'number'
          ? { minLength: field.minLength }
          : {}),
        ...(typeof field.maxLength === 'number'
          ? { maxLength: field.maxLength }
          : {}),
        ...(typeof field.defaultValue === 'string' && field.defaultValue
          ? { default: field.defaultValue }
          : {}),
      }
    case 'select':
    case 'radio': {
      const options = enumOptions(field)
      return {
        type: 'string',
        title: field.label,
        enum: options.map((o) => o.value),
        ...(typeof field.defaultValue === 'string' && field.defaultValue
          ? { default: field.defaultValue }
          : {}),
      }
    }
    case 'password':
    case 'textarea':
    case 'text':
    default:
      return {
        type: 'string',
        title: field.label,
        ...(typeof field.minLength === 'number'
          ? { minLength: field.minLength }
          : {}),
        ...(typeof field.maxLength === 'number'
          ? { maxLength: field.maxLength }
          : {}),
        ...(typeof field.defaultValue === 'string' && field.defaultValue
          ? { default: field.defaultValue }
          : {}),
      }
  }
}

function layoutWidthFromUi(
  ui: UiSchema | undefined,
): DynamicFormField['layoutWidth'] {
  const options = ui?.['ui:options'] as
    | { layoutWidth?: string }
    | undefined
  return options?.layoutWidth === 'half' ? 'half' : undefined
}

function withLayoutWidth(ui: UiSchema, field: DynamicFormField): UiSchema {
  if (field.layoutWidth !== 'half') return ui
  const prev = (ui['ui:options'] as Record<string, unknown> | undefined) ?? {}
  return {
    ...ui,
    'ui:options': {
      ...prev,
      layoutWidth: 'half',
    },
  }
}

function fieldToUi(field: DynamicFormField): UiSchema {
  if (field.type === 'group') {
    const ui: UiSchema = withLayoutWidth(
      {
        'ui:order': [...(field.children ?? []).map((c) => c.key), '*'],
      },
      field,
    )
    for (const child of field.children ?? []) {
      ui[child.key] = fieldToUi(child)
    }
    return ui
  }

  if (field.type === 'table') {
    const columns = field.children ?? []
    const itemsUi: UiSchema = {
      'ui:order': [...columns.map((c) => c.key), '*'],
    }
    for (const col of columns) {
      itemsUi[col.key] = fieldToUi(col)
    }
    return withLayoutWidth(
      {
        'ui:field': 'dynamicTable',
        'ui:options': {
          // Rows come from backend formData — no client add/remove.
          addable: false,
          removable: false,
        },
        items: itemsUi,
      },
      field,
    )
  }

  const ui: UiSchema = withLayoutWidth({}, field)
  if (field.placeholder) ui['ui:placeholder'] = field.placeholder
  if (field.helpText) ui['ui:help'] = field.helpText
  if (field.type === 'textarea') {
    ui['ui:widget'] = 'textarea'
    ui['ui:options'] = {
      ...((ui['ui:options'] as object | undefined) ?? {}),
      rows: 4,
    }
  }
  if (field.type === 'password') ui['ui:widget'] = 'password'
  if (field.type === 'email') ui['ui:widget'] = 'email'
  if (field.type === 'radio') {
    ui['ui:widget'] = 'radio'
    ui['ui:enumNames'] = enumOptions(field).map((o) => o.label)
  }
  if (field.type === 'select') {
    ui['ui:enumNames'] = enumOptions(field).map((o) => o.label)
  }
  return ui
}

function defaultFormData(field: DynamicFormField): unknown {
  if (field.type === 'group') {
    const data: Record<string, unknown> = {}
    for (const child of field.children ?? []) {
      const value = defaultFormData(child)
      if (value !== undefined) data[child.key] = value
    }
    return data
  }
  if (field.type === 'table') {
    return []
  }
  if (field.defaultValue !== undefined && field.defaultValue !== '') {
    return field.defaultValue
  }
  if (field.type === 'checkbox') return false
  return undefined
}

export function fieldsToRjsf(
  fields: readonly DynamicFormField[],
  meta?: { title?: string; description?: string },
): {
  schema: RJSFSchema
  uiSchema: UiSchema
  formData: Record<string, unknown>
} {
  const properties: Record<string, RJSFSchema> = {}
  const required: string[] = []
  const uiSchema: UiSchema = {
    'ui:order': [...fields.map((f) => f.key), '*'],
  }
  const formData: Record<string, unknown> = {}

  for (const field of fields) {
    properties[field.key] = fieldToProperty(field)
    uiSchema[field.key] = fieldToUi(field)
    // Groups with required children must be required on the parent, otherwise
    // AJV never validates the nested `required` when the object is omitted.
    if (fieldHasRequired(field)) required.push(field.key)
    const value = defaultFormData(field)
    if (value !== undefined) formData[field.key] = value
  }

  return {
    schema: {
      title: meta?.title,
      description: meta?.description,
      type: 'object',
      properties,
      ...(required.length ? { required } : {}),
    },
    uiSchema,
    formData,
  }
}

function inferType(
  prop: RJSFSchema,
  ui: UiSchema | undefined,
): DynamicFormFieldType {
  if (prop.type === 'array') return 'table'
  if (prop.type === 'object') return 'group'
  if (prop.type === 'boolean') return 'checkbox'
  if (prop.type === 'number' || prop.type === 'integer') return 'number'
  if (prop.format === 'date' || prop.format === 'date-time') return 'date'
  if (prop.format === 'email' || ui?.['ui:widget'] === 'email') return 'email'
  if (ui?.['ui:widget'] === 'password') return 'password'
  if (ui?.['ui:widget'] === 'radio') return 'radio'
  if (Array.isArray(prop.enum)) return 'select'
  if (ui?.['ui:widget'] === 'textarea') return 'textarea'
  return 'text'
}

function optionsFromEnum(
  prop: RJSFSchema,
  ui: UiSchema | undefined,
): DynamicFormSelectOption[] {
  const values = Array.isArray(prop.enum) ? prop.enum.map(String) : []
  const names = Array.isArray(ui?.['ui:enumNames'])
    ? (ui!['ui:enumNames'] as string[])
    : values
  return values.map((value, index) => ({
    id: createOptionId(),
    value,
    label: names[index] ?? value,
    enabled: true,
  }))
}

function propertyToField(
  key: string,
  prop: RJSFSchema,
  ui: UiSchema | undefined,
  requiredSet: Set<string>,
): DynamicFormField | null {
  if (!isPlainSchema(prop)) return null
  if (prop.oneOf || prop.anyOf || prop.allOf) return null

  if (prop.type === 'array') {
    const items = prop.items
    if (!isPlainSchema(items) || items.type !== 'object') return null
    const childProps = (items.properties ?? {}) as Record<string, RJSFSchema>
    const childRequired = new Set(items.required ?? [])
    const itemsUi = (ui?.items as UiSchema | undefined) ?? {}
    const childOrder = Array.isArray(itemsUi['ui:order'])
      ? (itemsUi['ui:order'] as string[]).filter((k) => k !== '*')
      : Object.keys(childProps)
    const childKeys = [
      ...childOrder.filter((k) => k in childProps),
      ...Object.keys(childProps).filter((k) => !childOrder.includes(k)),
    ]
    const children: DynamicFormField[] = []
    for (const childKey of childKeys) {
      const child = propertyToField(
        childKey,
        childProps[childKey],
        (itemsUi[childKey] as UiSchema | undefined) ?? {},
        childRequired,
      )
      if (child && child.type !== 'group' && child.type !== 'table') {
        children.push(child)
      }
    }
    return {
      id: createFieldId(),
      key,
      type: 'table',
      label: typeof prop.title === 'string' ? prop.title : key,
      required: requiredSet.has(key) || Boolean(prop.minItems && prop.minItems > 0),
      helpText:
        typeof prop.description === 'string'
          ? prop.description
          : typeof ui?.['ui:help'] === 'string'
            ? ui['ui:help']
            : '',
      children,
      ...(layoutWidthFromUi(ui) ? { layoutWidth: layoutWidthFromUi(ui) } : {}),
    }
  }

  if (prop.type === 'object') {
    const childProps = (prop.properties ?? {}) as Record<string, RJSFSchema>
    const childRequired = new Set(prop.required ?? [])
    const childOrder = Array.isArray(ui?.['ui:order'])
      ? (ui!['ui:order'] as string[]).filter((k) => k !== '*')
      : Object.keys(childProps)
    const childKeys = [
      ...childOrder.filter((k) => k in childProps),
      ...Object.keys(childProps).filter((k) => !childOrder.includes(k)),
    ]
    const children: DynamicFormField[] = []
    for (const childKey of childKeys) {
      const child = propertyToField(
        childKey,
        childProps[childKey],
        (ui?.[childKey] as UiSchema | undefined) ?? {},
        childRequired,
      )
      if (child) children.push(child)
    }
    return {
      id: createFieldId(),
      key,
      type: 'group',
      label: typeof prop.title === 'string' ? prop.title : key,
      helpText:
        typeof prop.description === 'string'
          ? prop.description
          : typeof ui?.['ui:help'] === 'string'
            ? ui['ui:help']
            : '',
      children,
      ...(layoutWidthFromUi(ui) ? { layoutWidth: layoutWidthFromUi(ui) } : {}),
    }
  }

  const type = inferType(prop, ui)
  const field: DynamicFormField = {
    id: createFieldId(),
    key,
    type,
    label: typeof prop.title === 'string' ? prop.title : key,
    required: requiredSet.has(key),
    placeholder:
      typeof ui?.['ui:placeholder'] === 'string' ? ui['ui:placeholder'] : '',
    helpText: typeof ui?.['ui:help'] === 'string' ? ui['ui:help'] : '',
    remindWhenEmpty: false,
    ...(layoutWidthFromUi(ui) ? { layoutWidth: layoutWidthFromUi(ui) } : {}),
    ...(typeof prop.minLength === 'number' ? { minLength: prop.minLength } : {}),
    ...(typeof prop.maxLength === 'number' ? { maxLength: prop.maxLength } : {}),
    ...(typeof prop.minimum === 'number' ? { minimum: prop.minimum } : {}),
    ...(typeof prop.maximum === 'number' ? { maximum: prop.maximum } : {}),
    ...(typeof prop.multipleOf === 'number'
      ? { multipleOf: prop.multipleOf }
      : {}),
  }

  if (type === 'select' || type === 'radio') {
    field.options = optionsFromEnum(prop, ui)
    field.selectConfig = { quickEdit: false, sortAlphabetical: false }
  }

  if (prop.default !== undefined) {
    field.defaultValue = prop.default as string | number | boolean
  } else if (type === 'checkbox') {
    field.defaultValue = false
  }

  return field
}

export function rjsfToFields(
  schema: RJSFSchema | undefined,
  uiSchema?: UiSchema,
): DynamicFormField[] {
  if (!schema?.properties || typeof schema.properties !== 'object') return []

  const props = schema.properties as Record<string, RJSFSchema>
  const required = new Set(schema.required ?? [])
  const order = Array.isArray(uiSchema?.['ui:order'])
    ? (uiSchema!['ui:order'] as string[]).filter((k) => k !== '*')
    : Object.keys(props)

  const keys = [
    ...order.filter((k) => k in props),
    ...Object.keys(props).filter((k) => !order.includes(k)),
  ]

  const fields: DynamicFormField[] = []
  for (const key of keys) {
    const field = propertyToField(
      key,
      props[key],
      (uiSchema?.[key] as UiSchema | undefined) ?? {},
      required,
    )
    if (field) fields.push(field)
  }
  return fields
}

function preserveFormData(
  previous: Record<string, unknown> | undefined,
  schema: RJSFSchema,
  defaults: Record<string, unknown>,
): Record<string, unknown> {
  const allowed = new Set(Object.keys(schema.properties ?? {}))
  const next: Record<string, unknown> = { ...defaults }
  for (const [key, value] of Object.entries(previous ?? {})) {
    if (!allowed.has(key)) continue
    const prop = (schema.properties as Record<string, RJSFSchema> | undefined)?.[
      key
    ]
    if (
      isPlainSchema(prop) &&
      prop.type === 'object' &&
      value &&
      typeof value === 'object' &&
      !Array.isArray(value)
    ) {
      next[key] = preserveFormData(
        value as Record<string, unknown>,
        prop,
        (defaults[key] as Record<string, unknown>) ?? {},
      )
    } else if (isPlainSchema(prop) && prop.type === 'array' && Array.isArray(value)) {
      next[key] = value
    } else {
      next[key] = value
    }
  }
  return next
}

export function syncDefinitionFromFields<
  T extends {
    name: string
    description?: string
    fields: DynamicFormField[]
    schema: RJSFSchema
    uiSchema?: UiSchema
    formData?: Record<string, unknown>
  },
>(form: T): T {
  const { schema, uiSchema, formData } = fieldsToRjsf(form.fields, {
    title: form.name,
    description: form.description,
  })
  return {
    ...form,
    schema,
    uiSchema,
    formData: preserveFormData(form.formData, schema, formData),
  }
}
