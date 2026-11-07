import type {
  CreateDynamicFormPayload,
  CreateDynamicFormSubmissionPayload,
  DynamicFormDefinition,
  DynamicFormListItem,
  DynamicFormSubmission,
  UpdateDynamicFormPayload,
} from '../types'
import {
  getDynamicFormDefinitionMock,
  getDynamicFormListMock,
} from '../mocks'
import {
  createEmptySchema,
  createFormId,
  createSubmissionId,
  fieldsToRjsf,
  rjsfToFields,
  syncDefinitionFromFields,
} from '../utils'

/** In-memory store seeded from JSON mocks (no localStorage). */
let formsMemory: DynamicFormDefinition[] | null = null
let submissionsMemory: DynamicFormSubmission[] = []

function clone<T>(value: T): T {
  return structuredClone(value)
}

function normalizeForm(form: DynamicFormDefinition): DynamicFormDefinition {
  const fields =
    Array.isArray(form.fields) && form.fields.length > 0
      ? form.fields
      : rjsfToFields(form.schema, form.uiSchema)
  return syncDefinitionFromFields({ ...form, fields })
}

function loadSeedForms(): DynamicFormDefinition[] {
  const definition = normalizeForm(
    clone(getDynamicFormDefinitionMock as DynamicFormDefinition),
  )
  const list = getDynamicFormListMock as DynamicFormListItem[]
  const extras = list
    .filter((item) => item.id !== definition.id)
    .map((item) =>
      normalizeForm({
        id: item.id,
        name: item.name,
        description: item.description,
        version: item.version,
        fields: [],
        schema: createEmptySchema(item.name),
        uiSchema: {},
        formData: {},
        createdAt: item.createdAt,
        updatedAt: item.updatedAt,
      }),
    )
  return [definition, ...extras]
}

function getForms(): DynamicFormDefinition[] {
  if (!formsMemory) formsMemory = loadSeedForms()
  return formsMemory
}

function toListItem(form: DynamicFormDefinition): DynamicFormListItem {
  const normalized = normalizeForm(form)
  return {
    id: normalized.id,
    name: normalized.name,
    description: normalized.description,
    version: normalized.version,
    createdAt: normalized.createdAt,
    updatedAt: normalized.updatedAt,
    fieldCount: normalized.fields.length,
  }
}

/** Reload forms from JSON mocks (used by Reset). */
export function resetFormsFromMockJson() {
  formsMemory = loadSeedForms()
  submissionsMemory = []
  return getForms().map((f) => normalizeForm(clone(f)))
}

export function listFormsLocal(): DynamicFormListItem[] {
  return getForms()
    .map(toListItem)
    .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))
}

export function getFormLocal(id: string): DynamicFormDefinition | null {
  const form = getForms().find((f) => f.id === id) ?? null
  return form ? normalizeForm(clone(form)) : null
}

export function createFormLocal(
  payload: CreateDynamicFormPayload,
): DynamicFormDefinition {
  const now = new Date().toISOString()
  const name = payload.name.trim() || 'Form mới'
  const fields = payload.fields ?? []
  const fromFields = fieldsToRjsf(fields, {
    title: name,
    description: payload.description,
  })
  const form = normalizeForm({
    id: createFormId(),
    name,
    description: payload.description?.trim() || '',
    version: 1,
    fields,
    schema: payload.schema ?? fromFields.schema ?? createEmptySchema(name),
    uiSchema: payload.uiSchema ?? fromFields.uiSchema,
    formData: payload.formData ?? fromFields.formData,
    createdAt: now,
    updatedAt: now,
  })
  getForms().unshift(form)
  return clone(form)
}

export function updateFormLocal(
  payload: UpdateDynamicFormPayload,
): DynamicFormDefinition {
  const forms = getForms()
  const index = forms.findIndex((f) => f.id === payload.id)
  if (index < 0) throw new Error('Không tìm thấy form')
  const current = normalizeForm(forms[index])
  const fields = payload.fields ?? current.fields
  const draft: DynamicFormDefinition = {
    ...current,
    name: payload.name?.trim() || current.name,
    description:
      payload.description !== undefined
        ? payload.description.trim()
        : current.description,
    fields,
    schema: payload.schema ?? current.schema,
    uiSchema:
      payload.uiSchema !== undefined ? payload.uiSchema : current.uiSchema,
    formData:
      payload.formData !== undefined ? payload.formData : current.formData,
  }
  const next = syncDefinitionFromFields({
    ...draft,
    fields,
  })
  const schemaChanged =
    payload.fields !== undefined ||
    payload.schema !== undefined ||
    payload.uiSchema !== undefined
  const saved: DynamicFormDefinition = {
    ...next,
    version: current.version + (schemaChanged ? 1 : 0),
    updatedAt: new Date().toISOString(),
  }
  forms[index] = saved
  return clone(saved)
}

export function deleteFormLocal(id: string) {
  formsMemory = getForms().filter((f) => f.id !== id)
  submissionsMemory = submissionsMemory.filter((s) => s.formId !== id)
}

export function createSubmissionLocal(
  payload: CreateDynamicFormSubmissionPayload,
): DynamicFormSubmission {
  const submission: DynamicFormSubmission = {
    id: createSubmissionId(),
    formId: payload.formId,
    data: payload.data,
    createdAt: new Date().toISOString(),
  }
  submissionsMemory = [submission, ...submissionsMemory]
  return clone(submission)
}

export function listSubmissionsLocal(formId: string): DynamicFormSubmission[] {
  return submissionsMemory
    .filter((s) => s.formId === formId)
    .sort((a, b) => b.createdAt.localeCompare(a.createdAt))
    .map((s) => clone(s))
}
