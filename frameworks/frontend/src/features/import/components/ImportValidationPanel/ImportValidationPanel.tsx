import type { ReactNode } from 'react'
import { Card } from 'primereact/card'
import { Message } from 'primereact/message'
import type { ImportValidationResult } from '../../types'
import type { ImportPageMessages } from '../../localization'
import {
  sectionHeaderInvalidClass,
  sectionHeaderValidClass,
  statBadgeDangerClass,
  statBadgeNeutralClass,
  statBadgeSuccessClass,
} from '../fieldStyles'
import { ImportPreviewTable } from '../ImportPreviewTable'

export type ImportValidationPanelProps = {
  result: ImportValidationResult
  messages: ImportPageMessages
  className?: string
  footer?: ReactNode
}

export function ImportValidationPanel({
  result,
  messages,
  className = '',
  footer,
}: ImportValidationPanelProps) {
  const severity = result.invalidCount > 0 ? 'warn' : 'success'

  return (
    <Card.Root
      className={`kit-import-result overflow-hidden rounded-xl border border-line bg-white shadow-sm ${className}`}
    >
      <Card.Body className="p-0">
        <div className="border-b border-line px-5 py-4 sm:px-6">
          <h3 className="m-0 text-base font-semibold text-slate-900">
            {messages.resultTitle}
          </h3>
          <Message.Root severity={severity} className="mt-3 border-0 bg-transparent p-0">
            <Message.Content>
              <Message.Text className="text-sm text-slate-700">
                {result.message}
              </Message.Text>
            </Message.Content>
          </Message.Root>

          <div className="mt-4 flex flex-wrap items-center gap-2">
            <span className={statBadgeNeutralClass}>
              {messages.processedLabel}: {result.processed}
            </span>
            <span className={statBadgeSuccessClass}>
              {messages.validLabel}: {result.validCount}
            </span>
            <span className={statBadgeDangerClass}>
              {messages.invalidLabel}: {result.invalidCount}
            </span>
          </div>
        </div>

        <div className="kit-import-sheet-bar flex flex-wrap items-center justify-between gap-2 border-b border-line bg-slate-50/70 px-5 py-2.5 text-sm text-slate-600 sm:px-6">
          <span className="font-medium text-slate-700">
            {messages.sheetPrefix}: {result.sheetName}
          </span>
          <span className="text-slate-500">
            {result.validCount} {messages.validShort} · {result.invalidCount}{' '}
            {messages.errorShort}
          </span>
        </div>

        <div className="space-y-0">
          {result.validCount > 0 && (
            <section className="kit-import-section">
              <div className={sectionHeaderValidClass}>
                {messages.validSectionTitle} ({result.validCount})
              </div>
              <div className="px-5 py-4 sm:px-6">
                <ImportPreviewTable
                  variant="valid"
                  validRows={result.validRows}
                  messages={messages}
                />
              </div>
            </section>
          )}

          {result.invalidCount > 0 && (
            <section className="kit-import-section border-t border-line">
              <div className={sectionHeaderInvalidClass}>
                {messages.invalidSectionTitle} ({result.invalidCount})
              </div>
              <div className="px-5 py-4 sm:px-6">
                <ImportPreviewTable
                  variant="invalid"
                  invalidRows={result.invalidRows}
                  messages={messages}
                />
              </div>
            </section>
          )}
        </div>

        {footer ? (
          <div className="flex flex-wrap items-center justify-end gap-2.5 border-t border-line px-5 py-4 sm:px-6">
            {footer}
          </div>
        ) : null}
      </Card.Body>
    </Card.Root>
  )
}
