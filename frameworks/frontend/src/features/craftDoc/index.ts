// Pages

export { CraftDocBuilderPage } from './pages/CraftDocBuilderPage'

export type {

  CraftDocBuilderPageProps,

  CraftDocBuilderPageContentContext,

} from './pages/CraftDocBuilderPage'



// Components

export { CraftDocPageShell } from './components/CraftDocPageShell'

export type { CraftDocPageShellProps } from './components/CraftDocPageShell'



export { DocxTemplateBuilder } from './components/DocxTemplateBuilder'

export type { DocxTemplateBuilderProps } from './components/DocxTemplateBuilder'



export { DocumentDataForm } from './components/DocumentDataForm'

export type { DocumentDataFormProps } from './components/DocumentDataForm'



export { DocumentPreviewPane } from './components/DocumentPreviewPane'

export type { DocumentPreviewPaneProps } from './components/DocumentPreviewPane'



export { TemplateDesignDrawer } from './components/TemplateDesignDrawer'

export type { TemplateDesignDrawerProps } from './components/TemplateDesignDrawer'



export { DocxPreview } from './components/DocxPreview'

export type { DocxPreviewProps } from './components/DocxPreview'



export { TemplateSidebar, TemplateList, FieldList } from './components/TemplateSidebar'

export type {

  TemplateSidebarProps,

  TemplateListProps,

  FieldListProps,

} from './components/TemplateSidebar'



export { DocumentWorkspace, DocumentToolbar, PageControls } from './components/DocumentWorkspace'

export type {

  DocumentWorkspaceProps,

  DocumentToolbarProps,

  PageControlsProps,

} from './components/DocumentWorkspace'



export { FieldConfigurationPanel } from './components/FieldConfigurationPanel'

export type { FieldConfigurationPanelProps } from './components/FieldConfigurationPanel'



export { AddFieldModal } from './components/AddFieldModal'

export type { AddFieldModalProps } from './components/AddFieldModal'



export { DeleteFieldDialog } from './components/DeleteFieldDialog'

export type { DeleteFieldDialogProps } from './components/DeleteFieldDialog'



export { FilledPreviewPanel } from './components/FilledPreviewPanel'

export type { FilledPreviewPanelProps } from './components/FilledPreviewPanel'



// Types

export {

  FieldType,

  FIELD_TYPE_LABEL,

  FIELD_TYPE_OPTIONS,

} from './types'

export type {

  FieldTypeValue,

  DocumentField,

  DocumentTemplate,

  PreviewMode,

  DocxPreviewStatus,

  CraftDocBuilderState as CraftDocBuilderStateModel,

  CreateDocumentFieldPayload,

  UpdateDocumentFieldPayload,

  CraftDocBuilderController,

  CraftDocPreviewControls,

  CraftDocTemplateDesignControls,

  CraftDocFormPanelProps,

  CraftDocPreviewPanelProps,

  CraftDocTemplateDesignDrawerProps,

} from './types'



// Constants

export {

  BASE_URL_CRAFT_DOC,

  CRAFT_DOC_ZOOM_MIN,

  CRAFT_DOC_ZOOM_MAX,

  CRAFT_DOC_ZOOM_STEP,

  CRAFT_DOC_ZOOM_DEFAULT,

  FAKE_FILLED_SAMPLE_DATA,

  getFakeFilledSampleData,

  getDefaultFieldValuesMap,

  LLA_IDAS_SAMPLE_DATA,

  getLlaIdasSampleData,

  CRAFT_DOC_PAGE_LETTER,

  CRAFT_DOC_PAGE_A4,

} from './constants'



// Hooks

export { useCraftDocBuilderState, buildCraftDocController } from './hooks'

export type {

  UseCraftDocBuilderStateOptions,

  CraftDocBuilderState,

  BuildCraftDocControllerOptions,

} from './hooks'



// Services

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

  callGetCraftDocTemplates,

  callGetCraftDocTemplateDetail,

  callGetCraftDocTemplateSampleData,

  callLoadCraftDocTemplateFromApi,

  callSaveCraftDocDocument,

  callExportCraftDocDocument,

  callUploadCraftDocTemplate,

  resolveCraftDocTemplateFromFile,

} from './services'

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

} from './services'



export { getCraftDocTemplatesMock, getLlaIdasTemplateDetailMock, getCraftDocTemplatesListMock } from './mocks'

export type { CraftDocTemplateDetailMock } from './mocks'



// Routes

export {

  CRAFT_DOC_ROUTES,

  getCraftDocRouteList,

  getCraftDocBuilderPath,

  configureCraftDocNavigate,

  navigateCraftDoc,

  craftDocPaths,

} from './routes'

export type { CraftDocRouteKey, CraftDocRouteItem, CraftDocNavigateFn } from './routes'



// Menu / permission / i18n / theme / utils

export { craftDocMenuItems } from './menu'

export type { CraftDocMenuItem } from './menu'



export {

  CRAFT_DOC_PERMISSIONS,

  hasCraftDocPermission,

} from './permission'

export type { CraftDocPermissionKey } from './permission'



export {

  craftDocMessages,

  getCraftDocMessages,

} from './localization'

export type { CraftDocLocale, CraftDocMessages } from './localization'



export { defaultCraftDocTheme } from './theme'

export type { CraftDocTheme } from './theme'



export {

  toPlaceholder,

  createFieldFromPlaceholder,

  createFieldFromPayload,

  createDocxBlob,

  createSampleDocxBlob,

  SAMPLE_DOCX_DEFINITIONS,

  resolveCraftDocContent,

  toPlaceholderMap,

  ensureFieldValues,

  mergeParsedFieldsWithValues,

  applyValuesMapToFields,

  buildFieldValuesFromFields,

} from './utils'

export type { CraftDocSlotContent } from './utils'


