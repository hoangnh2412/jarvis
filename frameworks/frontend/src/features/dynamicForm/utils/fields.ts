import type {
  DynamicFormField,
  DynamicFormFieldType,
  DynamicFormSelectOption,
  DynamicFormTableColumnType,
} from '../types'

export const TABLE_COLUMN_TYPES: DynamicFormTableColumnType[] = [
  'text',
  'number',
  'email',
  'select',
  'checkbox',
  'date',
]

export function isTableColumnType(
  type: string,
): type is DynamicFormTableColumnType {
  return (TABLE_COLUMN_TYPES as string[]).includes(type)
}

export function createFieldId() {
  return `fld_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`
}

export function createOptionId() {
  return `opt_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`
}

export const FIELD_TYPE_META: Record<
  DynamicFormFieldType,
  {
    label: string
    defaultLabel: string
    defaultKey: string
    description: string
  }
> = {
  text: {
    label: 'Single-Line Input',
    defaultLabel: 'Text',
    defaultKey: 'text',
    description: 'Single line text',
  },
  number: {
    label: 'Number',
    defaultLabel: 'Number',
    defaultKey: 'number',
    description: 'Numeric value',
  },
  email: {
    label: 'Email',
    defaultLabel: 'Email',
    defaultKey: 'email',
    description: 'Email address',
  },
  password: {
    label: 'Password',
    defaultLabel: 'Password',
    defaultKey: 'password',
    description: 'Masked text input',
  },
  select: {
    label: 'Dropdown',
    defaultLabel: 'Select',
    defaultKey: 'select',
    description: 'Choose one option',
  },
  radio: {
    label: 'Radio Button Group',
    defaultLabel: 'Radio',
    defaultKey: 'radio',
    description: 'Choose one from list',
  },
  checkbox: {
    label: 'Checkbox',
    defaultLabel: 'I agree to the terms and conditions',
    defaultKey: 'agree',
    description: 'True / false toggle',
  },
  date: {
    label: 'Date',
    defaultLabel: 'Date',
    defaultKey: 'date',
    description: 'Date picker',
  },
  textarea: {
    label: 'Long Text',
    defaultLabel: 'Description',
    defaultKey: 'description',
    description: 'Multi-line text',
  },
  group: {
    label: 'Form Group',
    defaultLabel: 'Section',
    defaultKey: 'section',
    description: 'Group fields into a section',
  },
  table: {
    label: 'Dynamic Table',
    defaultLabel: 'Line items',
    defaultKey: 'lineItems',
    description: 'Editable rows with PrimeReact DataTable',
  },
}

export const FIELD_LIBRARY_TYPES: DynamicFormFieldType[] = [
  'text',
  'number',
  'email',
  'password',
  'select',
  'radio',
  'checkbox',
  'date',
  'textarea',
  'group',
  'table',
]

export type FieldLibraryGroupId =
  | 'basic'
  | 'advanced'
  | 'layout'
  | 'data'
  | 'premium'

export type FieldLibraryGroup = {
  id: FieldLibraryGroupId
  label: string
  types: DynamicFormFieldType[]
}

export const FIELD_LIBRARY_GROUPS: FieldLibraryGroup[] = [
  {
    id: 'basic',
    label: 'Basic',
    types: ['text', 'number', 'textarea', 'checkbox', 'select', 'radio'],
  },
  {
    id: 'advanced',
    label: 'Advanced',
    types: ['email', 'password', 'date'],
  },
  {
    id: 'layout',
    label: 'Layout',
    types: ['group'],
  },
  {
    id: 'data',
    label: 'Data',
    types: ['table'],
  },
  {
    id: 'premium',
    label: 'Premium',
    types: [],
  },
]

export function createSelectOption(
  label: string,
  value?: string,
): DynamicFormSelectOption {
  const fromLabel = label
    .trim()
    .toLowerCase()
    .replace(/\s+/g, '_')
    .replace(/[^\w-]/g, '')
  const safe =
    value ?? (fromLabel || `option_${Date.now().toString(36).slice(-4)}`)
  return {
    id: createOptionId(),
    label,
    value: safe,
    enabled: true,
  }
}

export function normalizeSelectOptions(
  options: DynamicFormSelectOption[] | undefined,
): DynamicFormSelectOption[] {
  return (options ?? []).map((option, index) => ({
    id: option.id || `opt_legacy_${index}`,
    label: option.label,
    value: option.value,
    enabled: option.enabled !== false,
  }))
}

export function flattenFields(
  fields: readonly DynamicFormField[],
): DynamicFormField[] {
  const out: DynamicFormField[] = []
  for (const field of fields) {
    out.push(field)
    // Groups nest form fields; table children are column defs (not form paths).
    if (field.type === 'group' && field.children?.length) {
      out.push(...flattenFields(field.children))
    }
  }
  return out
}

export function collectFieldKeys(
  fields: readonly DynamicFormField[],
): string[] {
  return flattenFields(fields).map((f) => f.key)
}

