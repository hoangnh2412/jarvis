import { useMemo, useState, type ReactNode } from 'react'
import { ChevronDown, Search, Trash2 } from 'lucide-react'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { Textarea } from 'primereact/textarea'
import type {
  DynamicFormDefinition,
  DynamicFormField,
} from '../../types'
import {
  createSelectOption,
  createTableColumn,
  FIELD_TYPE_META,
  isTableColumnType,
  normalizeSelectOptions,
  TABLE_COLUMN_TYPES,
} from '../../utils'
import {
  btnOutlinedClass,
  fieldInputClass,
  fieldTextareaClass,
} from '../fieldStyles'

export type FieldSettingsPanelProps = {
  form: DynamicFormDefinition
  field: DynamicFormField | null
  onChangeForm: (patch: { name?: string; description?: string }) => void
  onChange: (next: DynamicFormField) => void
  onDelete: () => void
}

function PropBlock({
  label,
  children,
}: {
  label: string
  children: ReactNode
}) {
  return (
    <div className="kit-svc-props__block">
      <span className="kit-svc-props__label">{label}</span>
      {children}
    </div>
  )
}

function PropAccordion({
  title,
  open,
  onToggle,
  children,
}: {
  title: string
  open: boolean
  onToggle: () => void
  children: ReactNode
}) {
  return (
    <div className={['kit-svc-props__acc', open ? 'is-open' : ''].join(' ')}>
      <button
        type="button"
        className="kit-svc-props__acc-head"
        aria-expanded={open}
        onClick={onToggle}
      >
        <span>{title}</span>
        <ChevronDown
          className={[
            'kit-svc-props__chevron',
            open ? 'is-open' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        />
      </button>
      {open ? <div className="kit-svc-props__acc-body">{children}</div> : null}
    </div>
  )
}

export function FieldSettingsPanel({
  form,
  field,
  onChangeForm,
  onChange,
  onDelete,
}: FieldSettingsPanelProps) {
  const [query, setQuery] = useState('')
  const [openSections, setOpenSections] = useState({
    general: true,
    layout: true,
    validation: true,
    choices: true,
    columns: true,
    defaults: false,
  })
  const q = query.trim().toLowerCase()
  const visible = (label: string) => !q || label.toLowerCase().includes(q)

  const options = useMemo(
    () => (field ? normalizeSelectOptions(field.options) : []),
    [field],
  )

  const toggle = (key: keyof typeof openSections) => {
    setOpenSections((prev) => ({ ...prev, [key]: !prev[key] }))
  }

  if (!field) {
    return (
      <aside className="kit-svc-props" aria-label="Form settings">
        <div className="kit-svc-props__head">
          <div>
            <h2 className="kit-svc-props__title">General</h2>
            <p className="kit-svc-props__subtitle">Form</p>
          </div>
        </div>
        <div className="kit-svc-props__search">
          <Search className="kit-svc-props__search-icon" aria-hidden />
          <InputText
            unstyled
            className={`${fieldInputClass} kit-svc-props__search-input`}
            value={query}
            placeholder="Type to search..."
            onChange={(e: { target: { value: string } }) =>
              setQuery(e.target.value)
            }
          />
        </div>
        <div className="kit-svc-props__body kit-svc-props__body--flat">
          <PropAccordion
            title="General"
            open={openSections.general}
            onToggle={() => toggle('general')}
          >
            {visible('Form title') ? (
              <PropBlock label="Form title">
                <InputText
                  unstyled
                  className={fieldInputClass}
                  value={form.name}
                  onChange={(e: { target: { value: string } }) =>
                    onChangeForm({ name: e.target.value })
                  }
                />
              </PropBlock>
            ) : null}
            {visible('Form description') ? (
              <PropBlock label="Form description">
                <Textarea
                  unstyled
                  className={fieldTextareaClass}
                  rows={3}
                  value={form.description ?? ''}
                  onChange={(e: { target: { value: string } }) =>
                    onChangeForm({ description: e.target.value })
                  }
                />
              </PropBlock>
            ) : null}
          </PropAccordion>
          <p className="kit-svc-props__hint">
            Select a field on the canvas to edit its properties.
          </p>
        </div>
      </aside>
    )
  }

  const isChoice = field.type === 'select' || field.type === 'radio'
  const isTable = field.type === 'table'
  const columns = field.children ?? []
  const isStringy =
    field.type === 'text' ||
    field.type === 'email' ||
    field.type === 'password' ||
    field.type === 'textarea'
  const isNumber = field.type === 'number'

  const patchColumn = (index: number, patch: Partial<DynamicFormField>) => {
    const next = columns.map((col, i) =>
      i === index ? { ...col, ...patch } : col,
    )
    onChange({ ...field, children: next })
  }

  return (
    <aside className="kit-svc-props" aria-label="Field settings">
      <div className="kit-svc-props__head">
        <div>
          <h2 className="kit-svc-props__title">Properties</h2>
          <p className="kit-svc-props__subtitle">{field.label || 'Field'}</p>
        </div>
      </div>
      <div className="kit-svc-props__search">
        <Search className="kit-svc-props__search-icon" aria-hidden />
        <InputText
          unstyled
          className={`${fieldInputClass} kit-svc-props__search-input`}
          value={query}
          placeholder="Type to search..."
          onChange={(e: { target: { value: string } }) =>
            setQuery(e.target.value)
          }
        />
      </div>

      <div className="kit-svc-props__body kit-svc-props__body--flat">
        <PropAccordion
          title="General"
          open={openSections.general}
          onToggle={() => toggle('general')}
        >
          {visible('Field name') ? (
            <PropBlock label="Field name">
              <InputText
                unstyled
                className={fieldInputClass}
                value={field.key}
                spellCheck={false}
                onChange={(e: { target: { value: string } }) =>
                  onChange({ ...field, key: e.target.value })
                }
              />
            </PropBlock>
          ) : null}
          {visible('Title') ? (
            <PropBlock label="Title">
              <InputText
                unstyled
                className={fieldInputClass}
                value={field.label}
                onChange={(e: { target: { value: string } }) =>
                  onChange({ ...field, label: e.target.value })
                }
              />
            </PropBlock>
          ) : null}
          {visible('Description') ? (
            <PropBlock label="Description">
              <InputText
                unstyled
                className={fieldInputClass}
                value={field.helpText ?? ''}
                placeholder="Optional help text"
                onChange={(e: { target: { value: string } }) =>
                  onChange({ ...field, helpText: e.target.value })
                }
              />
            </PropBlock>
          ) : null}
        {field.type !== 'checkbox' &&
        field.type !== 'radio' &&
        field.type !== 'group' &&
        field.type !== 'table' &&
        visible('Placeholder') ? (
            <PropBlock label="Placeholder">
              <InputText
                unstyled
                className={fieldInputClass}
                value={field.placeholder ?? ''}
                onChange={(e: { target: { value: string } }) =>
                  onChange({ ...field, placeholder: e.target.value })
                }
              />
            </PropBlock>
          ) : null}
          {field.type !== 'group' && visible('Required') ? (
            <label className="kit-svc-props__check">
              <input
                type="checkbox"
                checked={Boolean(field.required)}
                onChange={(e) =>
                  onChange({ ...field, required: e.target.checked })
                }
              />
              <span>
                {isTable
                  ? 'Require at least one row'
                  : 'Is required'}
              </span>
            </label>
          ) : null}

          {field.type === 'group' ? (
            <p className="kit-svc-props__hint">
              {(field.children?.length ?? 0) === 0
                ? 'Empty section — drop fields into this group on the canvas.'
                : `${field.children!.length} field(s) inside this section. Select the group, then add fields from the toolbox.`}
            </p>
          ) : null}
          {isTable ? (
            <p className="kit-svc-props__hint">
              Configure columns below. Table rows come from backend form data
              (no add/remove row on the form).
            </p>
          ) : null}
        </PropAccordion>

        <PropAccordion
          title="Layout"
          open={openSections.layout}
          onToggle={() => toggle('layout')}
        >
          {visible('Width') ? (
            <PropBlock label="Width">
              <div className="kit-svc-props__segment" role="group" aria-label="Layout width">
                <button
                  type="button"
                  className={[
                    'kit-svc-props__segment-btn',
                    (field.layoutWidth ?? 'full') === 'full' ? 'is-active' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={() =>
                    onChange({ ...field, layoutWidth: 'full' })
                  }
                >
                  Full
                </button>
                <button
                  type="button"
                  className={[
                    'kit-svc-props__segment-btn',
                    field.layoutWidth === 'half' ? 'is-active' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={() =>
                    onChange({ ...field, layoutWidth: 'half' })
                  }
                >
                  Half
                </button>
              </div>
              <p className="kit-svc-props__hint">
                Full stacks vertically. Half places this next to another Half
                item on the same row.
              </p>
            </PropBlock>
          ) : null}
        </PropAccordion>

        {(isStringy || isNumber) && field.type !== 'group' && (
          <PropAccordion
            title="Validation"
            open={openSections.validation}
            onToggle={() => toggle('validation')}
          >
            {isStringy ? (
              <div className="kit-svc-props__row">
                <PropBlock label="Minimum length">
                  <InputText
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    value={String(field.minLength ?? '')}
                    onChange={(e: { target: { value: string } }) =>
                      onChange({
                        ...field,
                        minLength:
                          e.target.value === ''
                            ? undefined
                            : Number(e.target.value),
                      })
                    }
                  />
                </PropBlock>
                <PropBlock label="Maximum length">
                  <InputText
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    value={String(field.maxLength ?? '')}
                    onChange={(e: { target: { value: string } }) =>
                      onChange({
                        ...field,
                        maxLength:
                          e.target.value === ''
                            ? undefined
                            : Number(e.target.value),
                      })
                    }
                  />
                </PropBlock>
              </div>
            ) : null}
            {isNumber ? (
              <>
                <div className="kit-svc-props__row">
                  <PropBlock label="Minimum">
                    <InputText
                      unstyled
                      type="number"
                      className={fieldInputClass}
                      value={String(field.minimum ?? '')}
                      onChange={(e: { target: { value: string } }) =>
                        onChange({
                          ...field,
                          minimum:
                            e.target.value === ''
                              ? undefined
                              : Number(e.target.value),
                        })
                      }
                    />
                  </PropBlock>
                  <PropBlock label="Maximum">
                    <InputText
                      unstyled
                      type="number"
                      className={fieldInputClass}
                      value={String(field.maximum ?? '')}
                      onChange={(e: { target: { value: string } }) =>
                        onChange({
                          ...field,
                          maximum:
                            e.target.value === ''
                              ? undefined
                              : Number(e.target.value),
                        })
                      }
                    />
                  </PropBlock>
                </div>
                <PropBlock label="Step">
                  <InputText
                    unstyled
                    type="number"
                    className={fieldInputClass}
                    value={String(field.multipleOf ?? '')}
                    onChange={(e: { target: { value: string } }) =>
                      onChange({
                        ...field,
                        multipleOf:
                          e.target.value === ''
                            ? undefined
                            : Number(e.target.value),
                      })
                    }
                  />
                </PropBlock>
              </>
            ) : null}
          </PropAccordion>
        )}

        {isChoice ? (
          <PropAccordion
            title="Choices"
            open={openSections.choices}
            onToggle={() => toggle('choices')}
          >
            <div className="kit-svc-props__options">
              {options.map((option, index) => (
                <div key={option.id} className="kit-svc-props__option">
                  <InputText
                    unstyled
                    className={fieldInputClass}
                    value={option.label}
                    placeholder="Label"
                    onChange={(e: { target: { value: string } }) => {
                      const next = [...options]
                      next[index] = { ...next[index], label: e.target.value }
                      onChange({ ...field, options: next })
                    }}
                  />
                  <button
                    type="button"
                    className="kit-svc-props__option-remove"
                    aria-label="Remove option"
                    onClick={() =>
                      onChange({
                        ...field,
                        options: options.filter((_, i) => i !== index),
                      })
                    }
                  >
                    −
                  </button>
                </div>
              ))}
              <button
                type="button"
                className="kit-svc-props__add-option"
                onClick={() =>
                  onChange({
                    ...field,
                    options: [
                      ...options,
                      createSelectOption(`Item ${options.length + 1}`),
                    ],
                  })
                }
              >
                + Add choice
              </button>
            </div>
          </PropAccordion>
        ) : null}

        {isTable ? (
          <PropAccordion
            title="Columns"
            open={openSections.columns}
            onToggle={() => toggle('columns')}
          >
            <div className="kit-svc-props__options kit-df-table-cols">
              {columns.map((col, index) => (
                <div key={col.id} className="kit-df-table-cols__item">
                  <PropBlock label="Title">
                    <InputText
                      unstyled
                      className={fieldInputClass}
                      value={col.label}
                      onChange={(e: { target: { value: string } }) =>
                        patchColumn(index, { label: e.target.value })
                      }
                    />
                  </PropBlock>
                  <PropBlock label="Name">
                    <InputText
                      unstyled
                      className={fieldInputClass}
                      value={col.key}
                      spellCheck={false}
                      onChange={(e: { target: { value: string } }) =>
                        patchColumn(index, { key: e.target.value })
                      }
                    />
                  </PropBlock>
                  <PropBlock label="Type">
                    <select
                      className={fieldInputClass}
                      value={col.type}
                      onChange={(e) => {
                        const nextType = e.target.value
                        if (!isTableColumnType(nextType)) return
                        const rebuilt = createTableColumn(
                          nextType,
                          columns
                            .filter((_, i) => i !== index)
                            .map((c) => c.key),
                          { key: col.key, label: col.label },
                        )
                        patchColumn(index, {
                          ...rebuilt,
                          id: col.id,
                          key: col.key,
                          label: col.label,
                        })
                      }}
                    >
                      {TABLE_COLUMN_TYPES.map((type) => (
                        <option key={type} value={type}>
                          {FIELD_TYPE_META[type].label}
                        </option>
                      ))}
                    </select>
                  </PropBlock>
                  {col.type === 'select' ? (
                    <div className="kit-svc-props__options">
                      {normalizeSelectOptions(col.options).map((option, oi) => (
                        <div key={option.id} className="kit-svc-props__option">
                          <InputText
                            unstyled
                            className={fieldInputClass}
                            value={option.label}
                            placeholder="Choice"
                            onChange={(e: { target: { value: string } }) => {
                              const opts = [
                                ...normalizeSelectOptions(col.options),
                              ]
                              opts[oi] = {
                                ...opts[oi],
                                label: e.target.value,
                              }
                              patchColumn(index, { options: opts })
                            }}
                          />
                          <button
                            type="button"
                            className="kit-svc-props__option-remove"
                            aria-label="Remove choice"
                            onClick={() =>
                              patchColumn(index, {
                                options: normalizeSelectOptions(
                                  col.options,
                                ).filter((_, i) => i !== oi),
                              })
                            }
                          >
                            −
                          </button>
                        </div>
                      ))}
                      <button
                        type="button"
                        className="kit-svc-props__add-option"
                        onClick={() =>
                          patchColumn(index, {
                            options: [
                              ...normalizeSelectOptions(col.options),
                              createSelectOption(
                                `Option ${(col.options?.length ?? 0) + 1}`,
                              ),
                            ],
                          })
                        }
                      >
                        + Add choice
                      </button>
                    </div>
                  ) : null}
                  <div className="kit-df-table-cols__footer">
                    <label className="kit-svc-props__check">
                      <input
                        type="checkbox"
                        checked={Boolean(col.required)}
                        onChange={(e) =>
                          patchColumn(index, { required: e.target.checked })
                        }
                      />
                      <span>Required</span>
                    </label>
                    <button
                      type="button"
                      className="kit-df-table-cols__remove"
                      aria-label="Remove column"
                      onClick={() =>
                        onChange({
                          ...field,
                          children: columns.filter((_, i) => i !== index),
                        })
                      }
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ))}
              <button
                type="button"
                className="kit-svc-props__add-option"
                onClick={() =>
                  onChange({
                    ...field,
                    children: [
                      ...columns,
                      createTableColumn(
                        'text',
                        columns.map((c) => c.key),
                      ),
                    ],
                  })
                }
              >
                + Add column
              </button>
            </div>
          </PropAccordion>
        ) : null}

        {field.type !== 'group' && field.type !== 'table' ? (
        <PropAccordion
          title="Default"
          open={openSections.defaults}
          onToggle={() => toggle('defaults')}
        >
          {field.type === 'checkbox' ? (
            <label className="kit-svc-props__check">
              <input
                type="checkbox"
                checked={Boolean(field.defaultValue)}
                onChange={(e) =>
                  onChange({ ...field, defaultValue: e.target.checked })
                }
              />
              <span>Default checked</span>
            </label>
          ) : field.type !== 'radio' ? (
            <PropBlock label="Default value">
              <InputText
                unstyled
                type={
                  isNumber ? 'number' : field.type === 'date' ? 'date' : 'text'
                }
                className={fieldInputClass}
                value={String(field.defaultValue ?? '')}
                onChange={(e: { target: { value: string } }) =>
                  onChange({
                    ...field,
                    defaultValue: isNumber
                      ? e.target.value === ''
                        ? ''
                        : Number(e.target.value)
                      : e.target.value,
                  })
                }
              />
            </PropBlock>
          ) : (
            <p className="kit-svc-props__hint">No default for radio groups.</p>
          )}
        </PropAccordion>
        ) : null}
      </div>

      <div className="kit-svc-props__footer">
        <Button
          type="button"
          unstyled
          className={`${btnOutlinedClass} kit-svc-props__delete`}
          onClick={onDelete}
        >
          <Trash2 className="h-3.5 w-3.5" />
          Delete field
        </Button>
      </div>
    </aside>
  )
}
