import { useEffect, useRef, useState, type ReactElement } from 'react'
import {
  draggable,
  dropTargetForElements,
} from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import type {
  ArrayFieldItemTemplateProps,
  ArrayFieldTemplateProps,
  ErrorListProps,
  FieldErrorProps,
  FieldTemplateProps,
  ObjectFieldTemplateProps,
  StrictRJSFSchema,
  FormContextType,
  RJSFSchema,
  TemplatesType,
} from '@rjsf/utils'
import { GripVertical } from 'lucide-react'
import { Button } from 'primereact/button'
import type { DynamicFormFieldType } from '../types'
import { btnOutlinedClass, btnPrimaryClass } from '../components/fieldStyles'

export type FieldDropEdge = 'before' | 'after'

export type DynamicFormBuilderFormContext = {
  selectedKey?: string | null
  onSelectField?: (key: string) => void
  builderMode?: boolean
  onDropFieldType?: (
    type: DynamicFormFieldType,
    options?: {
      beforeKey?: string
      afterKey?: string
      parentKey?: string | null
    },
  ) => void
  onReorderFields?: (
    fromKey: string,
    targetKey: string,
    edge: FieldDropEdge,
  ) => void
  onMoveIntoGroup?: (fromKey: string, parentKey: string) => void
}

type DragPayload = {
  source?: string
  fieldType?: DynamicFormFieldType
  fieldKey?: string
}

type DropLocation = {
  current: {
    input: { clientY: number }
    dropTargets: Array<{ element: Element }>
  }
}

function isInnermostDropTarget(location: DropLocation, el: Element) {
  return location.current.dropTargets[0]?.element === el
}

function edgeFromPointer(el: Element, clientY: number): FieldDropEdge {
  const rect = el.getBoundingClientRect()
  const mid = rect.top + rect.height / 2
  return clientY < mid ? 'before' : 'after'
}

/** Top/bottom band → place beside group; middle → nest into group. */
function groupDropAction(
  el: Element,
  clientY: number,
): 'before' | 'after' | 'into' {
  const rect = el.getBoundingClientRect()
  const y = clientY - rect.top
  const band = Math.max(36, Math.min(56, rect.height * 0.22))
  if (y <= band) return 'before'
  if (y >= rect.height - band) return 'after'
  return 'into'
}

/** Insert before/after a direct child based on pointer Y (root or group body). */
function insertBesideFromY(
  container: Element,
  clientY: number,
  childKeys: string[],
): { beforeKey?: string; afterKey?: string } {
  if (childKeys.length === 0) return {}
  const body =
    container.querySelector(':scope > .kit-df-rjsf-form__body') ??
    container.querySelector(':scope > .kit-df-rjsf-group__body')
  const nodes = body
    ? Array.from(body.children).filter((node) =>
        (node as HTMLElement).classList?.contains('kit-df-rjsf-object__prop'),
      )
    : []

  for (let i = 0; i < nodes.length && i < childKeys.length; i++) {
    const rect = nodes[i].getBoundingClientRect()
    if (clientY < rect.top + rect.height / 2) {
      return { beforeKey: childKeys[i] }
    }
  }
  return { afterKey: childKeys[childKeys.length - 1] }
}

function FieldTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: FieldTemplateProps<T, S, F>) {
  const {
    id,
    label,
    required,
    description,
    errors,
    help,
    children,
    hidden,
    displayLabel,
    rawErrors,
    fieldPathId,
    registry,
    schema,
  } = props

  const ref = useRef<HTMLDivElement>(null)
  const handleRef = useRef<HTMLSpanElement>(null)
  const [dropEdge, setDropEdge] = useState<FieldDropEdge | null>(null)

  const path = fieldPathId?.path ?? []
  const last = path[path.length - 1]
  const name = typeof last === 'string' ? last : undefined
  const parentKey =
    path.length >= 2 && typeof path[path.length - 2] === 'string'
      ? String(path[path.length - 2])
      : null
  const isObjectField = schema?.type === 'object'
  const isTableField = schema?.type === 'array'

  const ctx = (registry.formContext ?? {}) as DynamicFormBuilderFormContext
  const selectable = Boolean(ctx.builderMode && name && ctx.onSelectField)
  const selected = Boolean(ctx.selectedKey && ctx.selectedKey === name)

  useEffect(() => {
    const el = ref.current
    const handle = handleRef.current
    if (!el || !name || !ctx.builderMode) return

    const cleanups: Array<() => void> = []

    if (handle) {
      cleanups.push(
        draggable({
          element: el,
          dragHandle: handle,
          getInitialData: () => ({
            source: 'canvas',
            fieldKey: name,
          }),
        }),
      )
    }

    // Group wrappers defer drop handling to ObjectFieldTemplate (nested).
    if (!isObjectField) {
      cleanups.push(
        dropTargetForElements({
          element: el,
          getData: () => ({
            target: 'field',
            fieldKey: name,
          }),
          canDrop: ({ source }) => {
            const data = source.data as DragPayload
            if (data.source === 'palette' && data.fieldType) return true
            if (
              data.source === 'canvas' &&
              data.fieldKey &&
              data.fieldKey !== name
            )
              return true
            return false
          },
          onDragEnter: ({ location }) => {
            if (!isInnermostDropTarget(location, el)) {
              setDropEdge(null)
              return
            }
            setDropEdge(edgeFromPointer(el, location.current.input.clientY))
          },
          onDrag: ({ location }) => {
            if (!isInnermostDropTarget(location, el)) {
              setDropEdge(null)
              return
            }
            setDropEdge(edgeFromPointer(el, location.current.input.clientY))
          },
          onDragLeave: () => setDropEdge(null),
          onDrop: ({ source, location }) => {
            setDropEdge(null)
            if (!isInnermostDropTarget(location, el)) return

            const data = source.data as DragPayload
            const edge = edgeFromPointer(el, location.current.input.clientY)

            if (data.source === 'palette' && data.fieldType) {
              ctx.onDropFieldType?.(data.fieldType, {
                parentKey,
                ...(edge === 'before'
                  ? { beforeKey: name }
                  : { afterKey: name }),
              })
              return
            }

            if (data.source === 'canvas' && data.fieldKey) {
              ctx.onReorderFields?.(data.fieldKey, name, edge)
            }
          },
        }),
      )
    }

    return () => {
      for (const stop of cleanups) stop()
    }
  }, [
    ctx.builderMode,
    ctx.onDropFieldType,
    ctx.onReorderFields,
    isObjectField,
    name,
    parentKey,
  ])

  if (hidden) return <div className="hidden">{children}</div>

  // Groups: drag handle lives on ObjectFieldTemplate header.
  if (isObjectField) {
    return (
      <div
        ref={ref}
        className={[
          'kit-df-rjsf-field',
          'kit-df-rjsf-field--group',
          rawErrors?.length ? 'kit-df-rjsf-field--error' : '',
          selectable ? 'kit-df-rjsf-field--selectable' : '',
          selected ? 'is-selected' : '',
        ]
          .filter(Boolean)
          .join(' ')}
        onClick={
          selectable && name
            ? (e) => {
                e.stopPropagation()
                ctx.onSelectField?.(name)
              }
            : undefined
        }
      >
        {children}
        {errors}
        {help}
      </div>
    )
  }

  // Tables: same white card shell as Form Group.
  if (isTableField) {
    return (
      <div
        ref={ref}
        className={[
          'kit-df-rjsf-field',
          'kit-df-rjsf-field--table',
          rawErrors?.length ? 'kit-df-rjsf-field--error' : '',
          selectable ? 'kit-df-rjsf-field--selectable' : '',
          selected ? 'is-selected' : '',
          dropEdge === 'before' ? 'is-drop-before' : '',
          dropEdge === 'after' ? 'is-drop-after' : '',
        ]
          .filter(Boolean)
          .join(' ')}
        onClick={
          selectable && name
            ? (e) => {
                e.stopPropagation()
                ctx.onSelectField?.(name)
              }
            : undefined
        }
      >
        <div className="kit-df-rjsf-table-card">
          <div className="kit-df-rjsf-table-card__head">
            {selectable ? (
              <span
                ref={handleRef}
                className="kit-df-rjsf-field__grip"
                title="Drag to reorder"
                onClick={(e) => e.stopPropagation()}
              >
                <GripVertical className="h-3.5 w-3.5" />
              </span>
            ) : ctx.builderMode ? (
              <span className="kit-df-rjsf-field__grip-spacer" aria-hidden />
            ) : null}
            <div className="kit-df-rjsf-table-card__heading">
              {label ? (
                <div
                  className={[
                    'kit-df-rjsf-table-card__title',
                    required ? 'kit-df-label-required' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                >
                  {label}
                  {required ? <span aria-hidden> *</span> : null}
                </div>
              ) : null}
              {description ? (
                <div className="kit-df-rjsf-table-card__description">
                  {description}
                </div>
              ) : null}
            </div>
          </div>
          <div className="kit-df-rjsf-table-card__body">
            {children}
            {errors}
            {help}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div
      ref={ref}
      className={[
        'kit-df-rjsf-field',
        rawErrors?.length ? 'kit-df-rjsf-field--error' : '',
        selectable ? 'kit-df-rjsf-field--selectable' : '',
        selected ? 'is-selected' : '',
        dropEdge === 'before' ? 'is-drop-before' : '',
        dropEdge === 'after' ? 'is-drop-after' : '',
      ]
        .filter(Boolean)
        .join(' ')}
      onClick={
        selectable && name
          ? (e) => {
              e.stopPropagation()
              ctx.onSelectField?.(name)
            }
          : undefined
      }
    >
      {(ctx.builderMode || (displayLabel && label)) && (
        <div className="kit-df-rjsf-field__head">
          {selectable ? (
            <span
              ref={handleRef}
              className="kit-df-rjsf-field__grip"
              title="Drag to reorder"
              onClick={(e) => e.stopPropagation()}
            >
              <GripVertical className="h-3.5 w-3.5" />
            </span>
          ) : ctx.builderMode ? (
            <span className="kit-df-rjsf-field__grip-spacer" aria-hidden />
          ) : null}
          {displayLabel && label ? (
            <label
              htmlFor={id}
              className={[
                'kit-df-rjsf-field__label',
                required ? 'kit-df-label-required' : '',
              ]
                .filter(Boolean)
                .join(' ')}
            >
              {label}
              {required ? <span aria-hidden> *</span> : null}
            </label>
          ) : ctx.builderMode ? (
            <span className="kit-df-rjsf-field__label-spacer" aria-hidden />
          ) : null}
        </div>
      )}
      <div className="kit-df-rjsf-field__body">
        {description ? (
          <div className="kit-df-rjsf-field__description">{description}</div>
        ) : null}
        {children}
        {errors}
        {help}
      </div>
    </div>
  )
}

function ObjectFieldTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: ObjectFieldTemplateProps<T, S, F>) {
  const {
    title,
    description,
    properties,
    required,
    registry,
    fieldPathId,
    uiSchema,
  } = props
  const path = fieldPathId?.path ?? []
  const isRoot = path.length === 0
  const groupKey =
    path.length > 0 && typeof path[path.length - 1] === 'string'
      ? String(path[path.length - 1])
      : null
  const parentOfGroup =
    path.length >= 2 && typeof path[path.length - 2] === 'string'
      ? String(path[path.length - 2])
      : null
  const childKeysSig = properties.map((prop) => prop.name).join('\0')

  const rootRef = useRef<HTMLFieldSetElement>(null)
  const handleRef = useRef<HTMLSpanElement>(null)
  const [isOver, setIsOver] = useState(false)
  const [groupEdge, setGroupEdge] = useState<FieldDropEdge | null>(null)
  const ctx = (registry.formContext ?? {}) as DynamicFormBuilderFormContext
  const selected = Boolean(
    groupKey && ctx.selectedKey && ctx.selectedKey === groupKey,
  )

  useEffect(() => {
    const el = rootRef.current
    if (!el || !ctx.builderMode) return

    const keys = childKeysSig ? childKeysSig.split('\0') : []
    const cleanups: Array<() => void> = []

    if (!isRoot && groupKey && handleRef.current) {
      cleanups.push(
        draggable({
          element: el,
          dragHandle: handleRef.current,
          getInitialData: () => ({
            source: 'canvas',
            fieldKey: groupKey,
          }),
        }),
      )
    }

    const updatePaletteIndicator = (clientY: number) => {
      if (isRoot) {
        const pos = insertBesideFromY(el, clientY, keys)
        setIsOver(true)
        setGroupEdge(pos.beforeKey ? 'before' : pos.afterKey ? 'after' : null)
        return
      }
      const action = groupDropAction(el, clientY)
      if (action === 'into') {
        setIsOver(true)
        setGroupEdge(null)
      } else {
        setIsOver(false)
        setGroupEdge(action)
      }
    }

    cleanups.push(
      dropTargetForElements({
        element: el,
        getData: () => ({
          target: isRoot ? 'canvas-root' : 'group-body',
          fieldKey: groupKey,
        }),
        canDrop: ({ source }) => {
          const data = source.data as DragPayload
          if (data.source === 'palette' && data.fieldType) return true
          if (
            !isRoot &&
            groupKey &&
            data.source === 'canvas' &&
            data.fieldKey &&
            data.fieldKey !== groupKey
          ) {
            return true
          }
          return false
        },
        onDragEnter: ({ location, source }) => {
          if (!isInnermostDropTarget(location, el)) {
            setIsOver(false)
            setGroupEdge(null)
            return
          }
          const data = source.data as DragPayload
          const y = location.current.input.clientY
          if (data.source === 'palette') {
            updatePaletteIndicator(y)
            return
          }
          if (!isRoot && groupKey && data.source === 'canvas') {
            const action = groupDropAction(el, y)
            if (action === 'into') {
              setIsOver(true)
              setGroupEdge(null)
            } else {
              setIsOver(false)
              setGroupEdge(action)
            }
          }
        },
        onDrag: ({ location, source }) => {
          if (!isInnermostDropTarget(location, el)) {
            setIsOver(false)
            setGroupEdge(null)
            return
          }
          const data = source.data as DragPayload
          const y = location.current.input.clientY
          if (data.source === 'palette') {
            updatePaletteIndicator(y)
            return
          }
          if (!isRoot && groupKey && data.source === 'canvas') {
            const action = groupDropAction(el, y)
            if (action === 'into') {
              setIsOver(true)
              setGroupEdge(null)
            } else {
              setIsOver(false)
              setGroupEdge(action)
            }
          }
        },
        onDragLeave: () => {
          setIsOver(false)
          setGroupEdge(null)
        },
        onDrop: ({ source, location }) => {
          setIsOver(false)
          setGroupEdge(null)
          if (!isInnermostDropTarget(location, el)) return

          const data = source.data as DragPayload
          const y = location.current.input.clientY

          if (data.source === 'palette' && data.fieldType) {
            if (isRoot) {
              const pos = insertBesideFromY(el, y, keys)
              ctx.onDropFieldType?.(data.fieldType, {
                parentKey: null,
                ...pos,
              })
              return
            }

            if (groupKey) {
              const action = groupDropAction(el, y)
              if (action === 'into') {
                const pos = insertBesideFromY(el, y, keys)
                ctx.onDropFieldType?.(data.fieldType, {
                  parentKey: groupKey,
                  ...pos,
                })
              } else {
                ctx.onDropFieldType?.(data.fieldType, {
                  parentKey: parentOfGroup,
                  ...(action === 'before'
                    ? { beforeKey: groupKey }
                    : { afterKey: groupKey }),
                })
              }
            }
            return
          }

          if (
            !isRoot &&
            groupKey &&
            data.source === 'canvas' &&
            data.fieldKey
          ) {
            const action = groupDropAction(el, y)
            if (action === 'into') {
              ctx.onMoveIntoGroup?.(data.fieldKey, groupKey)
            } else {
              ctx.onReorderFields?.(data.fieldKey, groupKey, action)
            }
          }
        },
      }),
    )

    return () => {
      for (const stop of cleanups) stop()
    }
  }, [
    childKeysSig,
    ctx.builderMode,
    ctx.onDropFieldType,
    ctx.onMoveIntoGroup,
    ctx.onReorderFields,
    groupKey,
    isRoot,
    parentOfGroup,
  ])

  return (
    <fieldset
      ref={rootRef}
      className={[
        isRoot ? 'kit-df-rjsf-form' : 'kit-df-rjsf-group',
        isOver ? 'is-drop-over' : '',
        groupEdge === 'before' ? 'is-drop-before' : '',
        groupEdge === 'after' ? 'is-drop-after' : '',
        selected ? 'is-selected' : '',
      ]
        .filter(Boolean)
        .join(' ')}
      onClick={
        !isRoot && groupKey && ctx.builderMode
          ? (e) => {
              e.stopPropagation()
              ctx.onSelectField?.(groupKey)
            }
          : undefined
      }
    >
      {!isRoot ? (
        <div className="kit-df-rjsf-group__head">
          {ctx.builderMode ? (
            <span
              ref={handleRef}
              className="kit-df-rjsf-field__grip"
              title="Drag to reorder section"
              onClick={(e) => e.stopPropagation()}
            >
              <GripVertical className="h-3.5 w-3.5" />
            </span>
          ) : null}
          <div className="kit-df-rjsf-group__heading">
            {title ? (
              <div className="kit-df-rjsf-group__title">
                {title}
                {required ? <span aria-hidden> *</span> : null}
              </div>
            ) : null}
            {description ? (
              <p className="kit-df-rjsf-group__description">{description}</p>
            ) : null}
          </div>
        </div>
      ) : (
        <>
          {title ? (
            <legend className="kit-df-rjsf-form__title">{title}</legend>
          ) : null}
          {description ? (
            <p className="kit-df-rjsf-form__description">{description}</p>
          ) : null}
        </>
      )}
      <div
        className={
          isRoot ? 'kit-df-rjsf-form__body' : 'kit-df-rjsf-group__body'
        }
      >
        {properties.map((prop) => {
          const propUi = (uiSchema?.[prop.name] ?? {}) as {
            'ui:options'?: { layoutWidth?: string }
          }
          const width =
            propUi['ui:options']?.layoutWidth === 'half' ? 'half' : 'full'
          return (
            <div
              key={prop.name}
              className={[
                'kit-df-rjsf-object__prop',
                `kit-df-rjsf-object__prop--${width}`,
              ].join(' ')}
            >
              {prop.content}
            </div>
          )
        })}
      </div>
      {!isRoot && ctx.builderMode && properties.length === 0 ? (
        <p className="kit-df-rjsf-group__empty">
          Drop fields here to build this section
        </p>
      ) : null}
      {isRoot && ctx.builderMode && properties.length === 0 ? (
        <p className="kit-svc-canvas__drop-hint">
          Drag a field or Form Group from the toolbox
        </p>
      ) : null}
    </fieldset>
  )
}

function ArrayFieldTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: ArrayFieldTemplateProps<T, S, F>) {
  const { title, items, canAdd, onAddClick, disabled, readonly, required } = props
  return (
    <fieldset className="kit-df-rjsf-array">
      {title ? (
        <legend className="kit-df-rjsf-array__title">
          {title}
          {required ? <span aria-hidden> *</span> : null}
        </legend>
      ) : null}
      <div className="kit-df-rjsf-array__items">{items}</div>
      {canAdd ? (
        <Button
          type="button"
          unstyled
          className={`${btnPrimaryClass} mt-2`}
          disabled={disabled || readonly}
          onClick={onAddClick}
        >
          Add item
        </Button>
      ) : null}
    </fieldset>
  )
}

function ArrayFieldItemTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: ArrayFieldItemTemplateProps<T, S, F>) {
  const { children, buttonsProps, hasToolbar, registry } = props
  const Buttons = registry.templates.ArrayFieldItemButtonsTemplate
  return (
    <div className="kit-df-rjsf-array__item">
      <div className="kit-df-rjsf-array__item-body">{children}</div>
      {hasToolbar && Buttons ? (
        <div className="kit-df-rjsf-array__item-actions">
          <Buttons {...buttonsProps} className={btnOutlinedClass} />
        </div>
      ) : null}
    </div>
  )
}

