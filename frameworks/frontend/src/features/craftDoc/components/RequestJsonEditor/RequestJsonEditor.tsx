import { useMemo, useState } from 'react'
import { Textarea } from 'primereact/textarea'
import { notify } from '../../../../common/Toaster'
import type { CraftDocMessages } from '../../localization'
import {
  buildCraftDocSaveRequestJson,
  prettyCraftDocJson,
} from '../../utils/saveRequestJson'
import type { DocumentField, DocumentTemplate } from '../../types'

export type RequestJsonEditorProps = {
  template: DocumentTemplate | null
  fields: DocumentField[]
  messages: CraftDocMessages
}

type RequestJsonSubTab = 'request' | 'fields' | 'values'

export function RequestJsonEditor({
  template,
  fields,
  messages,
}: RequestJsonEditorProps) {
  const [subTab, setSubTab] = useState<RequestJsonSubTab>('request')
  const requestJson = useMemo(
    () => (template ? buildCraftDocSaveRequestJson(template, fields) : null),
    [template, fields],
  )

  const jsonValue = useMemo(() => {
    if (!requestJson) return ''
    if (subTab === 'fields') return prettyCraftDocJson(requestJson.fields)
    if (subTab === 'values') return prettyCraftDocJson(requestJson.sampleData)
    return prettyCraftDocJson(requestJson)
  }, [requestJson, subTab])

  const copyJson = async () => {
    try {
      await navigator.clipboard.writeText(jsonValue)
      notify.success(messages.preview.requestJson.copied)
    } catch {
      notify.error(messages.preview.requestJson.copyFailed)
    }
  }

  if (!template) {
    return (
      <div className="flex h-full items-center justify-center p-6">
        <p className="text-sm text-slate-400">{messages.preview.empty}</p>
      </div>
    )
  }

  return (
    <section className="craft-doc-json" aria-label={messages.preview.tabRequestJson}>
      <div className="craft-doc-json__bar">
        <div className="craft-doc-json__tabs" role="tablist">
          {(
            [
              ['request', messages.preview.requestJson.request],
              ['fields', messages.preview.requestJson.fields],
              ['values', messages.preview.requestJson.values],
            ] as const
          ).map(([id, label]) => (
            <button
              key={id}
              type="button"
              role="tab"
              aria-selected={subTab === id}
              className={['craft-doc-json__tab', subTab === id ? 'is-active' : '']
                .filter(Boolean)
                .join(' ')}
              onClick={() => setSubTab(id)}
            >
              {label}
            </button>
          ))}
        </div>
        <button type="button" className="craft-doc-json__copy" onClick={() => void copyJson()}>
          {messages.preview.requestJson.copy}
        </button>
      </div>
      <Textarea
        unstyled
        readOnly
        className="craft-doc-json__code"
        value={jsonValue}
        spellCheck={false}
      />
    </section>
  )
}
