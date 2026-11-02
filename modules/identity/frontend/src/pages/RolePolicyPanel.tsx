import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, SlidersHorizontal } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { identityQueryKeys } from '../services/users'
import { listAuthorizationPolicies } from '../services/policies'
import { matchesPolicySearch, policyLimitMeta } from './policyLimitUi'
import { btnAuxClass, codeBadgeClass, fieldInputClass, systemBadgeClass } from '../components/fieldStyles'

export type RolePolicyPanelProps = {
  enabled: boolean
  readOnly: boolean
  assignedCodes: string[]
  onAssignedCodesChange: (codes: string[]) => void
  contextHint?: string | null
  /** Đóng popup trước khi chuyển sang màn quản lý phạm vi. */
  onNavigateToCatalog?: () => void
  /** Đường dẫn màn quản lý phạm vi của host; không truyền thì ẩn nút chuyển màn. */
  policyCatalogPath?: string
}

export function RolePolicyPanel({
  enabled,
  readOnly,
  assignedCodes,
  onAssignedCodesChange,
  contextHint,
  onNavigateToCatalog,
  policyCatalogPath,
}: RolePolicyPanelProps) {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')

  const policiesQuery = useQuery({
    queryKey: identityQueryKeys.authorizationPolicies,
    queryFn: listAuthorizationPolicies,
    enabled,
  })

  const policies = policiesQuery.data ?? []
  const loading = policiesQuery.isLoading

  const filteredPolicies = useMemo(
    () => policies.filter((policy) => matchesPolicySearch(policy, search)),
    [policies, search],
  )

  const toggleCode = (code: string) => {
    if (readOnly) return
    onAssignedCodesChange(
      assignedCodes.includes(code)
        ? assignedCodes.filter((c) => c !== code)
        : [...assignedCodes, code],
    )
  }

  return (
    <div className="space-y-4">
      {readOnly ? (
        <p className="text-sm text-slate-500">Vai trò hệ thống không sửa phạm vi truy cập.</p>
      ) : (
        <p className="text-pretty text-sm text-slate-500">
          {contextHint ? <span className="font-medium text-slate-800">{contextHint}. </span> : null}
          Phạm vi áp dụng cho <strong>toàn vai trò</strong>; gán nhiều phạm vi thì hệ thống kết hợp <strong>tất cả</strong> (AND).
        </p>
      )}

      <div className="mt-2 flex flex-col gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <label className="relative min-w-[12rem] flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
            <input
              type="search"
              className={`${fieldInputClass} pl-10`}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              aria-label="Tìm mã hoặc tên phạm vi"
              placeholder="Tìm mã hoặc tên phạm vi…"
              disabled={loading}
            />
          </label>
          {!readOnly && policyCatalogPath ? (
            <button
              type="button"
              className={btnAuxClass}
              onClick={() => {
                onNavigateToCatalog?.()
                window.setTimeout(() => navigate(policyCatalogPath), 0)
              }}
            >
              <SlidersHorizontal className="size-4" aria-hidden />
              Quản lý phạm vi
            </button>
          ) : null}
        </div>

        {!readOnly && policies.length > 0 ? (
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs">
            <button
              type="button"
              className="font-semibold text-teal-700 hover:underline"
              onClick={() => onAssignedCodesChange(policies.map((p) => p.code))}
            >
              Chọn tất cả
            </button>
            <button
              type="button"
              className="font-semibold text-teal-700 hover:underline"
              onClick={() => onAssignedCodesChange([])}
            >
              Bỏ chọn tất cả
            </button>
            <span className="text-slate-500">
              {assignedCodes.length}/{policies.length} phạm vi
            </span>
          </div>
        ) : null}

        {loading ? (
          <p className="text-sm text-slate-500">Đang tải danh mục…</p>
        ) : filteredPolicies.length === 0 ? (
          <p className="rounded-xl border border-dashed border-slate-200 px-4 py-8 text-center text-sm text-slate-500">
            {search.trim() ? 'Không có phạm vi truy cập khớp từ khóa.' : 'Danh mục phạm vi truy cập trống.'}
          </p>
        ) : (
          <ul className="max-h-[min(50vh,480px)] space-y-1 overflow-auto rounded-xl border border-slate-200 p-2">
            {filteredPolicies.map((policy) => {
              const meta = policyLimitMeta(policy)
              const checked = assignedCodes.includes(policy.code)
              return (
                <li
                  key={policy.id}
                  className={`rounded-lg px-3 py-2.5 transition-colors ${checked ? 'bg-teal-50' : ''}`}
                >
                  <label className={`flex gap-3 ${readOnly ? 'cursor-default opacity-90' : 'cursor-pointer'}`}>
                    <input
                      type="checkbox"
                      className="mt-1 size-4 shrink-0 accent-teal-700"
                      checked={checked}
                      disabled={readOnly}
                      onChange={() => toggleCode(policy.code)}
                    />
                    <span className="min-w-0 flex-1">
                      <span className="flex flex-wrap items-baseline gap-x-2 gap-y-0.5">
                        <span className="font-medium text-slate-800">{meta.title}</span>
                        <span className={codeBadgeClass}>{policy.code}</span>
                        {policy.isSystemPolicy ? (
                          <span className={systemBadgeClass}>Hệ thống</span>
                        ) : null}
                      </span>
                      <span className="mt-0.5 block text-pretty text-sm text-slate-500">{meta.summary}</span>
                      {meta.detail ? (
                        <span className="mt-0.5 block text-xs text-slate-500">{meta.detail}</span>
                      ) : null}
                    </span>
                  </label>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
