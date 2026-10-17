import { SAMPLE_DOCX_DEFINITIONS } from '../utils'
import templatesListSeed from './get-templates-list.json'
import sampleBriefTemplateSeed from './get-sample-brief-template.json'
import type { DocumentField } from '../types'

export type CraftDocTemplateDetailMock = {
  template: {
    id: string
    name: string
    fileName: string
    updatedAt?: string
    pageSize?: Record<string, unknown>
    marginsTwips?: Record<string, number>
    contentWidthTwips?: number
    contentWidthPx96Dpi?: number
  }
  fields: DocumentField[]
  sampleData: Record<string, string>
}

export const craftDocTemplatesMock = SAMPLE_DOCX_DEFINITIONS.map((item) => ({
  id: item.id,
  name: item.name,
  fileName: item.fileName,
}))

export function getCraftDocTemplatesMock() {
  return craftDocTemplatesMock
}

/** Danh sách mẫu từ file seed mock API (`get-templates-list.json`). */
export function getCraftDocTemplatesListMock() {
  return templatesListSeed.templates
}

/** Metadata + fields + sampleData Sample-Brief (`get-sample-brief-template.json`). */
export function getSampleBriefTemplateDetailMock(): CraftDocTemplateDetailMock {
  return {
    template: sampleBriefTemplateSeed.template,
    fields: sampleBriefTemplateSeed.fields as DocumentField[],
    sampleData: sampleBriefTemplateSeed.sampleData,
  }
}

export { LABOR_CONTRACT_SAMPLE_DATA, getLaborContractSampleData } from '../constants/sampleContractData'
export { SAMPLE_BRIEF_SAMPLE_DATA, getSampleBriefSampleData } from '../constants/sampleBriefSampleData'
