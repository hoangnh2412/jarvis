export { DynamicFormBuilderPage } from './pages/DynamicFormBuilderPage'
export type {
  DynamicFormBuilderPageProps,
  DynamicFormBuilderPageContentContext,
  DynamicFormGetResult,
  DynamicFormUpdateResult,
} from './pages/DynamicFormBuilderPage'

export {
  DynamicFormPageShell,
  DynamicFormRenderer,
  FieldLibrary,
  FieldSettingsPanel,
  SubmitResultModal,
  FormPreviewDialog,
} from './components'
export type {
  DynamicFormPageShellProps,
  DynamicFormRendererProps,
  FieldLibraryProps,
  FieldSettingsPanelProps,
  SubmitResultModalProps,
  FormPreviewDialogProps,
} from './components'

export type {
  DynamicFormFieldType,
  DynamicFormTableColumnType,
  DynamicFormSelectOption,
  DynamicFormSelectConfig,
  DynamicFormLayoutWidth,
  DynamicFormField,
  DynamicFormDefinition,
  DynamicFormListItem,
  CreateDynamicFormPayload,
  UpdateDynamicFormPayload,
  DynamicFormSubmission,
  CreateDynamicFormSubmissionPayload,
  RJSFSchema,
  UiSchema,
} from './types'

export {
  callGetDynamicFormList,
  callGetDynamicForm,
  callCreateDynamicForm,
  callUpdateDynamicForm,
  callDeleteDynamicForm,
  callCreateDynamicFormSubmission,
  callGetDynamicFormSubmissions,
} from './services'

export {
  getDynamicFormListMock,
  getDynamicFormDefinitionMock,
} from './mocks'

export {
  DYNAMIC_FORM_DEMO_ID,
  DYNAMIC_FORM_ROUTES,
  getDynamicFormPagePath,
  getDynamicFormBuilderPath,
  getDynamicFormDemoPath,
  getDynamicFormRouteList,
  dynamicFormPaths,
  configureDynamicFormNavigate,
  navigateDynamicForm,
} from './routes'
export type { DynamicFormRouteKey, DynamicFormNavigateFn } from './routes'

export { dynamicFormMenuItems } from './menu'
export type { DynamicFormMenuItem } from './menu'

export {
  DYNAMIC_FORM_PERMISSIONS,
  hasDynamicFormPermission,
} from './permission'
export type { DynamicFormPermissionKey } from './permission'

export {
  dynamicFormMessages,
  dynamicFormMessagesVi,
  dynamicFormMessagesEn,
  getDynamicFormMessages,
} from './localization'
export type { DynamicFormLocale, DynamicFormMessages } from './localization'

export {
  createFormId,
  createSubmissionId,
  createFieldId,
  createOptionId,
  createField,
  createTableColumn,
  TABLE_COLUMN_TYPES,
  isTableColumnType,
  FIELD_TYPE_META,
  FIELD_LIBRARY_TYPES,
  FIELD_LIBRARY_GROUPS,
  countSchemaProperties,
  createEmptySchema,
  createDemoDefinitionSeed,
  fieldsToRjsf,
  rjsfToFields,
  syncDefinitionFromFields,
  validateFormDefinition,
  validateFieldDraft,
  firstValidationMessage,
  canRemoveField,
  resolveDynamicFormContent,
  prettyJson,
} from './utils'
export type {
  DynamicFormSlotContent,
  DynamicFormValidationIssue,
  FieldLibraryGroup,
  FieldLibraryGroupId,
} from './utils'

export {
  DEMO_RJSF_SCHEMA,
  DEMO_RJSF_UI_SCHEMA,
  DEMO_RJSF_FORM_DATA,
  RJSF_FEATURE_PRESETS,
} from './constants/demoSchemas'
export type { RjsfFeaturePreset } from './constants/demoSchemas'

export { dynamicFormWidgets, dynamicFormTemplates } from './lib'

export { useDynamicForm } from './hooks'
