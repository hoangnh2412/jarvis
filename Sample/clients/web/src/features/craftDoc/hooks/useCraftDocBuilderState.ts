import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { getDefaultFieldValuesMap } from '../constants'
import {
  addFieldToDocument,
  bufferToBlob,
  downloadBlob,
  extractFields,
  generateDocument,
  readDocxInput,
  removeFieldFromDocument,
  renameFieldInDocument,
  resolveCraftDocTemplateFromFile,
} from '../services'
import {
  FieldType,
  type CreateDocumentFieldPayload,
  type DocumentField,
  type DocumentTemplate,
  type PreviewMode,
  type UpdateDocumentFieldPayload,
} from '../types'
import {
  createDocxBlob,
  createFieldFromPayload,
  createSampleDocxBlob,
  enrichDocumentFields,
  ensureFieldValues,
  filterFields,
  mergeParsedFieldsWithValues,
  SAMPLE_DOCX_DEFINITIONS,
  toPlaceholder,
  toPlaceholderMap,
} from '../utils'

const PREVIEW_DATA_DEBOUNCE_MS = 400

function createInitialTemplates(): DocumentTemplate[] {
  return SAMPLE_DOCX_DEFINITIONS.map((definition) => {
    const blob = createSampleDocxBlob(definition)
    return {
      id: definition.id,
      name: definition.name,
      fileName: definition.fileName,
      file: blob,
      fields: [],
      updatedAt: new Date().toISOString(),
    }
  })
}

function applyTemplateDefaultValues(template: DocumentTemplate): DocumentTemplate {
  return {
    ...template,
    fields: ensureFieldValues(template.fields, getDefaultFieldValuesMap(template.id)),
  }
}

export type UseCraftDocBuilderStateOptions = {
  initialTemplateId?: string | null
  templates?: DocumentTemplate[]
  activeTemplateId?: string | null
  onTemplatesChange?: (templates: DocumentTemplate[]) => void
  onActiveTemplateIdChange?: (id: string | null) => void
}

