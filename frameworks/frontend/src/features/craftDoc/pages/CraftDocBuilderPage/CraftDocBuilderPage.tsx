import {
  useCallback,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { notify } from '../../../../common/Toaster'
import { getErrorMessage } from '../../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../../lib/handleAction'
import { DocxTemplateBuilder } from '../../components/DocxTemplateBuilder'
import { CraftDocPageShell } from '../../components/CraftDocPageShell'
import { buildCraftDocController, useCraftDocBuilderState } from '../../hooks'
import { getCraftDocMessages, type CraftDocLocale } from '../../localization'
import {
  callExportCraftDocDocument,
  callSaveCraftDocDocument,
  callUploadCraftDocTemplate,
  type CraftDocExportPayload,
  type CraftDocExportResult,
  type CraftDocSavePayload,
  type CraftDocSaveResult,
} from '../../services'
import type {
  DocumentField,
  DocumentTemplate,
  DocxPreviewStatus,
} from '../../types'
import {
  resolveCraftDocContent,
  toPlaceholderMap,
  type CraftDocSlotContent,
} from '../../utils'

export type CraftDocBuilderPageContentContext = {
  templates: DocumentTemplate[]
  activeTemplate: DocumentTemplate | null
  /** Fields mẫu đang active — mỗi item có `value`. */
  fields: DocumentField[]
  /** @deprecated Dùng fields[].value */
  sampleData: Record<string, string>
  previewBlob: Blob | null
  status: DocxPreviewStatus
  loading: boolean
  saving: boolean
  dirty: boolean
  reload: () => Promise<void>
  save: () => Promise<void>
  exportDocument: () => Promise<void>
  DefaultLayout: ReactNode
}

export type CraftDocBuilderPageProps = {
  locale?: CraftDocLocale
  className?: string
  title?: string
  description?: string
  withShell?: boolean
  initialTemplateId?: string | null
  /** Controlled — mỗi template.fields[] có thể kèm `value`. */
  templates?: DocumentTemplate[]
  activeTemplateId?: string | null
  loading?: boolean
  onTemplatesChange?: (templates: DocumentTemplate[]) => void
  onActiveTemplateChange?: (id: string | null) => void
  callback?: {
    upload?: ActionProps<{ file: File }, File, DocumentTemplate>
    save?: ActionProps<
      CraftDocSavePayload,
      CraftDocSavePayload,
      CraftDocSaveResult
    >
    export?: ActionProps<
      CraftDocExportPayload,
      CraftDocExportPayload,
      CraftDocExportResult
    >
    createTemplate?: ActionProps<
      { name?: string },
      { name?: string },
      DocumentTemplate
    >
  }
  content?: CraftDocSlotContent<CraftDocBuilderPageContentContext>
}

export function CraftDocBuilderPage({
  locale = 'vi',
  className,
  title,
  description,
  withShell = false,
  initialTemplateId,
  templates: templatesProp,
  activeTemplateId: activeTemplateIdProp,
  loading: loadingProp,
  onTemplatesChange,
  onActiveTemplateChange,
  callback,
  content,
}: CraftDocBuilderPageProps) {
  const messages = getCraftDocMessages(locale)
  const [saving, setSaving] = useState(false)

  const state = useCraftDocBuilderState({
    initialTemplateId,
    templates: templatesProp,
    activeTemplateId: activeTemplateIdProp,
    onTemplatesChange,
    onActiveTemplateIdChange: onActiveTemplateChange,
  })

  const isLoading =
    loadingProp ?? (state.status === 'loading' || state.status === 'parsing')

  const reload = useCallback(async () => {
    await state.reloadPreview()
  }, [state])

  const buildSavePayload = useCallback((): CraftDocSavePayload | null => {
    if (!state.activeTemplate) return null
    const fields = state.activeTemplate.fields
    return {
      template: state.activeTemplate,
      fields,
      sampleData: toPlaceholderMap(fields),
    }
  }, [state.activeTemplate])

  const onSave = useCallback(async () => {
    const payload = buildSavePayload()
    if (!payload) return
    setSaving(true)
    try {
      await handleAction({
        ctx: payload,
        callback: callback?.save,
        defaultSubmit: callSaveCraftDocDocument,
        getPayload: (ctx) => ctx,
        onSuccess: async () => {
          state.saveTemplate()
        },
      })
      notify.success(messages.page.saved)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.page.error))
    } finally {
      setSaving(false)
    }
  }, [
    buildSavePayload,
    callback?.save,
    messages.page.error,
    messages.page.saved,
    state,
  ])

  const onExport = useCallback(async () => {
    const payload = buildSavePayload()
    if (!payload) return
    try {
      await handleAction({
        ctx: payload,
        callback: callback?.export,
        defaultSubmit: callExportCraftDocDocument,
        getPayload: (ctx) => ctx,
      })
      notify.success(messages.page.exported)
    } catch (error) {
      notify.error(getErrorMessage(error, messages.page.error))
    }
  }, [
    buildSavePayload,
    callback?.export,
    messages.page.error,
    messages.page.exported,
  ])

  const onUploadFile = useCallback(
    async (file: File) => {
      try {
        const outcome = await handleAction({
          ctx: { file },
          callback: callback?.upload,
          defaultSubmit: callUploadCraftDocTemplate,
          getPayload: (ctx) => ctx.file,
          onSuccess: async (_ctx, template) => {
            await state.applyUploadedTemplate(template)
            state.setTemplatePickerOpen(false)
          },
        })
        if (outcome.status === 'cancelled') return
        if (outcome.status === 'success' && outcome.result) {
          notify.success(messages.page.uploaded)
        }
      } catch (error) {
        notify.error(getErrorMessage(error, messages.page.error))
      }
    },
    [callback?.upload, messages.page.error, messages.page.uploaded, state],
  )

  const onCreateTemplate = useCallback(
    async (name?: string) => {
      try {
        await handleAction({
          ctx: { name },
          callback: callback?.createTemplate,
          defaultSubmit: async (payload) => state.createBlankTemplate(payload.name),
          getPayload: (ctx) => ctx,
        })
      } catch (error) {
        notify.error(getErrorMessage(error, messages.page.error))
      }
    },
    [callback?.createTemplate, messages.page.error, state],
  )

  const controller = useMemo(
    () =>
      buildCraftDocController({
        state,
        saving,
        onSave,
        onExport,
        onUploadFile,
        onCreateTemplate,
      }),
    [onCreateTemplate, onExport, onSave, onUploadFile, saving, state],
  )

  const defaultLayout = (
    <DocxTemplateBuilder
      locale={locale}
      controller={controller}
      className="min-h-0 flex-1"
    />
  )

  const activeFields = state.activeTemplate?.fields ?? []

  const pageContent = resolveCraftDocContent(
    content,
    {
      templates: state.templates,
      activeTemplate: state.activeTemplate,
      fields: activeFields,
      sampleData: state.placeholderMap,
      previewBlob: state.previewBlob,
      status: state.status,
      loading: isLoading,
      saving,
      dirty: state.dirty,
      reload,
      save: onSave,
      exportDocument: onExport,
      DefaultLayout: defaultLayout,
    },
    defaultLayout,
  )

  const body = (
    <div className="craft-doc-page flex h-full min-h-0 flex-1 flex-col overflow-hidden">
      {pageContent}
    </div>
  )

  if (!withShell) {
    return (
      <div
        className={[className, 'flex h-full min-h-0 flex-col overflow-hidden']
          .filter(Boolean)
          .join(' ')}
      >
        {body}
      </div>
    )
  }

  return (
    <CraftDocPageShell
      title={title ?? messages.page.title}
      description={description ?? messages.page.description}
      className={className}
    >
      {body}
    </CraftDocPageShell>
  )
}
