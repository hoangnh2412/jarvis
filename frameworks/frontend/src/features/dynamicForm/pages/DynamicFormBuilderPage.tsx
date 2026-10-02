import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import {
  ArrowDown,
  ArrowUp,
  CircleHelp,
  Eraser,
  Redo2,
  Save,
  Settings2,
  Trash2,
  Undo2,
} from 'lucide-react'
import { Button } from 'primereact/button'
import { Textarea } from 'primereact/textarea'
import { notify } from '../../../common/Toaster'
import { getErrorMessage } from '../../../lib/getErrorMessage'
import { handleAction, type ActionProps } from '../../../lib/handleAction'
import { DynamicFormRenderer } from '../components/DynamicFormRenderer'
import { FieldLibrary } from '../components/FieldLibrary'
import { FieldSettingsPanel } from '../components/FieldSettingsPanel'
import { btnOutlinedClass, btnPrimaryClass } from '../components/fieldStyles'
import {
  getDynamicFormMessages,
  type DynamicFormLocale,
} from '../localization'
import {
  callCreateDynamicFormSubmission,
  callGetDynamicForm,
  callUpdateDynamicForm,
} from '../services'
import { resetFormsFromMockJson } from '../services/dynamicFormStore'
import type {
  DynamicFormDefinition,
  DynamicFormField,
  UpdateDynamicFormPayload,
} from '../types'
import {
  createDemoDefinitionSeed,
  createField,
  collectFieldKeys,
  findFieldById,
  findFieldByKey,
  firstValidationMessage,
  insertFieldInTree,
  moveFieldById,
  moveFieldInTree,
  prettyJson,
  removeFieldFromTree,
  resolveDynamicFormContent,
  rjsfToFields,
  syncDefinitionFromFields,
  updateFieldInTree,
  validateFieldDraft,
  validateFormDefinition,
  type DynamicFormSlotContent,
} from '../utils'

export type DynamicFormGetResult = { data: DynamicFormDefinition }
export type DynamicFormUpdateResult = { data: DynamicFormDefinition }

export type DynamicFormBuilderPageContentContext = {
  form: DynamicFormDefinition | null
  loading: boolean
  saving: boolean
  save: () => Promise<void>
  reload: () => Promise<void>
  DefaultLayout: ReactNode
}

export type DynamicFormBuilderPageProps = {
  formId?: string
  form?: DynamicFormDefinition | null
  loading?: boolean
  locale?: DynamicFormLocale
  className?: string
  title?: string
  description?: string
  withShell?: boolean
  callback?: {
    load?: ActionProps<{ formId: string }, string, DynamicFormGetResult>
    save?: ActionProps<
      { form: DynamicFormDefinition },
      UpdateDynamicFormPayload,
      DynamicFormUpdateResult
    >
    back?: ActionProps<{ form: DynamicFormDefinition }, void, void>
  }
  content?: DynamicFormSlotContent<DynamicFormBuilderPageContentContext>
}

type CreatorTab = 'designer' | 'preview' | 'json'

function normalizeLoadedForm(data: DynamicFormDefinition): DynamicFormDefinition {
  const fields =
    data.fields?.length > 0
      ? data.fields
      : rjsfToFields(data.schema, data.uiSchema)
  return syncDefinitionFromFields({ ...data, fields })
}

/**
 * SurveyJS-style creator: Designer / Preview / JSON Editor.
 * Runtime render = RJSF + PrimeReact widgets.
 */