export function useCraftDocBuilderState(options: UseCraftDocBuilderStateOptions = {}) {
  const templatesControlled = options.templates != null
  const activeTemplateControlled = options.activeTemplateId !== undefined

  const [templates, setTemplatesState] = useState<DocumentTemplate[]>(() =>
    options.templates ?? createInitialTemplates(),
  )
  const [activeTemplateId, setActiveTemplateIdState] = useState<string | null>(
    options.activeTemplateId ?? options.initialTemplateId ?? 'labor-contract',
  )
  const [selectedFieldId, setSelectedFieldId] = useState<string | null>(null)
  const [previewMode, setPreviewMode] = useState<PreviewMode>('filled')
  const [designDrawerOpen, setDesignDrawerOpen] = useState(false)
  const [templatePickerOpen, setTemplatePickerOpen] = useState(false)
  const [previewBlob, setPreviewBlob] = useState<Blob | null>(null)
  const [zoom, setZoom] = useState(100)
  const [fitMode, setFitMode] = useState<'none' | 'width' | 'page'>('none')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [fullscreen, setFullscreen] = useState(false)
  const [status, setStatus] = useState<'idle' | 'loading' | 'parsing' | 'ready' | 'error'>('idle')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [dirty, setDirty] = useState(false)
  const [fieldSearch, setFieldSearch] = useState('')
  const [fieldTypeFilter, setFieldTypeFilter] = useState<DocumentField['type'] | 'all'>('all')
  const [leftCollapsed, setLeftCollapsed] = useState(false)
  const [rightCollapsed, setRightCollapsed] = useState(false)
  const [parsedFieldsByTemplate, setParsedFieldsByTemplate] = useState<
    Record<string, DocumentField[]>
  >({})

  const templatesRef = useRef(templates)
  templatesRef.current = templates
  const parsedFieldsRef = useRef<Record<string, DocumentField[]>>({})
  const templateBufferRef = useRef<ArrayBuffer | null>(null)

  useEffect(() => {
    if (!templatesControlled) return
    setTemplatesState(options.templates ?? [])
  }, [options.templates, templatesControlled])

  useEffect(() => {
    if (!activeTemplateControlled) return
    setActiveTemplateIdState(options.activeTemplateId ?? null)
  }, [activeTemplateControlled, options.activeTemplateId])

  const setTemplates = useCallback(
    (updater: DocumentTemplate[] | ((current: DocumentTemplate[]) => DocumentTemplate[])) => {
      setTemplatesState((current) => {
        const next = typeof updater === 'function' ? updater(current) : updater
        options.onTemplatesChange?.(next)
        return next
      })
    },
    [options.onTemplatesChange],
  )

  const setActiveTemplateId = useCallback(
    (value: string | null) => {
      setActiveTemplateIdState(value)
      options.onActiveTemplateIdChange?.(value)
    },
    [options.onActiveTemplateIdChange],
  )

  const activeTemplate = useMemo(
    () => templates.find((template) => template.id === activeTemplateId) ?? null,
    [templates, activeTemplateId],
  )

  const templateRevision = activeTemplate
    ? `${activeTemplate.id}:${activeTemplate.updatedAt ?? ''}`
    : null

  const fields = activeTemplate?.fields ?? []

  const placeholderMap = useMemo(() => toPlaceholderMap(fields), [fields])

  const placeholderMapRef = useRef(placeholderMap)
  placeholderMapRef.current = placeholderMap

  const selectedField = useMemo(
    () => fields.find((field) => field.id === selectedFieldId) ?? null,
    [fields, selectedFieldId],
  )

  const filteredFields = useMemo(
    () => filterFields(fields, fieldSearch, fieldTypeFilter),
    [fields, fieldSearch, fieldTypeFilter],
  )

  const syncTemplateFields = useCallback(
    (templateId: string, parsed: DocumentField[]) => {
      let merged = parsed
      setTemplates((current) => {
        const existing = current.find((item) => item.id === templateId)
        merged = existing
          ? mergeParsedFieldsWithValues(parsed, existing.fields)
          : ensureFieldValues(parsed, getDefaultFieldValuesMap(templateId))
        parsedFieldsRef.current[templateId] = merged
        return current.map((item) =>
          item.id === templateId ? { ...item, fields: merged } : item,
        )
      })
      setParsedFieldsByTemplate((current) => ({ ...current, [templateId]: merged }))
    },
    [setTemplates],
  )

  const parseTemplateFields = useCallback(async (template: DocumentTemplate) => {
    const cached = parsedFieldsRef.current[template.id]
    if (cached?.length) return cached

    setStatus('parsing')
    setErrorMessage(null)
    try {
      const buffer = await readDocxInput(template.file)
      const parsed = extractFields(buffer, template.fields, template.id)
      syncTemplateFields(template.id, parsed)
      setStatus('ready')
      return parsedFieldsRef.current[template.id] ?? []
    } catch (error) {
      setStatus('error')
      setErrorMessage(
        error instanceof Error ? error.message : 'Unable to analyze template fields.',
      )
      return []
    }
  }, [syncTemplateFields])

  const buildPreviewBlob = useCallback(
    (buffer: ArrayBuffer, data: Record<string, string>, mode: PreviewMode) => {
      if (mode === 'template' || Object.keys(data).length === 0) {
        return bufferToBlob(buffer)
      }
      return generateDocument(buffer, data)
    },
    [],
  )

  useEffect(() => {
    if (!activeTemplateId || !templateRevision) {
      templateBufferRef.current = null
      setPreviewBlob(null)
      setStatus('idle')
      return
    }

    const template = templatesRef.current.find((item) => item.id === activeTemplateId)
    if (!template) {
      templateBufferRef.current = null
      setPreviewBlob(null)
      setStatus('idle')
      return
    }

    let cancelled = false

    const run = async () => {
      setStatus('loading')
      setErrorMessage(null)
      try {
        await parseTemplateFields(template)
        const buffer = await readDocxInput(template.file)
        if (cancelled) return

        templateBufferRef.current = buffer
        const blob = buildPreviewBlob(buffer, placeholderMapRef.current, previewMode)
        setPreviewBlob(blob)
        setStatus('ready')
        setCurrentPage(1)
      } catch (error) {
        if (!cancelled) {
          setStatus('error')
          setErrorMessage(
            error instanceof Error
              ? error.message
              : 'Unable to preview this document.',
          )
        }
      }
    }

    void run()
    return () => {
      cancelled = true
    }
  }, [activeTemplateId, templateRevision, previewMode, parseTemplateFields, buildPreviewBlob])

  useEffect(() => {
    if (!activeTemplateId || previewMode !== 'filled') return

    const timer = window.setTimeout(() => {
      const buffer = templateBufferRef.current
      if (!buffer) return
      const blob = generateDocument(buffer, placeholderMap)
      setPreviewBlob(blob)
    }, PREVIEW_DATA_DEBOUNCE_MS)

    return () => window.clearTimeout(timer)
  }, [placeholderMap, activeTemplateId, previewMode])

  const reloadPreview = useCallback(async () => {
    const template = templatesRef.current.find((item) => item.id === activeTemplateId)
    if (!template) return

    setStatus('loading')
    setErrorMessage(null)
    try {
      await parseTemplateFields(template)
      const buffer = await readDocxInput(template.file)
      templateBufferRef.current = buffer
      const blob = buildPreviewBlob(buffer, placeholderMapRef.current, previewMode)
      setPreviewBlob(blob)
      setStatus('ready')
      setCurrentPage(1)
    } catch (error) {
      setStatus('error')
      setErrorMessage(
        error instanceof Error ? error.message : 'Unable to preview this document.',
      )
    }
  }, [activeTemplateId, buildPreviewBlob, parseTemplateFields, previewMode])

  const applyUploadedTemplate = useCallback(
    async (template: DocumentTemplate) => {
      const rawBuffer = await readDocxInput(template.file)
      const withValues = applyTemplateDefaultValues({
        ...template,
        file: bufferToBlob(rawBuffer),
      })
      parsedFieldsRef.current[withValues.id] = withValues.fields
      setParsedFieldsByTemplate((current) => ({
        ...current,
        [withValues.id]: withValues.fields,
      }))
      setTemplates((current) => {
        const withoutDuplicate = current.filter((item) => item.id !== withValues.id)
        return [withValues, ...withoutDuplicate]
      })
      setActiveTemplateId(withValues.id)
      setSelectedFieldId(null)
      setPreviewMode('template')
      setDirty(false)

      templateBufferRef.current = rawBuffer
      const map = toPlaceholderMap(withValues.fields)
      placeholderMapRef.current = map
      setPreviewBlob(bufferToBlob(rawBuffer))
      setStatus('ready')
      setCurrentPage(1)
    },
    [buildPreviewBlob, setActiveTemplateId, setTemplates],
  )

  const selectTemplate = useCallback(
    (templateId: string) => {
      setActiveTemplateId(templateId)
      setSelectedFieldId(null)
      setPreviewMode('filled')
      setFitMode('none')
      setZoom(100)
      setTemplates((current) =>
        current.map((template) =>
          template.id === templateId
            ? applyTemplateDefaultValues(template)
            : template,
        ),
      )
    },
    [setActiveTemplateId, setTemplates],
  )

  const setFieldValue = useCallback(
    (fieldName: string, value: string) => {
      if (!activeTemplateId) return
      setTemplates((current) =>
        current.map((template) => {
          if (template.id !== activeTemplateId) return template
          return {
            ...template,
            fields: template.fields.map((field) =>
              field.name === fieldName ? { ...field, value } : field,
            ),
          }
        }),
      )
      setDirty(true)
    },
    [activeTemplateId, setTemplates],
  )

  const setFieldValues = useCallback(
    (updates: Record<string, string>) => {
      if (!activeTemplateId) return
      const names = new Set(Object.keys(updates))
      if (!names.size) return
      setTemplates((current) =>
        current.map((template) => {
          if (template.id !== activeTemplateId) return template
          return {
            ...template,
            fields: template.fields.map((field) =>
              names.has(field.name)
                ? { ...field, value: updates[field.name] ?? '' }
                : field,
            ),
          }
        }),
      )
      setDirty(true)
    },
    [activeTemplateId, setTemplates],
  )

  const createBlankTemplate = useCallback(async (name = 'Mẫu mới') => {
    const templateId = `blank-${Date.now()}`
    const blob = createDocxBlob(name.toUpperCase(), [
      'Nội dung mẫu — thêm placeholder bằng chức năng Chỉnh sửa mẫu.',
      '{{sampleField}}',
    ])
    const fields = enrichDocumentFields(
      [
        createFieldFromPayload({
          name: 'sampleField',
          label: 'Trường mẫu',
          type: FieldType.Text,
        }),
      ],
      templateId,
    ).map((field) => ({ ...field, value: '' }))
    const template: DocumentTemplate = {
      id: templateId,
      name,
      fileName: `${name.replace(/\s+/g, '-')}.docx`,
      file: blob,
      fields,
      updatedAt: new Date().toISOString(),
    }
    await applyUploadedTemplate(template)
    setDesignDrawerOpen(true)
    return template
  }, [applyUploadedTemplate])

  const selectField = useCallback((fieldId: string | null) => {
    setSelectedFieldId(fieldId)
    if (fieldId) setRightCollapsed(false)
  }, [])

  const updateField = useCallback(
    async (fieldId: string, patch: UpdateDocumentFieldPayload) => {
      if (!activeTemplate) return

      const currentField = activeTemplate.fields.find((field) => field.id === fieldId)
      if (!currentField) return

      const nextName = patch.name?.trim() || currentField.name
      const nextField: DocumentField = {
        ...currentField,
        ...patch,
        name: nextName,
        label: patch.label?.trim() || currentField.label,
        placeholder: patch.placeholder ?? toPlaceholder(nextName),
      }

      let buffer = await readDocxInput(activeTemplate.file)
      if (nextName !== currentField.name) {
        buffer = renameFieldInDocument(buffer, currentField.name, nextName)
      }

      const nextFields = activeTemplate.fields.map((field) =>
        field.id === fieldId ? nextField : field,
      )
      const nextBlob = bufferToBlob(buffer)

      setTemplates((current) =>
        current.map((template) =>
          template.id === activeTemplate.id
            ? {
                ...template,
                file: nextBlob,
                fields: nextFields,
                updatedAt: new Date().toISOString(),
              }
            : template,
        ),
      )
      parsedFieldsRef.current[activeTemplate.id] = nextFields
      setParsedFieldsByTemplate((current) => ({
        ...current,
        [activeTemplate.id]: nextFields,
      }))
      templateBufferRef.current = buffer
      setPreviewBlob(buildPreviewBlob(buffer, toPlaceholderMap(nextFields), previewMode))
      setDirty(true)
    },
    [activeTemplate, buildPreviewBlob, previewMode, setTemplates],
  )

  const addField = useCallback(
    async (payload: CreateDocumentFieldPayload) => {
      if (!activeTemplate) return null

      const field = { ...createFieldFromPayload(payload), value: payload.defaultValue ?? '' }
      let buffer = await readDocxInput(activeTemplate.file)
      buffer = addFieldToDocument(buffer, field.placeholder)
      const nextBlob = bufferToBlob(buffer)
      const nextFields = [...activeTemplate.fields, field]

      setTemplates((current) =>
        current.map((template) =>
          template.id === activeTemplate.id
            ? {
                ...template,
                file: nextBlob,
                fields: nextFields,
                updatedAt: new Date().toISOString(),
              }
            : template,
        ),
      )
      parsedFieldsRef.current[activeTemplate.id] = nextFields
      setParsedFieldsByTemplate((current) => ({
        ...current,
        [activeTemplate.id]: nextFields,
      }))
      templateBufferRef.current = buffer
      setPreviewBlob(buildPreviewBlob(buffer, toPlaceholderMap(nextFields), previewMode))
      setSelectedFieldId(field.id)
      setDirty(true)
      return field
    },
    [activeTemplate, buildPreviewBlob, previewMode, setTemplates],
  )

  const deleteField = useCallback(
    async (fieldId: string) => {
      if (!activeTemplate) return

      const field = activeTemplate.fields.find((item) => item.id === fieldId)
      if (!field) return

      let buffer = await readDocxInput(activeTemplate.file)
      buffer = removeFieldFromDocument(buffer, field.name)
      const nextBlob = bufferToBlob(buffer)
      const nextFields = activeTemplate.fields.filter((item) => item.id !== fieldId)

      setTemplates((current) =>
        current.map((template) =>
          template.id === activeTemplate.id
            ? {
                ...template,
                file: nextBlob,
                fields: nextFields,
                updatedAt: new Date().toISOString(),
              }
            : template,
        ),
      )
      parsedFieldsRef.current[activeTemplate.id] = nextFields
      setParsedFieldsByTemplate((current) => ({
        ...current,
        [activeTemplate.id]: nextFields,
      }))
      templateBufferRef.current = buffer
      setPreviewBlob(buildPreviewBlob(buffer, toPlaceholderMap(nextFields), previewMode))
      if (selectedFieldId === fieldId) setSelectedFieldId(null)
      setDirty(true)
    },
    [activeTemplate, buildPreviewBlob, previewMode, selectedFieldId, setTemplates],
  )

  const uploadTemplate = useCallback(async (file: File) => {
    setStatus('parsing')
    setErrorMessage(null)
    try {
      const template = await resolveCraftDocTemplateFromFile(file)
      await applyUploadedTemplate(template)
      return template
    } catch (error) {
      setStatus('error')
      setErrorMessage(
        error instanceof Error ? error.message : 'Unable to upload this document.',
      )
      return null
    }
  }, [applyUploadedTemplate])

  const exportDocument = useCallback(async () => {
    if (!activeTemplate) return
    const buffer = templateBufferRef.current ?? (await readDocxInput(activeTemplate.file))
    const blob = generateDocument(buffer, toPlaceholderMap(activeTemplate.fields))
    downloadBlob(blob, activeTemplate.fileName)
  }, [activeTemplate])

  const saveTemplate = useCallback(() => {
    setDirty(false)
  }, [])

  const zoomIn = useCallback(() => {
    setFitMode('none')
    setZoom((value) => Math.min(value + 10, 200))
  }, [])

  const zoomOut = useCallback(() => {
    setFitMode('none')
    setZoom((value) => Math.max(value - 10, 50))
  }, [])

  const resetZoom = useCallback(() => {
    setFitMode('none')
    setZoom(100)
  }, [])

  return {
    templates,
    activeTemplateId,
    activeTemplate,
    selectedFieldId,
    selectedField,
    filteredFields,
    previewMode,
    setPreviewMode,
    previewBlob,
    /** Derived flat map — adapter legacy; nguồn chính: activeTemplate.fields[].value */
    sampleData: placeholderMap,
    placeholderMap,
    zoom,
    fitMode,
    setFitMode,
    currentPage,
    setCurrentPage,
    totalPages,
    setTotalPages,
    fullscreen,
    setFullscreen,
    status,
    errorMessage,
    dirty,
    fieldSearch,
    setFieldSearch,
    fieldTypeFilter,
    setFieldTypeFilter,
    leftCollapsed,
    setLeftCollapsed,
    rightCollapsed,
    setRightCollapsed,
    designDrawerOpen,
    setDesignDrawerOpen,
    templatePickerOpen,
    setTemplatePickerOpen,
    selectTemplate,
    selectField,
    setFieldValue,
    setFieldValues,
    createBlankTemplate,
    updateField,
    addField,
    deleteField,
    uploadTemplate,
    applyUploadedTemplate,
    exportDocument,
    saveTemplate,
    reloadPreview,
    zoomIn,
    zoomOut,
    resetZoom,
  }
}

export type CraftDocBuilderState = ReturnType<typeof useCraftDocBuilderState>
