import { useCallback, useEffect, useMemo, useState } from 'react'
import { AdminList, ConfirmDialog, Tag, type AdminListColumn, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Trash2 } from 'lucide-react'
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
    retry: false,
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

  const columns = useMemo<AdminListColumn<AuthorizationPolicyDetail>[]>(() => [
    { key: 'code', label: 'Mã phạm vi', sortable: true, sortValue: (policy) => policy.code, className: 'font-mono font-semibold', value: (policy) => policy.code },
    { key: 'name', label: 'Tên hiển thị', sortable: true, sortValue: (policy) => policy.name, value: (policy) => policy.name },
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
    <section className="flex h-full min-h-0 flex-col overflow-hidden p-4 text-sm text-slate-800 sm:p-6">
      <AdminList
        items={filteredPolicies}
        columns={columns}
        loading={policiesQuery.isLoading}
        searchValue={search}
        onSearchChange={setSearch}
        searchLabel="Tìm phạm vi"
        searchPlaceholder="Tìm mã hoặc tên phạm vi…"
        emptyMessage={
          policiesQuery.isError
            ? 'Không tải được danh mục phạm vi.'
            : policiesQuery.isLoading
              ? 'Đang tải danh mục phạm vi…'
              : 'Không tìm thấy phạm vi phù hợp.'
        }
        actionsFor={actionsFor}
        primaryAction={(
          <button type="button" className={btnPrimaryClass} onClick={() => setCreateOpen(true)}>
            <Plus className="size-4" aria-hidden />
            Tạo phạm vi
          </button>
        )}
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
