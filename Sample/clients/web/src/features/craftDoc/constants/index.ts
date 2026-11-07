import { getLaborContractSampleData, LABOR_CONTRACT_SAMPLE_DATA } from './sampleContractData'
import { getSampleBriefSampleData, SAMPLE_BRIEF_SAMPLE_DATA } from './sampleBriefSampleData'

export { getLaborContractSampleData, LABOR_CONTRACT_SAMPLE_DATA } from './sampleContractData'
export { getSampleBriefSampleData, SAMPLE_BRIEF_SAMPLE_DATA } from './sampleBriefSampleData'
export { CRAFT_DOC_PAGE_LETTER, CRAFT_DOC_PAGE_A4 } from './pageSize'

export const BASE_URL_CRAFT_DOC = '/craft-doc'

export const CRAFT_DOC_ZOOM_MIN = 50
export const CRAFT_DOC_ZOOM_MAX = 200
export const CRAFT_DOC_ZOOM_STEP = 10
export const CRAFT_DOC_ZOOM_DEFAULT = 100

/** Dữ liệu mẫu mặc định (trống — người dùng tự nhập) */
export const FAKE_FILLED_SAMPLE_DATA = LABOR_CONTRACT_SAMPLE_DATA

/** Giá trị mặc định theo template (legacy map — merge vào fields[].value). */
export function getDefaultFieldValuesMap(templateId?: string | null): Record<string, string> {
  if (templateId === 'sample-brief') return getSampleBriefSampleData()
  if (templateId?.startsWith('upload-')) return {}
  return getLaborContractSampleData()
}

/** @deprecated Dùng getDefaultFieldValuesMap + fields[].value */
export function getFakeFilledSampleData(templateId?: string | null): Record<string, string> {
  return getDefaultFieldValuesMap(templateId)
}