export function DynamicFormBuilderPage({
  formId,
  form: formProp,
  loading: loadingProp,
  locale = 'vi',
  className,
  title,
  description,
  withShell = true,
  callback,
  content,
}: DynamicFormBuilderPageProps) {
  const messages = getDynamicFormMessages(locale)
  const b = messages.builder
  const controlled = formProp !== undefined

  const [form, setForm] = useState<DynamicFormDefinition | null>(
    formProp ? normalizeLoadedForm(formProp) : null,
  )
  const [baseline, setBaseline] = useState<DynamicFormDefinition | null>(null)
  const [loading, setLoading] = useState(!controlled && Boolean(formId))
  const [saving, setSaving] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [tab, setTab] = useState<CreatorTab>('designer')
  const [jsonSubTab, setJsonSubTab] = useState<
    'schema' | 'uiSchema' | 'formData'
  >('schema')

  useEffect(() => {
    if (controlled) {
      const next = formProp ? normalizeLoadedForm(formProp) : null
      setForm(next)
      setBaseline(next)
    }
  }, [controlled, formProp])

  useEffect(() => {
    if (controlled && loadingProp !== undefined) setLoading(loadingProp)
  }, [controlled, loadingProp])

  const reload = useCallback(async () => {
    if (controlled) return
    if (!formId) {
      setForm(null)
      setBaseline(null)
      setLoading(false)
      return
    }
    setLoading(true)
    try {
      const outcome = await handleAction<
        { formId: string },
        string,
        DynamicFormGetResult
      >({
        ctx: { formId },
        callback: callback?.load,
        defaultSubmit: callGetDynamicForm,
        getPayload: ({ formId: id }) => id,
        onSuccess: (_ctx, result) => {
          const next = normalizeLoadedForm(result.data)
          setForm(next)
          setBaseline(next)
        },
      })
      if (outcome.status === 'cancelled') {
        setForm(null)
        setBaseline(null)
      }
    } catch (error) {
      notify.error(getErrorMessage(error, b.error))
      setForm(null)
      setBaseline(null)
    } finally {
      setLoading(false)
    }
  }, [b.error, callback?.load, controlled, formId])

  useEffect(() => {
    if (controlled) return
    void reload()
  }, [controlled, reload])

  const selectedField = useMemo(
    () => (form && selectedId ? findFieldById(form.fields, selectedId) : null),
    [form, selectedId],
  )

  const updateFields = (fields: DynamicFormField[]) => {
    setForm((current) =>
      current ? syncDefinitionFromFields({ ...current, fields }) : current,
    )
  }

  const addField = useCallback(
    (
      type: DynamicFormField['type'],
      options?: {
        beforeKey?: string
        afterKey?: string
        parentKey?: string | null
      },
    ) => {
      setForm((current) => {
        if (!current) return current
        let parentKey = options?.parentKey ?? null
        if (parentKey === undefined) parentKey = null
        const field = createField(type, collectFieldKeys(current.fields))
        const next = insertFieldInTree(current.fields, field, {
          parentKey,
          beforeKey: options?.beforeKey,
          afterKey: options?.afterKey,
        })
        setSelectedId(field.id)
        return syncDefinitionFromFields({ ...current, fields: next })
      })
      setTab('designer')
    },
    [],
  )

  const reorderByKey = useCallback(
    (fromKey: string, targetKey: string, edge: 'before' | 'after' = 'before') => {
      setForm((current) => {
        if (!current) return current
        return syncDefinitionFromFields({
          ...current,
          fields: moveFieldInTree(current.fields, fromKey, targetKey, edge),
        })
      })
    },
    [],
  )

  const moveIntoGroup = useCallback((fromKey: string, parentKey: string) => {
    setForm((current) => {
      if (!current) return current
      const from = findFieldByKey(current.fields, fromKey)
      if (!from || from.key === parentKey) return current
      if (from.type === 'group' && findFieldByKey(from.children ?? [], parentKey)) {
        return current
      }
      const without = removeFieldFromTree(current.fields, from.id)
      const next = insertFieldInTree(without, from, { parentKey })
      return syncDefinitionFromFields({ ...current, fields: next })
    })
  }, [])

  const removeField = (id: string) => {
    if (!form) return
    updateFields(removeFieldFromTree(form.fields, id))
    setSelectedId((current) => (current === id ? null : current))
  }

  const moveField = (id: string, dir: -1 | 1) => {
    if (!form) return
    updateFields(moveFieldById(form.fields, id, dir))
  }

  const patchField = (next: DynamicFormField) => {
    if (!form) return
    // Allow clearing Title / Field name (and table column title/name) while typing;
    // enforce on Save.
    const issues = validateFieldDraft(next, form.fields).filter(
      (issue) =>
        issue.code !== 'field.label.required' &&
        issue.code !== 'field.key.required' &&
        issue.code !== 'field.table.column.label' &&
        issue.code !== 'field.table.column.key',
    )
    const message = firstValidationMessage(issues)
    if (message) {
      notify.error(message)
      return
    }
    updateFields(updateFieldInTree(form.fields, next))
  }

  const patchFormMeta = (patch: {
    name?: string
    description?: string
  }) => {
    setForm((current) => {
      if (!current) return current
      const next = {
        ...current,
        ...patch,
      }
      return syncDefinitionFromFields(next)
    })
  }

  const resetForm = () => {
    const forms = resetFormsFromMockJson()
    const seed =
      (formId ? forms.find((f) => f.id === formId) : undefined) ??
      forms[0] ??
      createDemoDefinitionSeed()
    setForm(normalizeLoadedForm(seed))
    setBaseline(normalizeLoadedForm(seed))
    setSelectedId(null)
    setTab('designer')
    notify.success('Form reset')
  }

  const save = useCallback(async () => {
    if (!form) return
    const synced = syncDefinitionFromFields(form)
    const issues = validateFormDefinition(synced)
    const message = firstValidationMessage(issues)
    if (message) {
      notify.error(message)
      return
    }
    setSaving(true)
    try {
      const payload: UpdateDynamicFormPayload = {
        id: synced.id,
        name: synced.name,
        description: synced.description,
        fields: synced.fields,
        schema: synced.schema,
        uiSchema: synced.uiSchema,
        formData: synced.formData,
      }
      const outcome = await handleAction<
        { form: DynamicFormDefinition },
        UpdateDynamicFormPayload,
        DynamicFormUpdateResult
      >({
        ctx: { form: synced },
        callback: callback?.save,
        defaultSubmit: callUpdateDynamicForm,
        getPayload: () => payload,
        onSuccess: (_ctx, result) => {
          const next = normalizeLoadedForm(result.data)
          setForm(next)
          setBaseline(next)
          notify.success(b.saved)
        },
      })
      if (outcome.status === 'cancelled') return
    } catch (error) {
      notify.error(getErrorMessage(error, b.error))
    } finally {
      setSaving(false)
    }
  }, [b.error, b.saved, callback?.save, form])

  const isLoading = controlled ? Boolean(loadingProp) : loading

  const selectByKey = (key: string) => {
    if (!form) return
    const found = findFieldByKey(form.fields, key)
    if (found) setSelectedId(found.id)
  }

  const addFromToolbox = useCallback(
    (type: DynamicFormField['type']) => {
      const parentKey =
        selectedField?.type === 'group' ? selectedField.key : null
      addField(type, { parentKey })
    },
    [addField, selectedField],
  )

  const jsonValue = useMemo(() => {
    if (!form) return ''
    if (jsonSubTab === 'schema') return prettyJson(form.schema)
    if (jsonSubTab === 'uiSchema') return prettyJson(form.uiSchema)
    return prettyJson(form.formData)
  }, [form, jsonSubTab])

  const copyJson = async () => {
    try {
      await navigator.clipboard.writeText(jsonValue)
      notify.success('Copied')
    } catch {
      notify.error('Copy failed')
    }
  }

  const designerMode = tab === 'designer'
  const previewOnly = tab === 'preview'

  const creator =
    isLoading || !form ? (
      <div className="kit-svc kit-svc--loading">
        <p className="m-0 text-sm text-slate-500">Loading…</p>
      </div>
    ) : (
      <div className={['kit-svc', previewOnly ? 'kit-svc--preview' : ''].filter(Boolean).join(' ')}>
        <header className="kit-svc__topbar">
          <nav className="kit-svc__tabs" aria-label="Creator tabs">
            {(
              [
                ['designer', 'Designer'],
                ['preview', 'Preview'],
                ['json', 'JSON Editor'],
              ] as const
            ).map(([id, label]) => (
              <button
                key={id}
                type="button"
                className={[
                  'kit-svc__tab',
                  tab === id ? 'is-active' : '',
                ]
                  .filter(Boolean)
                  .join(' ')}
                onClick={() => {
                  setTab(id)
                  if (id !== 'designer') setSelectedId(null)
                }}
              >
                {label}
              </button>
            ))}
          </nav>

          <div className="kit-svc__tools">
            <button
              type="button"
              className="kit-svc__icon-btn"
              title="Undo"
              disabled
              aria-disabled
            >
              <Undo2 className="h-4 w-4" />
            </button>
            <button
              type="button"
              className="kit-svc__icon-btn"
              title="Redo"
              disabled
              aria-disabled
            >
              <Redo2 className="h-4 w-4" />
            </button>
            <button
              type="button"
              className="kit-svc__icon-btn"
              title="Reset"
              onClick={resetForm}
            >
              <Eraser className="h-4 w-4" />
            </button>
            <button
              type="button"
              className="kit-svc__icon-btn"
              title="Form settings"
              onClick={() => {
                setTab('designer')
                setSelectedId(null)
              }}
            >
              <Settings2 className="h-4 w-4" />
            </button>
            <Button
              type="button"
              unstyled
              className={`${btnPrimaryClass} kit-svc__save`}
              disabled={saving}
              onClick={() => void save()}
            >
              <Save className="h-3.5 w-3.5" />
              {saving ? 'Saving…' : 'Save'}
            </Button>
            <button
              type="button"
              className="kit-svc__icon-btn"
              title={title ?? 'Dynamic Form Builder'}
            >
              <CircleHelp className="h-4 w-4" />
            </button>
          </div>
        </header>

        {tab === 'json' ? (
          <section className="kit-svc-json" aria-label="JSON Editor">
            <div className="kit-svc-json__bar">
              <div className="kit-svc-json__tabs" role="tablist">
                {(
                  [
                    ['schema', 'Schema'],
                    ['uiSchema', 'UI Schema'],
                    ['formData', 'Form Data'],
                  ] as const
                ).map(([id, label]) => (
                  <button
                    key={id}
                    type="button"
                    role="tab"
                    aria-selected={jsonSubTab === id}
                    className={[
                      'kit-svc-json__tab',
                      jsonSubTab === id ? 'is-active' : '',
                    ]
                      .filter(Boolean)
                      .join(' ')}
                    onClick={() => setJsonSubTab(id)}
                  >
                    {label}
                  </button>
                ))}
              </div>
              <Button
                type="button"
                unstyled
                className={btnOutlinedClass}
                onClick={() => void copyJson()}
              >
                Copy
              </Button>
            </div>
            <Textarea
              unstyled
              readOnly
              className="kit-svc-json__code"
              value={jsonValue}
              spellCheck={false}
            />
          </section>
        ) : (
          <div className="kit-svc__workspace">
            {designerMode ? (
              <FieldLibrary onAdd={addFromToolbox} />
            ) : (
              <div className="kit-svc-toolbox kit-svc-toolbox--spacer" aria-hidden />
            )}

            <section className="kit-svc-canvas" aria-label="Form canvas">
              <div
                className="kit-svc-canvas__stage"
                onClick={() => {
                  if (designerMode) setSelectedId(null)
                }}
              >
                <div className="kit-svc-canvas__surface">
                  {description ? (
                    <p className="kit-svc-canvas__hint">{description}</p>
                  ) : null}
                  <DynamicFormRenderer
                    schema={form.schema}
                    uiSchema={form.uiSchema}
                    formData={form.formData}
                    liveValidate={false}
                    showErrorList={false}
                    disabled={submitting}
                    emptyMessage="No fields yet"
                    emptyHint="Drag a field from the toolbox onto the canvas"
                    submitLabel={submitting ? 'Submitting…' : 'Complete'}
                    formContext={
                      designerMode
                        ? {
                            builderMode: true,
                            selectedKey: selectedField?.key ?? null,
                            onSelectField: selectByKey,
                            onDropFieldType: addField,
                            onReorderFields: reorderByKey,
                            onMoveIntoGroup: moveIntoGroup,
                          }
                        : undefined
                    }
                    onChange={(data) => {
                      setForm((curr) =>
                        curr ? { ...curr, formData: data } : curr,
                      )
                    }}
                    onSubmit={async (data) => {
                      if (!form?.id || submitting) return
                      setSubmitting(true)
                      try {
                        await callCreateDynamicFormSubmission({
                          formId: form.id,
                          data,
                        })
                        notify.success('Form submitted')
                      } catch (error) {
                        notify.error(
                          getErrorMessage(error, 'Submit failed'),
                        )
                      } finally {
                        setSubmitting(false)
                      }
                    }}
                  />
                </div>

                {designerMode && selectedField ? (
                  <div
                    className="kit-svc-canvas__fab"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <button
                      type="button"
                      className="kit-svc-canvas__fab-btn"
                      title="Move up"
                      onClick={() => moveField(selectedField.id, -1)}
                    >
                      <ArrowUp className="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      className="kit-svc-canvas__fab-btn"
                      title="Move down"
                      onClick={() => moveField(selectedField.id, 1)}
                    >
                      <ArrowDown className="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      className="kit-svc-canvas__fab-btn kit-svc-canvas__fab-btn--danger"
                      title="Delete field"
                      onClick={() => removeField(selectedField.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                ) : null}
              </div>
            </section>

            {designerMode ? (
              <FieldSettingsPanel
                form={form}
                field={selectedField}
                onChangeForm={patchFormMeta}
                onChange={patchField}
                onDelete={() => {
                  if (selectedField) removeField(selectedField.id)
                }}
              />
            ) : (
              <div className="kit-svc-props kit-svc-props--spacer" aria-hidden />
            )}
          </div>
        )}
      </div>
    )

  const DefaultLayout = withShell ? (
    <div
      className={['kit-dynamic-form-page font-sans text-ink', className]
        .filter(Boolean)
        .join(' ')}
    >
      {creator}
    </div>
  ) : (
    <div className={className}>{creator}</div>
  )

  return (
    <>
      {resolveDynamicFormContent(
        content,
        {
          form,
          loading: isLoading,
          saving,
          save,
          reload,
          DefaultLayout,
        },
        DefaultLayout,
      )}
    </>
  )
}
