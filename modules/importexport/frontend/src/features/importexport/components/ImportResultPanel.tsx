import { Card, Message, Tag } from '@platform/core'
import type { ImportErrorSummary, ImportResult } from '../types'

export type ImportResultPanelProps = {
  result: ImportResult
  message?: string
  /** validate = sau bước validate; import = sau import thực tế */
  mode?: 'validate' | 'import'
}

type RowErrorDetail = {
  rowNumber: number
  issues: Array<{
    message: string
    columnName?: string | null
    fieldName?: string | null
    errorCode?: string | null
  }>
}

function groupErrorsByRow(errorSummary: ImportErrorSummary[]): RowErrorDetail[] {
  const map = new Map<number, RowErrorDetail['issues']>()

  for (const err of errorSummary) {
    for (const rowNumber of err.rowNumbers ?? []) {
      const list = map.get(rowNumber) ?? []
      list.push({
        message: err.message,
        columnName: err.columnName,
        fieldName: err.fieldName,
        errorCode: err.errorCode,
      })
      map.set(rowNumber, list)
    }
  }

  return [...map.entries()]
    .sort(([a], [b]) => a - b)
    .map(([rowNumber, issues]) => ({ rowNumber, issues }))
}

function collectColumnKeys(rows: Record<string, unknown>[]): string[] {
  const keys = new Set<string>()
  for (const row of rows) {
    for (const key of Object.keys(row)) keys.add(key)
  }
  return [...keys]
}

function formatCell(value: unknown): string {
  if (value == null) return '—'
  if (typeof value === 'object') return JSON.stringify(value)
  return String(value)
}

