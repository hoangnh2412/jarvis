export {
  escapeXmlText,
  extractPlaceholderNamesFromXml,
  replacePlaceholdersInXml,
  removePlaceholderFromXml,
  insertPlaceholderInXml,
  normalizeSplitPlaceholdersInXml,
  removeEmptyRunsInXml,
  normalizeDocxXml,
} from './placeholderXml'
export {
  createFieldId,
  toPlaceholder,
  placeholderToName,
  createFieldFromPlaceholder,
  createFieldFromPayload,
  humanizeFieldName,
  mergeExtractedFields,
  buildSampleDataFromFields,
  buildFieldValuesFromFields,
  enrichDocumentField,
  enrichDocumentFields,
  groupDocumentFields,
  validateDocumentData,
  filterFields,
  fieldTypeIcon,
  getFieldInputPlaceholder,
  isDocxPlaceholderToken,
} from './fieldHelpers'
export type { GroupedDocumentFields } from './fieldHelpers'
export {
  toPlaceholderMap,
  ensureFieldValues,
  mergeParsedFieldsWithValues,
  applyValuesMapToFields,
} from './fieldValues'
export type { DynamicFieldGroup } from './fieldGrouping'
export {
  buildDynamicFieldGroups,
  inferFieldGroup,
  DYNAMIC_GROUP_LABELS,
} from './fieldGrouping'
export {
  createDocxBlob,
  createDocxFromParagraphs,
  createSampleDocxBlob,
  SAMPLE_DOCX_DEFINITIONS,
} from './createSampleDocx'
export type { SampleDocxDefinition, ParagraphDef, ParagraphAlign, BlockDef, TableDef, TableRowDef } from './createSampleDocx'
export { resolveCraftDocContent } from './content'
export {
  buildCraftDocSaveRequestJson,
  prettyCraftDocJson,
} from './saveRequestJson'
export type { CraftDocSaveRequestJson } from './saveRequestJson'
export type { CraftDocSlotContent } from './content'
export {
  DATE_PART_COMPOSITE_DEFS,
  detectDatePartComposites,
  buildFormFieldItems,
  groupFormFieldItems,
  datePartsToFormValue,
  formValueToDateParts,
  compositeDatePartsToUpdates,
  validateFormFieldItems,
} from './compositeDateFields'
export type {
  DatePartCompositeDef,
  DatePartCompositeView,
  FormFieldItem,
  GroupedFormFieldItems,
} from './compositeDateFields'