export function formatFieldError(message: string): string {
  const trimmed = message.trim()
  if (!trimmed) return message

  let next = trimmed
  const required = /^must have required property '(.+)'$/i.exec(trimmed)
  if (required) next = `${required[1]} is required`
  else {
    const requiredAlt = /^'(.+)' is a required property$/i.exec(trimmed)
    if (requiredAlt) next = `${requiredAlt[1]} is required`
    else if (/^must be equal to constant$/i.test(trimmed)) {
      next = 'This field is required'
    } else if (/^must be equal to one of the allowed values$/i.test(trimmed)) {
      next = 'This field is required'
    }
  }

  return next.charAt(0).toUpperCase() + next.slice(1)
}

function FieldErrorTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(props: FieldErrorProps<T, S, F>): ReactElement | null {
  const { errors = [] } = props
  const list = errors.filter(Boolean).map((error) => formatFieldError(String(error)))
  if (!list.length) return null
  return (
    <div className="kit-df-rjsf-field-error" role="alert">
      {list.map((message, index) => (
        <p key={index} className="kit-df-rjsf-field-error__msg">
          {message}
        </p>
      ))}
    </div>
  )
}

function ErrorListTemplate<
  T = unknown,
  S extends StrictRJSFSchema = RJSFSchema,
  F extends FormContextType = any,
>(_props: ErrorListProps<T, S, F>): ReactElement | null {
  // Field-level errors only — no top summary card.
  return null
}

export const dynamicFormTemplates = {
  FieldTemplate,
  ObjectFieldTemplate,
  ArrayFieldTemplate,
  ArrayFieldItemTemplate,
  FieldErrorTemplate,
  ErrorListTemplate,
} satisfies Partial<TemplatesType> as unknown as Partial<TemplatesType>
