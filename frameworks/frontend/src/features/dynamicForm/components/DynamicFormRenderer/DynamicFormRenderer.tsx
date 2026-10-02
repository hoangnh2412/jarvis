import { useEffect, useMemo, useRef, useState } from 'react'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import type { IChangeEvent } from '@rjsf/core'
import type { RJSFSchema, UiSchema } from '@rjsf/utils'
import { dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import {
  dynamicFormFields,
  dynamicFormTemplates,
  dynamicFormWidgets,
  formatFieldError,
} from '../../lib'
import type { DynamicFormBuilderFormContext } from '../../lib/templates'
import type { DynamicFormFieldType } from '../../types'

export type DynamicFormRendererProps = {
  schema: RJSFSchema
  uiSchema?: UiSchema
  formData?: Record<string, unknown>
  submitLabel?: string
  emptyMessage?: string
  emptyHint?: string
  disabled?: boolean
  liveValidate?: boolean
  showErrorList?: false | 'top' | 'bottom'
  className?: string
  omitExtraData?: boolean
  liveOmit?: boolean
  noHtml5Validate?: boolean
  /** Ẩn nút submit (dùng khi builder muốn tự control) */
  hideSubmit?: boolean
  formContext?: DynamicFormBuilderFormContext
  onChange?: (data: Record<string, unknown>) => void
  onSubmit: (data: Record<string, unknown>) => void | Promise<void>
}

function isEmptySchema(schema: RJSFSchema | undefined) {
  if (!schema) return true
  const props = schema.properties
  if (!props || typeof props !== 'object') return true
  return Object.keys(props).length === 0 && !schema.oneOf && !schema.anyOf
}

function EmptyDropZone({
  emptyMessage,
  emptyHint,
  formContext,
}: {
  emptyMessage: string
  emptyHint?: string
  formContext?: DynamicFormBuilderFormContext
}) {
  const ref = useRef<HTMLDivElement>(null)
  const [isOver, setIsOver] = useState(false)

  useEffect(() => {
    const el = ref.current
    if (!el || !formContext?.builderMode) return
    return dropTargetForElements({
      element: el,
      getData: () => ({ target: 'empty-canvas' }),
      canDrop: ({ source }) => {
        const data = source.data as {
          source?: string
          fieldType?: DynamicFormFieldType
        }
        return data.source === 'palette' && Boolean(data.fieldType)
      },
      onDragEnter: () => setIsOver(true),
      onDragLeave: () => setIsOver(false),
      onDrop: ({ source }) => {
        setIsOver(false)
        const data = source.data as {
          source?: string
          fieldType?: DynamicFormFieldType
        }
        if (data.source === 'palette' && data.fieldType) {
          formContext.onDropFieldType?.(data.fieldType)
        }
      },
    })
  }, [formContext])

  return (
    <div
      ref={ref}
      className={[
        'kit-df-empty',
        isOver ? 'is-drop-over' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <p className="kit-df-empty__title">{emptyMessage}</p>
      {emptyHint ? <p className="kit-df-empty__hint">{emptyHint}</p> : null}
    </div>
  )
}

export function DynamicFormRenderer({
  schema,
  uiSchema,
  formData: formDataProp,
  submitLabel = 'Submit',
  emptyMessage = 'No fields yet',
  emptyHint,
  disabled = false,
  liveValidate = false,
  showErrorList = false,
  className = '',
  // Keep object groups in formData so nested `required` is validated on submit.
  omitExtraData = false,
  liveOmit = false,
  noHtml5Validate = true,
  hideSubmit = false,
  formContext,
  onChange,
  onSubmit,
}: DynamicFormRendererProps) {
  const [formData, setFormData] = useState<Record<string, unknown>>(
    formDataProp ?? {},
  )

  useEffect(() => {
    setFormData(formDataProp ?? {})
  }, [formDataProp])

  const resolvedUiSchema = useMemo<UiSchema>(() => {
    const fromSchema = (uiSchema?.['ui:submitButtonOptions'] ?? {}) as Record<
      string,
      unknown
    >
    const fromProps = (fromSchema.props ?? {}) as Record<string, unknown>
    return {
      ...(uiSchema ?? {}),
      'ui:submitButtonOptions': {
        ...fromSchema,
        submitText: submitLabel,
        norender: hideSubmit,
        props: {
          ...fromProps,
          disabled: disabled || Boolean(fromProps.disabled),
          className: [
            'pr-btn-primary',
            'kit-df-rjsf-submit',
            'inline-flex',
            'h-10',
            'items-center',
            'justify-center',
            'px-5',
            'text-sm',
            'font-semibold',
            typeof fromProps.className === 'string' ? fromProps.className : '',
          ]
            .filter(Boolean)
            .join(' '),
        },
      },
    }
  }, [uiSchema, submitLabel, disabled, hideSubmit])

  if (isEmptySchema(schema)) {
    return (
      <EmptyDropZone
        emptyMessage={emptyMessage}
        emptyHint={emptyHint}
        formContext={formContext}
      />
    )
  }

  return (
    <div
      className={['kit-df-rjsf', className].filter(Boolean).join(' ')}
      onKeyDown={(e) => {
        // Prevent Enter on inputs/checkbox from submitting; only Complete (type=submit) does.
        if (
          e.key === 'Enter' &&
          e.target instanceof HTMLElement &&
          e.target.tagName !== 'TEXTAREA' &&
          e.target.getAttribute('type') !== 'submit'
        ) {
          e.preventDefault()
        }
      }}
    >
      <Form
        schema={schema}
        uiSchema={resolvedUiSchema}
        formData={formData}
        validator={validator}
        widgets={dynamicFormWidgets}
        fields={dynamicFormFields}
        templates={dynamicFormTemplates}
        formContext={formContext}
        disabled={disabled}
        liveValidate={liveValidate}
        showErrorList={showErrorList}
        omitExtraData={omitExtraData}
        liveOmit={liveOmit}
        noHtml5Validate={noHtml5Validate}
        transformErrors={(errors) =>
          errors.map((err) => {
            const message = formatFieldError(err.message ?? '')
            const stack = formatFieldError(err.stack ?? err.message ?? '')
            return { ...err, message, stack }
          })
        }
        onChange={(e: IChangeEvent) => {
          const next = (e.formData ?? {}) as Record<string, unknown>
          setFormData(next)
          onChange?.(next)
        }}
        onSubmit={async (e: IChangeEvent) => {
          await onSubmit((e.formData ?? {}) as Record<string, unknown>)
        }}
      />
    </div>
  )
}
