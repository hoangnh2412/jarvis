/** Keys aligned with Sample.Settings.ImportExportSettingDefinition */
export const IMPORT_EXPORT_IMPORT_SETTING_GROUP = 'ImportGroup'
export const IMPORT_EXPORT_EXPORT_SETTING_GROUP = 'ExportGroup'

/** @deprecated Use IMPORT_EXPORT_IMPORT_SETTING_GROUP */
export const IMPORT_EXPORT_SETTING_GROUP = IMPORT_EXPORT_IMPORT_SETTING_GROUP

export const IMPORT_EXPORT_SETTING_KEYS = {
  studentImportTemplateCode: 'ImportExport.Student.ImportTemplateCode',
  studentExportTemplateCode: 'ImportExport.Student.ExportTemplateCode',
  userImportTemplateCode: 'ImportExport.User.ImportTemplateCode',
  userExportTemplateCode: 'ImportExport.User.ExportTemplateCode',
} as const
