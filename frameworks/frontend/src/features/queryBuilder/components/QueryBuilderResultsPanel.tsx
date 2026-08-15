function pickEmployeeCell(row: Record<string, unknown>, key: string): string {
  const camel = key.charAt(0).toLowerCase() + key.slice(1)
  const raw = row[key] ?? row[camel]
  if (raw == null) return '—'
  if (typeof raw === 'object') {
    const nested = raw as Record<string, unknown>
    if (key === 'Department' || key.toLowerCase() === 'department') {
      return String(nested.Name ?? nested.name ?? '—')
    }
    if (key === 'Position' || key.toLowerCase() === 'position') {
      return String(nested.Title ?? nested.title ?? '—')
    }
    return JSON.stringify(raw)
  }
  return String(raw)
}

export type QueryBuilderResultsPanelProps = {
  loading?: boolean
  total: number
  items: unknown[]
  error?: string | null
}

/** Simple results table for company employee list APIs. */
export function QueryBuilderResultsPanel({
  loading,
  total,
  items,
  error,
}: QueryBuilderResultsPanelProps) {
  const columns = [
    'FullName',
    'BaseSalary',
    'IsActive',
    'Department',
    'Position',
  ] as const

  return (
    <div className="border-t border-slate-100 px-4 py-3">
      <div className="mb-2 flex items-center justify-between gap-2">
        <p className="m-0 text-sm font-semibold text-slate-900">Kết quả</p>
        <p className="m-0 text-xs text-slate-500">
          {loading ? 'Đang tải…' : `${total} bản ghi`}
        </p>
      </div>
      {error ? (
        <p className="m-0 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </p>
      ) : null}
      <div className="overflow-x-auto rounded-xl border border-slate-200">
        <table className="w-full min-w-[40rem] border-collapse text-left text-sm">
          <thead className="bg-slate-50 text-xs font-semibold uppercase tracking-wide text-slate-500">
            <tr>
              {columns.map((col) => (
                <th key={col} className="px-3 py-2.5 font-semibold">
                  {col}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {!loading && items.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-3 py-6 text-center text-slate-500"
                >
                  Không có dữ liệu — bấm Áp dụng để gọi API
                </td>
              </tr>
            ) : (
              items.map((item, index) => {
                const row = (item ?? {}) as Record<string, unknown>
                const id = String(row.Id ?? row.id ?? index)
                return (
                  <tr
                    key={id}
                    className="border-t border-slate-100 text-slate-800"
                  >
                    {columns.map((col) => (
                      <td key={col} className="px-3 py-2.5">
                        {pickEmployeeCell(row, col)}
                      </td>
                    ))}
                  </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
