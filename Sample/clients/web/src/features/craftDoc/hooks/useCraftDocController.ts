import type { CraftDocBuilderController } from '../types'
import type { CraftDocBuilderState } from './useCraftDocBuilderState'

export type BuildCraftDocControllerOptions = {
  state: CraftDocBuilderState
  saving?: boolean
  onSave: () => void | Promise<void>
  onExport: () => void | Promise<void>
  onUploadFile: (file: File) => Promise<void>
  onCreateTemplate: (name?: string) => Promise<void>
}

export function buildCraftDocController({
  state,
  saving = false,
  onSave,
  onExport,
  onUploadFile,
  onCreateTemplate,
}: BuildCraftDocControllerOptions): CraftDocBuilderController {
  return {
    templates: state.templates,
    activeTemplateId: state.activeTemplateId,
    activeTemplate: state.activeTemplate,
    sampleData: state.sampleData,
    fields: state.activeTemplate?.fields ?? [],
    previewBlob: state.previewBlob,
    previewMode: state.previewMode,
    status: state.status,
    errorMessage: state.errorMessage,
    dirty: state.dirty,
    saving,
    designDrawerOpen: state.designDrawerOpen,
    templatePickerOpen: state.templatePickerOpen,
    preview: {
      zoom: state.zoom,
      fitMode: state.fitMode,
      currentPage: state.currentPage,
      totalPages: state.totalPages,
      fullscreen: state.fullscreen,
      onZoomIn: state.zoomIn,
      onZoomOut: state.zoomOut,
      onResetZoom: state.resetZoom,
      onFitWidth: () => state.setFitMode('width'),
      onFitPage: () => state.setFitMode('page'),
      onFullscreenToggle: () => state.setFullscreen(!state.fullscreen),
      onPreviousPage: () => state.setCurrentPage(Math.max(1, state.currentPage - 1)),
      onNextPage: () =>
        state.setCurrentPage(Math.min(state.totalPages, state.currentPage + 1)),
      onCurrentPageChange: state.setCurrentPage,
      onTotalPagesChange: state.setTotalPages,
    },
    design: {
      filteredFields: state.filteredFields,
      selectedFieldId: state.selectedFieldId,
      selectedField: state.selectedField,
      fieldSearch: state.fieldSearch,
      fieldTypeFilter: state.fieldTypeFilter,
      onSearchChange: state.setFieldSearch,
      onTypeFilterChange: state.setFieldTypeFilter,
      onSelectField: state.selectField,
      onUpdateField: state.updateField,
      onAddField: state.addField,
      onDeleteField: state.deleteField,
    },
    setFieldValue: state.setFieldValue,
    setFieldValues: state.setFieldValues,
    selectTemplate: state.selectTemplate,
    setDesignDrawerOpen: state.setDesignDrawerOpen,
    setTemplatePickerOpen: state.setTemplatePickerOpen,
    onSave,
    onExport,
    onUploadFile,
    onCreateTemplate,
  }
}