export function ImportResultPanel({
  result,
  message,
  mode = 'import',
}: ImportResultPanelProps) {
  const hasFailures = result.failedRowCount > 0
  const title = mode === 'validate' ? 'Kết quả validate' : 'Kết quả import'

  return (
    <Card.Root className="overflow-hidden rounded-xl border border-line bg-white shadow-sm">
      <Card.Header className="border-b border-line px-5 py-4">
        <Card.Title className="m-0 text-sm font-semibold text-ink">
          {title}
        </Card.Title>
        {message ? (
          <Card.Subtitle className="mt-1 m-0 text-sm text-mute">
            {message}
          </Card.Subtitle>
        ) : null}
      </Card.Header>

      <Card.Body className="space-y-5 p-5">
        <div className="flex flex-wrap items-center gap-2">
          <Tag
            severity="secondary"
            rounded
            className="inline-flex items-center rounded-lg bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700"
          >
            Đã xử lý: {result.processedRowCount}
          </Tag>
          <Tag
            severity="success"
            rounded
            className="inline-flex items-center rounded-lg bg-emerald-50 px-2.5 py-1 text-xs font-medium text-emerald-700"
          >
            Hợp lệ: {result.sheets.reduce((n, s) => n + s.validRowCount, 0)}
          </Tag>
          <Tag
            severity={hasFailures ? 'danger' : 'secondary'}
            rounded
            className={[
              'inline-flex items-center rounded-lg px-2.5 py-1 text-xs font-medium',
              hasFailures
                ? 'bg-red-50 text-red-700'
                : 'bg-slate-100 text-slate-600',
            ].join(' ')}
          >
            Không hợp lệ: {result.failedRowCount}
          </Tag>
          {mode === 'import' ? (
            <Tag
              severity="success"
              rounded
              className="inline-flex items-center rounded-lg bg-teal-50 px-2.5 py-1 text-xs font-medium text-teal-800"
            >
              Đã lưu: {result.savedCount}
            </Tag>
          ) : null}
        </div>

        {result.sheets.map((sheet) => {
          const invalidRows = groupErrorsByRow(sheet.errorSummary ?? [])
          const validRows = sheet.rows ?? []
          const columns = collectColumnKeys(validRows)

          return (
            <Card.Root
              key={sheet.sheetName}
              className="overflow-hidden rounded-lg border border-line bg-slate-50/60"
            >
              <Card.Header className="flex flex-wrap items-baseline justify-between gap-2 border-b border-line/60 px-4 py-3">
                <Card.Title className="m-0 text-sm font-medium text-ink">
                  Sheet: {sheet.sheetName}
                </Card.Title>
                <span className="text-xs text-mute">
                  hợp lệ {sheet.validRowCount} · lỗi {sheet.failedRowCount}
                </span>
              </Card.Header>

              <Card.Body className="space-y-4 p-4">
                <section>
                  <h4 className="m-0 mb-2 text-xs font-semibold uppercase tracking-wide text-emerald-800">
                    Dữ liệu hợp lệ ({sheet.validRowCount})
                  </h4>
                  {sheet.rowsOmitted ? (
                    <p className="m-0 text-sm text-mute">
                      Chi tiết dòng hợp lệ không được trả về (file quá lớn).
                      Số lượng hợp lệ: {sheet.validRowCount}.
                    </p>
                  ) : validRows.length > 0 ? (
                    <div className="overflow-x-auto rounded-lg border border-line bg-white">
                      <table className="w-full min-w-[480px] border-collapse text-left text-sm">
                        <thead>
                          <tr className="border-b border-line bg-slate-50 text-xs font-semibold uppercase tracking-wide text-mute">
                            <th className="px-3 py-2">#</th>
                            {columns.map((col) => (
                              <th key={col} className="px-3 py-2">
                                {col}
                              </th>
                            ))}
                          </tr>
                        </thead>
                        <tbody>
                          {validRows.map((row, index) => (
                            <tr
                              key={index}
                              className="border-b border-line/60 last:border-b-0"
                            >
                              <td className="px-3 py-2 text-mute">{index + 1}</td>
                              {columns.map((col) => (
                                <td key={col} className="px-3 py-2 text-ink">
                                  {formatCell(row[col])}
                                </td>
                              ))}
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ) : (
                    <Message.Root
                      severity="secondary"
                      className="border-0 bg-transparent p-0"
                    >
                      <Message.Content>
                        <Message.Text className="text-sm text-mute">
                          Không có dòng hợp lệ trên sheet này.
                        </Message.Text>
                      </Message.Content>
                    </Message.Root>
                  )}
                </section>

                <section>
                  <h4 className="m-0 mb-2 text-xs font-semibold uppercase tracking-wide text-red-700">
                    Dữ liệu không hợp lệ ({sheet.failedRowCount})
                  </h4>
                  {invalidRows.length > 0 ? (
                    <div className="overflow-x-auto rounded-lg border border-red-100 bg-white">
                      <table className="w-full min-w-[480px] border-collapse text-left text-sm">
                        <thead>
                          <tr className="border-b border-line bg-red-50/80 text-xs font-semibold uppercase tracking-wide text-red-800">
                            <th className="px-3 py-2">Excel row</th>
                            <th className="px-3 py-2">Cột / Field</th>
                            <th className="px-3 py-2">Lỗi</th>
                          </tr>
                        </thead>
                        <tbody>
                          {invalidRows.map(({ rowNumber, issues }) =>
                            issues.map((issue, issueIndex) => (
                              <tr
                                key={`${rowNumber}-${issueIndex}`}
                                className="border-b border-line/60 last:border-b-0"
                              >
                                {issueIndex === 0 ? (
                                  <td
                                    className="px-3 py-2 align-top font-medium text-ink"
                                    rowSpan={issues.length}
                                  >
                                    {rowNumber}
                                  </td>
                                ) : null}
                                <td className="px-3 py-2 align-top text-mute">
                                  {issue.columnName ?? '—'}
                                  {issue.fieldName
                                    ? ` → ${issue.fieldName}`
                                    : ''}
                                </td>
                                <td className="px-3 py-2 align-top text-red-700">
                                  {issue.message}
                                  {issue.errorCode ? (
                                    <span className="ml-1 text-xs text-red-500">
                                      ({issue.errorCode})
                                    </span>
                                  ) : null}
                                </td>
                              </tr>
                            )),
                          )}
                        </tbody>
                      </table>
                    </div>
                  ) : (
                    <Message.Root
                      severity="success"
                      className="border-0 bg-transparent p-0"
                    >
                      <Message.Content>
                        <Message.Text className="text-sm text-emerald-700">
                          Không có dòng lỗi trên sheet này.
                        </Message.Text>
                      </Message.Content>
                    </Message.Root>
                  )}
                </section>
              </Card.Body>
            </Card.Root>
          )
        })}
      </Card.Body>
    </Card.Root>
  )
}
