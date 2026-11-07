export {
  readDocxInput,
  parseTemplate,
  extractFields,
  replaceFields,
  generateDocument,
  removeFieldFromDocument,
  addFieldToDocument,
  renameFieldInDocument,
  parseTemplateFile,
  bufferToBlob,
  downloadBlob,
} from './docxTemplateService'

export {
  callGetCraftDocTemplates,
  callGetCraftDocTemplateDetail,
  callGetCraftDocTemplateSampleData,
  callLoadCraftDocTemplateFromApi,
  callSaveCraftDocDocument,
  callExportCraftDocDocument,
  callUploadCraftDocTemplate,
  resolveCraftDocTemplateFromFile,
} from './craftDoc'
export type {
  CraftDocPageSizeMeta,
  CraftDocMarginsTwips,
  CraftDocTemplateMeta,
  CraftDocTemplateListResult,
  CraftDocTemplateDetailResult,
  CraftDocTemplateDetailApiResponse,
  CraftDocSampleDataResult,
  CraftDocSavePayload,
  CraftDocExportPayload,
  CraftDocSaveResult,
  CraftDocExportResult,
  CraftDocUploadPayload,
  CraftDocUploadApiResult,
} from './craftDoc'
