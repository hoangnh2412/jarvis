import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { notify } from '../../../../common/Toaster'
import {
  getErrorMessage,
  handleAction,
  type ActionProps,
} from '../../../../lib'
import { CraftPdfEditorShell } from '../../components/CraftPdfEditorShell'
import { CraftPdfPreviewDialog } from '../../components/CraftPdfPreviewDialog'
import {
  getFakeCraftPdfDataFields,
  PDF_PAGE_A4,
  toBindingExpression,
} from '../../constants'
import { useCraftPdfEditorState } from '../../hooks'
import {
  getCraftPdfMessages,
  type CraftPdfLocale,
} from '../../localization'
import {
  getCraftPdfListPath,
  navigateCraftPdf,
} from '../../routes'
import {
  callGeneratePdf,
  callGetPdfTemplate,
  callUpdatePdfTemplate,
} from '../../services'
import type {
  CraftPdfDataField,
  GeneratePdfRequest,
  GeneratePdfResponse,
  PdfTemplate,
} from '../../types'
import { PdfElementType } from '../../types'
import {
  buildCraftPdfSampleMap,
  resolveCraftPdfContent,
  type CraftPdfSlotContent,
} from '../../utils'

export type CraftPdfEditorPageContentContext = {
  template: PdfTemplate | null
  loading: boolean
  saving: boolean
  previewOpen: boolean
  setPreviewOpen: (v: boolean) => void
  fileUrl: string | null
  save: () => Promise<void>
  preview: () => void
  generate: () => Promise<void>
  DefaultEditor: ReactNode
}

export type CraftPdfEditorPageProps = {
  /** Template id — fetch nếu không truyền `template` */
  templateId?: string
  template?: PdfTemplate | null
  locale?: CraftPdfLocale
  className?: string
  /** PDF URL từ backend cho react-pdf preview */
  previewFileUrl?: string | null
  /** Data fields schema — fake mặc định, sau get từ API */
  dataFields?: CraftPdfDataField[]
  callback?: {
    save?: ActionProps<{ template: PdfTemplate }, PdfTemplate, void>
    generate?: ActionProps<
      { template: PdfTemplate; request: GeneratePdfRequest },
      GeneratePdfRequest,
      GeneratePdfResponse | void
    >
    back?: ActionProps<{ template: PdfTemplate }, void, void>
  }
  content?: CraftPdfSlotContent<CraftPdfEditorPageContentContext>
}

