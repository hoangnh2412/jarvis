import { DataTable } from 'primereact/datatable'
import type { ImportInvalidRow, ImportValidRow } from '../../types'
import type { ImportPageMessages } from '../../localization'

export type ImportPreviewTableProps = {
  variant: 'valid' | 'invalid'
  validRows?: ImportValidRow[]
  invalidRows?: ImportInvalidRow[]
  messages: ImportPageMessages
  className?: string
}

const headCellClass =
  'px-4 py-2.5 text-left text-[11px] font-semibold uppercase tracking-[0.06em] text-slate-500'

const bodyCellClass = 'px-4 py-2.5 text-sm text-slate-800'

export function ImportPreviewTable({
  variant,
  validRows = [],
  invalidRows = [],
  messages,
  className = '',
}: ImportPreviewTableProps) {
  if (variant === 'valid') {
    return (
      <div className={`kit-import-table overflow-hidden rounded-lg border border-line ${className}`}>
        <DataTable.Root data={validRows} dataKey="row">
          <DataTable.TableContainer>
            <DataTable.Table className="w-full border-collapse text-left">
              <DataTable.THead>
                <DataTable.THeadRow className="border-b border-line bg-slate-50/90">
                  <DataTable.THeadCell className={headCellClass}>
                    {messages.colIndex}
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className={headCellClass}>
                    {messages.colAge}
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className={headCellClass}>
                    {messages.colName}
                  </DataTable.THeadCell>
                </DataTable.THeadRow>
              </DataTable.THead>
              <DataTable.TBody>
                {({ item }) => {
                  const row = item as ImportValidRow
                  return (
                    <DataTable.Row className="border-b border-slate-100 last:border-b-0 hover:bg-slate-50/60">
                      <DataTable.Cell className={bodyCellClass}>{row.row}</DataTable.Cell>
                      <DataTable.Cell className={bodyCellClass}>{row.age}</DataTable.Cell>
                      <DataTable.Cell className={bodyCellClass}>{row.name}</DataTable.Cell>
                    </DataTable.Row>
                  )
                }}
              </DataTable.TBody>
            </DataTable.Table>
          </DataTable.TableContainer>
        </DataTable.Root>
      </div>
    )
  }

  if (invalidRows.length === 0) {
    return (
      <p className="kit-import-empty m-0 rounded-lg border border-dashed border-slate-200 bg-slate-50/70 px-4 py-6 text-center text-sm text-slate-500">
        {messages.noInvalidRows}
      </p>
    )
  }

  return (
    <div className={`kit-import-table overflow-hidden rounded-lg border border-line ${className}`}>
      <DataTable.Root data={invalidRows} dataKey="row">
        <DataTable.TableContainer>
          <DataTable.Table className="w-full border-collapse text-left">
            <DataTable.THead>
              <DataTable.THeadRow className="border-b border-line bg-slate-50/90">
                <DataTable.THeadCell className={headCellClass}>
                  {messages.colIndex}
                </DataTable.THeadCell>
                <DataTable.THeadCell className={headCellClass}>
                  {messages.colAge}
                </DataTable.THeadCell>
                <DataTable.THeadCell className={headCellClass}>
                  {messages.colName}
                </DataTable.THeadCell>
                <DataTable.THeadCell className={headCellClass}>
                  {messages.colErrors}
                </DataTable.THeadCell>
              </DataTable.THeadRow>
            </DataTable.THead>
            <DataTable.TBody>
              {({ item }) => {
                const row = item as ImportInvalidRow
                return (
                  <DataTable.Row className="border-b border-slate-100 last:border-b-0 hover:bg-red-50/30">
                    <DataTable.Cell className={bodyCellClass}>{row.row}</DataTable.Cell>
                    <DataTable.Cell className={bodyCellClass}>{row.age ?? '—'}</DataTable.Cell>
                    <DataTable.Cell className={bodyCellClass}>{row.name ?? '—'}</DataTable.Cell>
                    <DataTable.Cell className={`${bodyCellClass} text-red-600`}>
                      {row.errors.join('; ')}
                    </DataTable.Cell>
                  </DataTable.Row>
                )
              }}
            </DataTable.TBody>
          </DataTable.Table>
        </DataTable.TableContainer>
      </DataTable.Root>
    </div>
  )
}