export function findFieldById(
  fields: readonly DynamicFormField[],
  id: string,
): DynamicFormField | null {
  for (const field of fields) {
    if (field.id === id) return field
    if (field.type === 'group' && field.children) {
      const found = findFieldById(field.children, id)
      if (found) return found
    }
  }
  return null
}

export function findFieldByKey(
  fields: readonly DynamicFormField[],
  key: string,
): DynamicFormField | null {
  for (const field of fields) {
    if (field.key === key) return field
    if (field.type === 'group' && field.children) {
      const found = findFieldByKey(field.children, key)
      if (found) return found
    }
  }
  return null
}

/** Returns parent group key, or null if field is at root. */
function locateParent(
  fields: readonly DynamicFormField[],
  childKey: string,
): { found: boolean; parentKey: string | null } {
  for (const field of fields) {
    if (field.key === childKey) return { found: true, parentKey: null }
    if (field.type === 'group' && field.children?.length) {
      const child = locateParent(field.children, childKey)
      if (child.found) {
        return {
          found: true,
          parentKey: child.parentKey === null ? field.key : child.parentKey,
        }
      }
    }
  }
  return { found: false, parentKey: null }
}

export function getParentKeyOfField(
  fields: readonly DynamicFormField[],
  childKey: string,
): string | null {
  const result = locateParent(fields, childKey)
  return result.found ? result.parentKey : null
}

export function updateFieldInTree(
  fields: DynamicFormField[],
  next: DynamicFormField,
): DynamicFormField[] {
  return fields.map((field) => {
    if (field.id === next.id) return next
    if (field.type === 'group' && field.children) {
      return {
        ...field,
        children: updateFieldInTree(field.children, next),
      }
    }
    return field
  })
}

export function removeFieldFromTree(
  fields: DynamicFormField[],
  id: string,
): DynamicFormField[] {
  return fields
    .filter((field) => field.id !== id)
    .map((field) =>
      field.type === 'group' && field.children
        ? { ...field, children: removeFieldFromTree(field.children, id) }
        : field,
    )
}

export function insertFieldInTree(
  fields: DynamicFormField[],
  field: DynamicFormField,
  options?: {
    parentKey?: string | null
    beforeKey?: string
    afterKey?: string
  },
): DynamicFormField[] {
  const parentKey = options?.parentKey ?? null
  const beforeKey = options?.beforeKey
  const afterKey = options?.afterKey

  const insertInList = (list: DynamicFormField[]) => {
    const next = [...list]
    if (beforeKey) {
      const index = next.findIndex((f) => f.key === beforeKey)
      if (index >= 0) {
        next.splice(index, 0, field)
        return next
      }
    }
    if (afterKey) {
      const index = next.findIndex((f) => f.key === afterKey)
      if (index >= 0) {
        next.splice(index + 1, 0, field)
        return next
      }
    }
    next.push(field)
    return next
  }

  if (!parentKey) return insertInList(fields)

  return fields.map((f) => {
    if (f.key === parentKey && f.type === 'group') {
      return { ...f, children: insertInList(f.children ?? []) }
    }
    if (f.type === 'group' && f.children) {
      return {
        ...f,
        children: insertFieldInTree(f.children, field, options),
      }
    }
    return f
  })
}

export type FieldDropEdge = 'before' | 'after'

export function moveFieldInTree(
  fields: DynamicFormField[],
  fromKey: string,
  targetKey: string,
  edge: FieldDropEdge = 'before',
): DynamicFormField[] {
  if (fromKey === targetKey) return fields
  const from = findFieldByKey(fields, fromKey)
  if (!from) return fields
  if (from.type === 'group' && findFieldByKey(from.children ?? [], targetKey)) {
    return fields
  }
  const parentOfTarget = getParentKeyOfField(fields, targetKey)
  const without = removeFieldFromTree(fields, from.id)
  return insertFieldInTree(without, from, {
    parentKey: parentOfTarget,
    ...(edge === 'before'
      ? { beforeKey: targetKey }
      : { afterKey: targetKey }),
  })
}

export function moveFieldById(
  fields: DynamicFormField[],
  id: string,
  dir: -1 | 1,
): DynamicFormField[] {
  const moveInList = (list: DynamicFormField[]): DynamicFormField[] | null => {
    const index = list.findIndex((f) => f.id === id)
    if (index >= 0) {
      const nextIndex = index + dir
      if (nextIndex < 0 || nextIndex >= list.length) return list
      const next = [...list]
      const [item] = next.splice(index, 1)
      next.splice(nextIndex, 0, item)
      return next
    }
    for (let i = 0; i < list.length; i++) {
      const field = list[i]
      if (field.type === 'group' && field.children) {
        const children = moveInList(field.children)
        if (children) {
          const next = [...list]
          next[i] = { ...field, children }
          return next
        }
      }
    }
    return null
  }
  return moveInList(fields) ?? fields
}

