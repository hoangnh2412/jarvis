import { useCallback, useEffect, useMemo, useState } from 'react'
import { ConfirmDialog, PageHeader, Tag, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Search, Trash2 } from 'lucide-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  deleteAuthorizationPolicy,
  listAuthorizationPolicies,
  type AuthorizationPolicyDetail,
} from '../services/policies'
import { identityQueryKeys } from '../services/users'
import { notifyApiError } from '../notify'
import { notify } from '@platform/core'
import { CreatePolicyLimitDialog } from './CreatePolicyLimitDialog'
import {
  canMutatePolicyCatalog,
  matchesPolicySearch,
  policyHumanSummary,
} from './policyLimitUi'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass } from '../components/fieldStyles'

/** Danh mục phạm vi truy cập (ABAC) — tạo/sửa/xóa trước khi gán vào vai trò. */
export function PolicyScopesPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editingPolicy, setEditingPolicy] = useState<AuthorizationPolicyDetail | null>(null)
  const [pendingDelete, setPendingDelete] = useState<AuthorizationPolicyDetail | null>(null)

  const policiesQuery = useQuery({
    queryKey: identityQueryKeys.authorizationPolicies,
    queryFn: listAuthorizationPolicies,
  })

  useEffect(() => {
    if (policiesQuery.isError) notifyApiError(policiesQuery.error)
  }, [policiesQuery.error, policiesQuery.isError])

  const policies = policiesQuery.data ?? []

  const filteredPolicies = useMemo(
    () => policies.filter((policy) => matchesPolicySearch(policy, search)),
    [policies, search],
  )

  const refreshPolicies = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: identityQueryKeys.authorizationPolicies })
    await queryClient.invalidateQueries({ queryKey: identityQueryKeys.policyLimits })
    await queryClient.invalidateQueries({ queryKey: identityQueryKeys.permissionCatalog })
  }, [queryClient])

  const deleteMutation = useMutation({
    mutationFn: (policyId: string) => deleteAuthorizationPolicy(policyId),
    onError: notifyApiError,
    onSuccess: async () => {
      await refreshPolicies()
      setPendingDelete(null)
      notify.success('Đã xóa phạm vi truy cập.')
    },
  })

  const closeForm = useCallback(() => {
    setCreateOpen(false)
    setEditingPolicy(null)
  }, [])

  const columns = useMemo<SimpleColumn<AuthorizationPolicyDetail>[]>(() => [
    { key: 'code', label: 'Mã phạm vi', className: 'font-mono font-semibold', value: (policy) => policy.code },
    { key: 'name', label: 'Tên hiển thị', value: (policy) => policy.name },
    {
      key: 'summary',
      label: 'Điều kiện',
      render: (policy) => (
        <span className="text-pretty text-slate-500">{policyHumanSummary(policy)}</span>
      ),
    },
    {
      key: 'flags',
      label: 'Loại',
      render: (policy) => (
        policy.isSystemPolicy
          ? <Tag severity="secondary">Hệ thống</Tag>
          : <span className="text-xs text-slate-500">Tùy chỉnh</span>
      ),
    },
  ], [])

  const actionsFor = useCallback((policy: AuthorizationPolicyDetail): RowActionItem[] => {
    if (!canMutatePolicyCatalog(policy)) return []
    return [
      { id: 'edit', label: 'Sửa', icon: Pencil, onClick: () => setEditingPolicy(policy) },
      { id: 'delete', label: 'Xóa', icon: Trash2, danger: true, separatorBefore: true, onClick: () => setPendingDelete(policy) },
    ]
  }, [])

  return (
    <section className="flex h-full min-h-0 flex-col gap-4 overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <PageHeader
        title="Phạm vi truy cập"
        description="Danh mục điều kiện ABAC gán cho vai trò."
        actions={(
          <>
            <label className="relative min-w-56 flex-1 sm:max-w-sm">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
              <input
                aria-label="Tìm phạm vi"
                value={search}
                onChange={(event) => setSearch(event.currentTarget.value)}
                placeholder="Tìm mã hoặc tên phạm vi…"
                className="h-10 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600"
              />
            </label>
            <button
              type="button"
              className={btnPrimaryClass}
              onClick={() => setCreateOpen(true)}
            >
              <Plus className="size-4" aria-hidden />
              Tạo phạm vi
            </button>
          </>
        )}
      />
      <SimpleTable
        items={filteredPolicies}
        columns={columns}
        loading={policiesQuery.isLoading}
        emptyMessage={policiesQuery.isLoading ? 'Đang tải danh mục phạm vi…' : 'Không tìm thấy phạm vi phù hợp.'}
        actionsFor={actionsFor}
      />
      {createOpen || editingPolicy !== null ? (
        <CreatePolicyLimitDialog
          open
          editPolicy={editingPolicy}
          onClose={closeForm}
        />
      ) : null}
      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={() => pendingDelete && deleteMutation.mutate(pendingDelete.id)}
        title="Xóa phạm vi truy cập?"
        description={pendingDelete ? `Phạm vi ${pendingDelete.name} (${pendingDelete.code}) sẽ bị xóa. Chỉ xóa được khi chưa gán cho bất kỳ vai trò.` : undefined}
      />
    </section>
  )
}
