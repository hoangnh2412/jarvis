import { SAMPLE_DOCX_DEFINITIONS } from '../utils'
import templatesListSeed from './get-templates-list.json'
import llaIdasTemplateSeed from './get-lla-idas-template.json'
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

/** Metadata + fields + sampleData LLA-IDAS (`get-lla-idas-template.json`). */
export function getLlaIdasTemplateDetailMock(): CraftDocTemplateDetailMock {
  return {
    template: llaIdasTemplateSeed.template,
    fields: llaIdasTemplateSeed.fields as DocumentField[],
    sampleData: llaIdasTemplateSeed.sampleData,
  }
}

export { LABOR_CONTRACT_SAMPLE_DATA, getLaborContractSampleData } from '../constants/sampleContractData'
export { LLA_IDAS_SAMPLE_DATA, getLlaIdasSampleData } from '../constants/llaIdasSampleData'