export function createField(
  type: DynamicFormFieldType,
  existingKeys: string[] = [],
): DynamicFormField {
  const meta = FIELD_TYPE_META[type]
  let key = meta.defaultKey
  let n = 1
  while (existingKeys.includes(key)) {
    n += 1
    key = `${meta.defaultKey}_${n}`
  }
  const field: DynamicFormField = {
    id: createFieldId(),
    key,
    type,
    label: meta.defaultLabel,
    required: false,
    remindWhenEmpty: false,
    placeholder: '',
    helpText: '',
  }

  if (type === 'group') {
    field.children = []
    field.label = 'New section'
    field.helpText = 'Group related fields together'
    return field
  }

  if (type === 'table') {
    field.label = 'Line items'
    field.helpText = 'Rows are loaded from the server'
    field.children = [
      createTableColumn('text', ['label'], {
        key: 'label',
        label: 'Label',
      }),
      createTableColumn('number', ['label', 'amount'], {
        key: 'amount',
        label: 'Amount',
      }),
    ]
    return field
  }

  if (type === 'select' || type === 'radio') {
    field.options = [
      createSelectOption('Option 1', 'option_1'),
      createSelectOption('Option 2', 'option_2'),
      createSelectOption('Option 3', 'option_3'),
    ]
    field.selectConfig = { quickEdit: false, sortAlphabetical: false }
    field.defaultValue = ''
  }
  if (type === 'checkbox') {
    field.defaultValue = false
    field.label = 'I agree to the terms and conditions'
  }
  if (type === 'email') {
    field.placeholder = 'name@example.com'
  }
  if (type === 'password') {
    field.placeholder = 'Enter password'
  }
  if (type === 'number') {
    field.placeholder = '0'
  }
  if (type === 'text') {
    field.placeholder = 'Enter text'
    field.minLength = 0
    field.maxLength = 100
  }
  if (type === 'textarea') {
    field.placeholder = 'Enter details'
  }

  return field
}

/** Create a column definition for a `table` field. */
export function createTableColumn(
  type: DynamicFormTableColumnType,
  existingKeys: string[] = [],
  overrides?: Partial<Pick<DynamicFormField, 'key' | 'label'>>,
): DynamicFormField {
  const base = createField(type, existingKeys)
  return {
    ...base,
    key: overrides?.key ?? base.key,
    label: overrides?.label ?? base.label,
    // Columns are never nested containers.
    children: undefined,
  }
}

export type DynamicFormValidationIssue = {
  code: string
  message: string
  fieldId?: string
}

export function validateFieldDraft(
  field: DynamicFormField,
  allFields: readonly DynamicFormField[],
): DynamicFormValidationIssue[] {
  const issues: DynamicFormValidationIssue[] = []
  if (!field.label?.trim()) {
    issues.push({
      code: 'field.label.required',
      message: 'Label is required',
      fieldId: field.id,
    })
  }
  const key = field.key?.trim() ?? ''
  const flat = flattenFields(allFields)
  if (!key) {
    issues.push({
      code: 'field.key.required',
      message: 'Field name is required',
      fieldId: field.id,
    })
  } else if (!/^[a-zA-Z_][\w]*$/.test(key)) {
    issues.push({
      code: 'field.key.invalid',
      message:
        'Field name must start with a letter or _ and contain only letters, numbers, _',
      fieldId: field.id,
    })
  } else if (flat.some((f) => f.id !== field.id && f.key === key)) {
    issues.push({
      code: 'field.key.duplicate',
      message: `Field name "${key}" is already used`,
      fieldId: field.id,
    })
  }
  if (field.type === 'select' || field.type === 'radio') {
    const options = normalizeSelectOptions(field.options).filter(
      (o) => o.enabled !== false,
    )
    if (options.length === 0) {
      issues.push({
        code: 'field.select.empty',
        message: 'Add at least one option',
        fieldId: field.id,
      })
    }
  }
  if (field.type === 'table') {
    const columns = field.children ?? []
    if (columns.length === 0) {
      issues.push({
        code: 'field.table.empty',
        message: 'Add at least one column',
        fieldId: field.id,
      })
    }
    const seen = new Set<string>()
    for (const col of columns) {
      const colKey = col.key?.trim() ?? ''
      if (!colKey) {
        issues.push({
          code: 'field.table.column.key',
          message: 'Each column needs a field name',
          fieldId: field.id,
        })
      } else if (seen.has(colKey)) {
        issues.push({
          code: 'field.table.column.duplicate',
          message: `Column name "${colKey}" is duplicated`,
          fieldId: field.id,
        })
      } else {
        seen.add(colKey)
      }
      if (!col.label?.trim()) {
        issues.push({
          code: 'field.table.column.label',
          message: 'Each column needs a title',
          fieldId: field.id,
        })
      }
    }
  }
  return issues
}

export function firstValidationMessage(
  issues: DynamicFormValidationIssue[],
): string | undefined {
  return issues[0]?.message
}

export function canRemoveField(_field: DynamicFormField): boolean {
  return true
}
