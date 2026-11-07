import type {
  CreateDynamicFormPayload,
  CreateDynamicFormSubmissionPayload,
  DynamicFormDefinition,
  DynamicFormListItem,
  DynamicFormSubmission,
  UpdateDynamicFormPayload,
} from '../types'
import {
  createFormLocal,
  createSubmissionLocal,
  deleteFormLocal,
  getFormLocal,
  listFormsLocal,
  listSubmissionsLocal,
  updateFormLocal,
} from './dynamicFormStore'

/**
 * Service layer — fake bằng JSON mocks (`features/dynamicForm/mocks/*.json`).
 * Runtime giữ bản sao in-memory để Save/Reset trong session.
 * Khi có BE, đổi sang axios `v1/forms` giữ nguyên chữ ký.
 */
function asData<T>(data: T) {
  return Promise.resolve({ data })
}

export const callGetDynamicFormList = async () =>
  asData<DynamicFormListItem[]>(listFormsLocal())

export const callGetDynamicForm = async (id: string) => {
  const form = getFormLocal(id)
  if (!form) throw new Error('Không tìm thấy form')
  return asData<DynamicFormDefinition>(form)
}

export const callCreateDynamicForm = async (payload: CreateDynamicFormPayload) =>
  asData<DynamicFormDefinition>(createFormLocal(payload))

export const callUpdateDynamicForm = async (payload: UpdateDynamicFormPayload) =>
  asData<DynamicFormDefinition>(updateFormLocal(payload))

export const callDeleteDynamicForm = async (id: string) => {
  deleteFormLocal(id)
  return asData<{ id: string }>({ id })
}

export const callCreateDynamicFormSubmission = async (
  payload: CreateDynamicFormSubmissionPayload,
) => asData<DynamicFormSubmission>(createSubmissionLocal(payload))

export const callGetDynamicFormSubmissions = async (formId: string) =>
  asData<DynamicFormSubmission[]>(listSubmissionsLocal(formId))
