import { Button } from 'primereact/button'
import { toolbarBtn, toolbarBtnPrimary } from './controlElements/styles'

export type QueryBuilderApiToolbarProps = {
  loading?: boolean
  disabled?: boolean
  onClear: () => void
  onApply: () => void
}

/** Toolbar Apply/Clear — chỉ gọi API 1 (`/employees`). API khác phát triển sau. */
export function QueryBuilderApiToolbar({
  loading,
  disabled,
  onClear,
  onApply,
}: QueryBuilderApiToolbarProps) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 bg-gradient-to-r from-slate-50 to-white px-4 py-3">
      <div className="min-w-0">
        <p className="m-0 text-sm font-semibold text-slate-900">
          Bộ lọc nhân viên
        </p>
        <p className="m-0 mt-0.5 text-xs text-slate-500">
          Gọi PagedListRequest tới /api/v1/company/employees
        </p>
      </div>
      <div className="flex flex-wrap items-center gap-2">
        <Button
          type="button"
          unstyled
          className={toolbarBtn}
          disabled={disabled || loading}
          onClick={onClear}
        >
          Xóa
        </Button>
        <Button
          type="button"
          unstyled
          className={toolbarBtnPrimary}
          disabled={disabled || loading}
          onClick={onApply}
        >
          {loading ? 'Đang tải…' : 'Áp dụng'}
        </Button>
      </div>
    </div>
  )
}
