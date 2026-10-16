export {
  IMPORT_EXPORT_API_KEY_HEADER,
  IMPORT_EXPORT_TENANT_HEADER,
  IMPORT_EXPORT_DEFAULT_TENANT_ID,
  IMPORT_EXPORT_PERMISSIONS,
  resolveImportExportTenantId,
} from './constants'
export type { ImportExportPermission } from './constants'

export type {
  ColumnMapping,
  SheetTemplate,
  ExcelTemplate,
  ApiEnvelope,
  PlatformBaseResponse,
  FileDownloadPayload,
  ImportErrorSummary,
  SheetImportResult,
  ImportResult,
  CreateTemplateInput,
  UpdateTemplateInput,
  UpdateTemplateMetadataInput,
  ImportExportEntitySummary,
  ImportExportFieldSchema,
} from './types'

export {
  IMPORT_EXPORT_ROUTES,
  getImportExportTemplateListPath,
  getImportExportTemplateCreatePath,
  getImportExportTemplateEditPath,
  getImportExportTemplateConfigurationPath,
  getImportExportTemplateImportPath,
  configureImportExportNavigate,
  navigateImportExport,
  importExportPaths,
} from './routes'
export type {
  ImportExportRouteKey,
  ImportExportNavigateFn,
} from './routes'

export { importExportMenuItems } from './menu'
export type { ImportExportMenuItem } from './menu'

export { hasImportExportPermission } from './permission'

export {
  columnMappingSchema,
  templateFormSchema,
  templateMetadataSchema,
  templateConfigurationSchema,
  templateFormDefaultValues,
  templateConfigurationDefaultValues,
} from './validation'
export type {
  TemplateFormValues,
  TemplateMetadataValues,
  TemplateConfigurationValues,
} from './validation'

export {
  configureImportExportHttp,
  listTemplates,
  getTemplate,
  createTemplate,
  updateTemplate,
  updateTemplateMetadata,
  updateTemplateConfiguration,
  deleteTemplate,
  downloadTemplateFile,
  listImportExportEntities,
  getImportExportEntityFields,
  validateExcelImport,
  importExcel,
  getImportExportHttp,
} from './services'
export type { ImportExportHttpConfig } from './services'

export { MappingTable } from './components/MappingTable'
export type { MappingTableProps } from './components/MappingTable'
export { ImportResultPanel } from './components/ImportResultPanel'
export type { ImportResultPanelProps } from './components/ImportResultPanel'
export { ImportExportPageShell } from './components/PageShell'
export type { ImportExportPageShellProps } from './components/PageShell'

export { TemplateListPage } from './pages/TemplateListPage'
export { TemplateFormPage } from './pages/TemplateFormPage'
export { TemplateMetadataPage } from './pages/TemplateMetadataPage'
export type { TemplateMetadataPageProps } from './pages/TemplateMetadataPage'
export { TemplateConfigurationPage } from './pages/TemplateConfigurationPage'
export type { TemplateConfigurationPageProps } from './pages/TemplateConfigurationPage'
export {
  ImportWorkflowPage,
  ExcelImportPage,
} from './pages'
export type {
  ImportWorkflowPageProps,
  ImportWorkflowCallbacks,
  ImportWorkflowOutcome,
  ExcelImportPageProps,
} from './pages'

// Re-export import UI primitives from @platform/core (KitFileUpload, ImportPage, …)
export {
  KitFileUpload,
  FileUploadField,
  ImportPage,
  ImportStudentPage,
  ImportPageShell,
  ImportPreviewTable,
  ImportValidationPanel,
  getImportMessages,
  getImportPageMessages,
  importMessages,
  importBtnSuccessClass,
} from '@platform/core'
export type {
  KitFileUploadProps,
  ImportPageProps,
  ImportPageContentContext,
  ImportStudentPageProps,
  ImportStudentPageContentContext,
  ImportPageShellProps,
  ImportPreviewTableProps,
  ImportValidationPanelProps,
  ImportLocale,
  ImportMessages,
  ImportPageMessages,
  ImportValidationResult,
  ImportCommitResult,
} from '@platform/core'