export function CraftPdfEditorPage({
  templateId,
  template: controlledTemplate,
  locale = 'vi',
  className,
  previewFileUrl,
  dataFields,
  callback,
  content,
}: CraftPdfEditorPageProps) {
  const msg = getCraftPdfMessages(locale)
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [previewOpen, setPreviewOpen] = useState(false)
  const [fileUrl, setFileUrl] = useState<string | null>(previewFileUrl ?? null)
  const resolvedDataFields = dataFields ?? getFakeCraftPdfDataFields()
  const sampleData = useMemo(
    () => buildCraftPdfSampleMap(resolvedDataFields),
    [resolvedDataFields],
  )

  const editor = useCraftPdfEditorState({
    initialTemplate: controlledTemplate ?? null,
  })

  const load = useCallback(async () => {
    if (controlledTemplate) {
      editor.loadTemplate(controlledTemplate)
      return
    }
    if (!templateId) return
    setLoading(true)
    try {
      const tpl = await callGetPdfTemplate(templateId)
      editor.loadTemplate(tpl)
    } catch (err) {
      notify.error(getErrorMessage(err, 'Không tải được template'))
    } finally {
      setLoading(false)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps -- load once per id
  }, [controlledTemplate, templateId])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (previewFileUrl !== undefined) setFileUrl(previewFileUrl)
  }, [previewFileUrl])

  const save = async () => {
    if (!editor.template) return
    setSaving(true)
    try {
      const outcome = await handleAction<
        { template: PdfTemplate },
        PdfTemplate,
        void
      >({
        ctx: { template: editor.template },
        callback: callback?.save,
        defaultSubmit: async (template) => {
          await callUpdatePdfTemplate(template.id, {
            name: template.name,
            description: template.description,
            elements: template.elements,
            pageSize: template.pageSize,
            status: template.status,
          })
          notify.success('Đã lưu template')
        },
        getPayload: ({ template }) => template,
        onSuccess: () => {
          editor.setDirty(false)
        },
      })
      if (outcome.status === 'cancelled') return
    } catch (err) {
      const message = getErrorMessage(err, 'Không lưu được template')
      if (message.includes('backend') || message.includes('BASE_URL')) {
        notify.info(msg.saveSoon)
        editor.setDirty(false)
      } else {
        notify.error(message)
      }
    } finally {
      setSaving(false)
    }
  }

  const preview = () => {
    setPreviewOpen(true)
  }

  const generate = async () => {
    if (!editor.template) return
    try {
      const request: GeneratePdfRequest = {
        templateId: editor.template.id,
        data: {},
      }
      const outcome = await handleAction<
        { template: PdfTemplate; request: GeneratePdfRequest },
        GeneratePdfRequest,
        GeneratePdfResponse | void
      >({
        ctx: { template: editor.template, request },
        callback: callback?.generate,
        defaultSubmit: callGeneratePdf,
        getPayload: ({ request: submitRequest }) => submitRequest,
        onSuccess: (_ctx, result) => {
          if (!result) {
            notify.info(msg.generateSoon)
            return
          }
          if (result.fileUrl || result.blobUrl) {
            setFileUrl(result.fileUrl ?? result.blobUrl ?? null)
            setPreviewOpen(true)
            return
          }
          notify.info(msg.generateSoon)
        },
      })
      if (outcome.status === 'cancelled') return
    } catch {
      notify.info(msg.generateSoon)
      setPreviewOpen(true)
    }
  }

  const onBack = () => {
    void (async () => {
      if (!editor.template) return
      try {
        const outcome = await handleAction<
          { template: PdfTemplate },
          void,
          void
        >({
          ctx: { template: editor.template },
          callback: callback?.back,
          defaultSubmit: async () => undefined,
          getPayload: () => undefined,
          onSuccess: () => {
            navigateCraftPdf(getCraftPdfListPath())
          },
        })
        if (outcome.status === 'cancelled') return
      } catch (err) {
        notify.error(getErrorMessage(err, 'Không thể quay lại danh sách'))
      }
    })()
  }

  const insertDataField = (field: CraftPdfDataField) => {
    const expr = toBindingExpression(field)
    const selected = editor.selectedElement
    if (selected?.type === PdfElementType.Text) {
      editor.updateElement(selected.id, {
        content: expr,
        bindingKey: field.key,
        name: field.label,
      })
      return
    }
    if (selected?.type === PdfElementType.RichText) {
      editor.updateElement(selected.id, {
        html: `<p>${expr}</p>`,
        bindingKey: field.key,
        name: field.label,
      })
      return
    }
    editor.addElement(PdfElementType.Text, undefined, {
      content: expr,
      bindingKey: field.key,
      name: field.label,
      width: Math.max(160, expr.length * 7),
    })
  }

  if (loading && !editor.template) {
    return (
      <p className={`m-0 py-16 text-center text-sm text-slate-400 ${className}`}>
        Đang tải editor…
      </p>
    )
  }

  if (!editor.template) {
    return (
      <p className={`m-0 py-16 text-center text-sm text-slate-400 ${className}`}>
        Không tìm thấy template.
      </p>
    )
  }

  const DefaultEditor = (
    <CraftPdfEditorShell
      title={editor.template.name}
      pageSize={editor.pageSize ?? PDF_PAGE_A4}
      elements={editor.elements}
      selectedId={editor.selectedId}
      selectedElement={editor.selectedElement}
      zoom={editor.zoom}
      dirty={editor.dirty}
      saving={saving}
      paletteTitle={msg.palette}
      dataFieldsTitle={msg.dataFields}
      dataFields={resolvedDataFields}
      sampleData={sampleData}
      propertiesTitle={msg.properties}
      emptyPropertiesLabel={msg.noSelection}
      bindingHint={msg.bindingHint}
      onSelect={editor.setSelectedId}
      onChangeElement={editor.updateElement}
      onAddElement={editor.addElement}
      onInsertDataField={insertDataField}
      onRemoveElement={editor.removeElement}
      onZoomIn={editor.zoomIn}
      onZoomOut={editor.zoomOut}
      onBack={onBack}
      onSave={() => void save()}
      onPreview={preview}
      onGenerate={() => void generate()}
      className={className}
    />
  )

  const ctx: CraftPdfEditorPageContentContext = {
    template: editor.template,
    loading,
    saving,
    previewOpen,
    setPreviewOpen,
    fileUrl,
    save,
    preview,
    generate,
    DefaultEditor,
  }

  return (
    <>
      {resolveCraftPdfContent(content, ctx, DefaultEditor)}
      <CraftPdfPreviewDialog
        visible={previewOpen}
        onHide={() => setPreviewOpen(false)}
        title={msg.previewTitle}
        closeLabel={msg.close}
        htmlHint={msg.previewHtmlHint}
        fileUrl={fileUrl}
        pageSize={editor.pageSize ?? PDF_PAGE_A4}
        elements={editor.elements}
        sampleData={sampleData}
      />
    </>
  )
}
