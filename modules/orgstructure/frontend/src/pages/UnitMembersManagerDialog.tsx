import { useCallback, useMemo, useState, type Dispatch, type SetStateAction } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FeatureDialog, Tag, notify, type RowActionItem } from '@platform/core'
import { Briefcase, Pencil, Plus, UserMinus, UserPlus } from 'lucide-react'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnAuxClass, btnPrimaryClass, btnSecondaryClass, fieldInputClass } from '../components/fieldStyles'
import { notifyApiError } from '../notify'
import { catalogSearchMatches } from '../utils'
import {
  createOrgUnitMember,
  listJobGrades,
  listJobPositions,
  listJobTitles,
  listOrgUnitMembers,
  orgStructureQueryKeys,
  setOrgUnitMemberActive,
  updateOrgUnitMember,
  type OrgUnit,
  type OrgUnitMember,
} from '../services/orgStructure'

/** Người dùng host cung cấp để gán vào đơn vị (package không tự gọi API người dùng). */
export type OrgStructureAssignableUser = {
  id: string
  userName: string
  fullName: string
  email: string
  isActive: boolean
}

type MemberDraft = {
  userId: string
  jobPositionId: string
  jobGradeId: string
  jobTitleId: string
}

function emptyDraft(
  positions: { id: string }[],
  grades: { id: string }[],
): MemberDraft {
  return {
    userId: '',
    jobPositionId: positions[0]?.id ?? '',
    jobGradeId: grades[0]?.id ?? '',
    jobTitleId: '',
  }
}

function memberLabel(member: OrgUnitMember): string {
  return member.displayName?.trim() || member.userName
}

function userDisplayLabel(user: OrgStructureAssignableUser): string {
  const name = user.fullName?.trim() || user.userName
  return `${name} (${user.userName})`
}

function memberInitials(label: string): string {
  const parts = label.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return '?'
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase()
  return `${parts[0][0] ?? ''}${parts[parts.length - 1][0] ?? ''}`.toUpperCase()
}

function MemberAssignmentFields({
  draft,
  setDraft,
  positions,
  grades,
  titles,
}: {
  draft: MemberDraft
  setDraft: Dispatch<SetStateAction<MemberDraft>>
  positions: { id: string; name: string }[]
  grades: { id: string; name: string }[]
  titles: { id: string; name: string }[]
}) {
  return (
    <section
      className="space-y-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
      aria-labelledby="member-assignment-heading"
    >
      <div className="flex items-start gap-3 border-b border-slate-200 pb-3">
        <span
          className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-teal-700/10 text-teal-700"
          aria-hidden
        >
          <Briefcase className="size-4" />
        </span>
        <div className="min-w-0 flex-1">
          <h3 id="member-assignment-heading" className="m-0 text-sm font-semibold text-slate-900">
            Phân công vị trí
          </h3>
          <p className="mb-0 mt-1 text-xs leading-relaxed text-slate-500">
            Gán <span className="font-medium text-slate-800">Position</span> (vai trò công việc),{' '}
            <span className="font-medium text-slate-800">Job grade</span> (cấp chuyên môn) và{' '}
            <span className="font-medium text-slate-800">Job title</span> (chức vụ quản lý, tùy chọn).
          </p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <label className="grid gap-1.5">
          <span className="text-sm font-medium text-slate-800">
            Position <span className="text-red-600" aria-hidden>*</span>
          </span>
          <span className="text-xs text-slate-500">Vị trí công việc trong đơn vị</span>
          <select
            className={fieldInputClass}
            value={draft.jobPositionId}
            onChange={(event) => setDraft((current) => ({ ...current, jobPositionId: event.target.value }))}
          >
            {positions.map((item) => (
              <option key={item.id} value={item.id}>{item.name}</option>
            ))}
          </select>
        </label>
        <label className="grid gap-1.5">
          <span className="text-sm font-medium text-slate-800">
            Job grade <span className="text-red-600" aria-hidden>*</span>
          </span>
          <span className="text-xs text-slate-500">Cấp bậc chuyên môn</span>
          <select
            className={fieldInputClass}
            value={draft.jobGradeId}
            onChange={(event) => setDraft((current) => ({ ...current, jobGradeId: event.target.value }))}
          >
            {grades.map((item) => (
              <option key={item.id} value={item.id}>{item.name}</option>
            ))}
          </select>
        </label>
      </div>

      <label className="grid gap-1.5">
        <span className="flex flex-wrap items-center gap-2 text-sm font-medium text-slate-800">
          Job title
          <span className="rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-medium uppercase tracking-wide text-slate-500">
            Tùy chọn
          </span>
        </span>
        <span className="text-xs text-slate-500">Chức vụ quản lý; để trống nếu là nhân viên thường</span>
        <select
          className={fieldInputClass}
          value={draft.jobTitleId}
          onChange={(event) => setDraft((current) => ({ ...current, jobTitleId: event.target.value }))}
        >
          <option value="">Không có / nhân viên thường</option>
          {titles.map((item) => (
            <option key={item.id} value={item.id}>{item.name}</option>
          ))}
        </select>
      </label>
    </section>
  )
}

export type UnitMembersManagerDialogProps = {
  unit: OrgUnit
  onClose: () => void
  /** Host quyết định quyền quản lý (mặc định cho phép; host bọc kiểm quyền bên ngoài nếu cần). */
  canManage?: boolean
  /** Host nạp danh sách người dùng để gán vào đơn vị; không truyền thì không thêm được thành viên mới. */
  loadUsers?: () => Promise<OrgStructureAssignableUser[]>
}

export function UnitMembersManagerDialog({
  unit,
  onClose,
  canManage = true,
  loadUsers,
}: UnitMembersManagerDialogProps) {
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<OrgUnitMember | null>(null)
  const [selectedUserIds, setSelectedUserIds] = useState<Set<string>>(() => new Set())
  const [userSearch, setUserSearch] = useState('')
  const [draft, setDraft] = useState<MemberDraft>({
    userId: '',
    jobPositionId: '',
    jobGradeId: '',
    jobTitleId: '',
  })

  const membersQuery = useQuery({
    queryKey: orgStructureQueryKeys.unitMembers(unit.id),
    queryFn: () => listOrgUnitMembers(unit.id),
  })
  const positionsQuery = useQuery({
    queryKey: orgStructureQueryKeys.jobPositions,
    queryFn: listJobPositions,
  })
  const gradesQuery = useQuery({
    queryKey: orgStructureQueryKeys.jobGrades,
    queryFn: listJobGrades,
  })
  const titlesQuery = useQuery({
    queryKey: orgStructureQueryKeys.jobTitles,
    queryFn: listJobTitles,
  })
  const usersQuery = useQuery({
    queryKey: ['org-structure', 'assignable-users'] as const,
    queryFn: () => loadUsers!(),
    enabled: Boolean(loadUsers),
  })

  const positions = positionsQuery.data ?? []
  const grades = gradesQuery.data ?? []
  const titles = titlesQuery.data ?? []
  const members = membersQuery.data ?? []

  const refresh = useCallback(async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: orgStructureQueryKeys.unitMembers(unit.id) }),
      queryClient.invalidateQueries({ queryKey: orgStructureQueryKeys.unitMemberSummary }),
    ])
  }, [queryClient, unit.id])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const body = {
        jobPositionId: draft.jobPositionId,
        jobGradeId: draft.jobGradeId,
        jobTitleId: draft.jobTitleId || null,
      }
      if (editing?.id) return updateOrgUnitMember(editing.id, body)
      const userIds = [...selectedUserIds]
      if (userIds.length === 0) throw new Error('Chọn ít nhất một người dùng.')
      for (const userId of userIds) {
        await createOrgUnitMember(unit.id, { userId, ...body })
      }
      return userIds.length
    },
    onError: (error) => {
      if (error instanceof Error && error.message === 'Chọn ít nhất một người dùng.') {
        notify.error(error.message)
        return
      }
      notifyApiError(error)
    },
    onSuccess: async (result) => {
      const wasEdit = Boolean(editing)
      await refresh()
      setFormOpen(false)
      setEditing(null)
      setSelectedUserIds(new Set())
      setUserSearch('')
      if (wasEdit) {
        notify.success('Đã lưu thành viên.')
      } else if (typeof result === 'number' && result > 1) {
        notify.success(`Đã thêm ${result} thành viên.`)
      } else {
        notify.success('Đã thêm thành viên.')
      }
    },
  })

  const activeMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      setOrgUnitMemberActive(id, isActive),
    onError: notifyApiError,
    onSuccess: async (_, { isActive }) => {
      await refresh()
      notify.success(isActive ? 'Đã bật lại thành viên.' : 'Đã gỡ thành viên khỏi đơn vị.')
    },
  })

  const assignedUserIds = useMemo(() => new Set(members.map((m) => m.userId)), [members])
  const userOptions = useMemo(() => {
    const rows = usersQuery.data ?? []
    return rows
      .filter((user) => user.isActive && !assignedUserIds.has(user.id))
      .sort((left, right) => userDisplayLabel(left).localeCompare(userDisplayLabel(right), 'vi'))
  }, [assignedUserIds, usersQuery.data])

  const filteredUserOptions = useMemo(
    () => userOptions.filter((user) => catalogSearchMatches(
      userSearch,
      user.userName,
      user.fullName,
      user.email,
    )),
    [userOptions, userSearch],
  )

  const openCreate = () => {
    setEditing(null)
    setSelectedUserIds(new Set())
    setUserSearch('')
    setDraft(emptyDraft(positions, grades))
    setFormOpen(true)
  }

  const toggleUser = (userId: string, checked: boolean) => {
    setSelectedUserIds((current) => {
      const next = new Set(current)
      if (checked) next.add(userId)
      else next.delete(userId)
      return next
    })
  }

  const selectAllFiltered = () => {
    setSelectedUserIds((current) => {
      const next = new Set(current)
      for (const user of filteredUserOptions) next.add(user.id)
      return next
    })
  }

  const clearSelection = () => setSelectedUserIds(new Set())

  const openEdit = (row: OrgUnitMember) => {
    setEditing(row)
    setDraft({
      userId: row.userId,
      jobPositionId: row.jobPositionId,
      jobGradeId: row.jobGradeId,
      jobTitleId: row.jobTitleId ?? '',
    })
    setFormOpen(true)
  }

  const closeForm = () => {
    setFormOpen(false)
    setEditing(null)
    setSelectedUserIds(new Set())
    setUserSearch('')
  }

  const loadingCatalog =
    positionsQuery.isLoading || gradesQuery.isLoading || titlesQuery.isLoading

  const columns = useMemo<SimpleColumn<OrgUnitMember>[]>(() => [
    {
      key: 'user',
      label: 'Người dùng',
      render: (row) => (
        <>
          <div className="font-medium">{memberLabel(row)}</div>
          <div className="text-xs text-slate-500">{row.userName}</div>
        </>
      ),
    },
    { key: 'position', label: 'Position', value: (row) => row.jobPositionName },
    { key: 'grade', label: 'Job grade', value: (row) => row.jobGradeName },
    { key: 'title', label: 'Job title', className: 'text-slate-500', value: (row) => row.jobTitleName ?? '—' },
    {
      key: 'isActive',
      label: 'Trạng thái',
      render: (row) => row.isActive
        ? <Tag severity="success">Đang gắn</Tag>
        : <Tag severity="secondary">Đã gỡ</Tag>,
    },
  ], [])

  const actionsFor = useCallback((row: OrgUnitMember): RowActionItem[] => [
    { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => openEdit(row) },
    row.isActive
      ? {
          id: 'remove',
          label: 'Gỡ khỏi đơn vị',
          icon: UserMinus,
          danger: true,
          separatorBefore: true,
          onClick: () => activeMutation.mutate({ id: row.id, isActive: false }),
        }
      : {
          id: 'restore',
          label: 'Gắn lại',
          icon: UserPlus,
          separatorBefore: true,
          onClick: () => activeMutation.mutate({ id: row.id, isActive: true }),
        },
    // eslint-disable-next-line react-hooks/exhaustive-deps
  ], [activeMutation])

  return (
    <>
      <FeatureDialog
        open
        onClose={onClose}
        title="Thành viên đơn vị"
        description={`${unit.name} — Position (vị trí CV), Job grade (cấp chuyên môn), Job title (chức vụ, tùy chọn).`}
        size="2xl"
        footer={(
          <>
            {canManage ? (
              <button
                type="button"
                className={btnPrimaryClass}
                onClick={openCreate}
                disabled={loadingCatalog || positions.length === 0 || grades.length === 0}
              >
                <Plus className="size-4" aria-hidden />
                Thêm thành viên
              </button>
            ) : null}
            <button type="button" className={btnSecondaryClass} onClick={onClose}>Đóng</button>
          </>
        )}
      >
        {membersQuery.isError ? (
          <p className="text-sm text-red-600">Không tải được danh sách thành viên.</p>
        ) : (
          <SimpleTable
            items={members}
            columns={columns}
            loading={membersQuery.isLoading}
            emptyMessage={membersQuery.isLoading ? 'Đang tải…' : canManage ? 'Chưa có thành viên. Bấm Thêm thành viên để gán user vào đơn vị này.' : 'Chưa có thành viên.'}
            actionsFor={canManage ? actionsFor : undefined}
          />
        )}
      </FeatureDialog>

      {formOpen ? (
        <FeatureDialog
          open
          onClose={() => {
            if (saveMutation.isPending) return
            closeForm()
          }}
          title={editing ? 'Sửa thành viên' : 'Thêm thành viên'}
          description={
            editing
              ? `Cập nhật phân công cho thành viên trong ${unit.name}.`
              : 'Chọn một hoặc nhiều người chưa thuộc đơn vị; gán Position, Job grade, Job title chung rồi lưu một lần.'
          }
          size={editing ? 'lg' : '2xl'}
          onSubmit={(event) => {
            event.preventDefault()
            saveMutation.mutate()
          }}
          submitting={saveMutation.isPending}
          submitLabel={editing ? 'Lưu' : `Lưu${selectedUserIds.size > 0 ? ` (${selectedUserIds.size})` : ''}`}
          cancelLabel="Hủy"
        >
          <div className="grid gap-4">
            {editing ? (
              <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-slate-50 px-4 py-3.5">
                <div
                  className="flex size-12 shrink-0 items-center justify-center rounded-full border border-teal-600/20 bg-white text-sm font-semibold text-teal-700 shadow-sm"
                  aria-hidden
                >
                  {memberInitials(memberLabel(editing))}
                </div>
                <div className="min-w-0">
                  <p className="m-0 text-[11px] font-medium uppercase tracking-wider text-slate-500">
                    Thành viên
                  </p>
                  <p className="m-0 truncate text-base font-semibold text-slate-900">
                    {memberLabel(editing)}
                  </p>
                  <p className="m-0 mt-0.5 truncate text-xs text-slate-500">
                    @{editing.userName}
                  </p>
                </div>
              </div>
            ) : !loadUsers ? (
              <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-6 text-center text-sm text-slate-500">
                Host chưa cung cấp danh sách người dùng cho package, nên chưa chọn được người để gán vào đơn vị.
              </p>
            ) : (
              <section className="space-y-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
                <div>
                  <h3 className="m-0 text-sm font-semibold text-slate-900">Chọn người dùng</h3>
                  <p className="mb-0 mt-1 text-xs text-slate-500">
                    Chỉ hiển thị tài khoản đang hoạt động và chưa thuộc đơn vị này.
                  </p>
                </div>
                <label className="grid gap-1.5 text-sm">
                  <span className="font-medium text-slate-800">Tìm kiếm</span>
                  <input
                    className={fieldInputClass}
                    value={userSearch}
                    placeholder="Tên, username, email…"
                    onChange={(event) => setUserSearch(event.target.value)}
                  />
                </label>
                <div className="flex flex-wrap items-center gap-2 text-sm">
                  <button
                    type="button"
                    className={btnAuxClass}
                    disabled={filteredUserOptions.length === 0}
                    onClick={selectAllFiltered}
                  >
                    Chọn tất cả ({filteredUserOptions.length})
                  </button>
                  <button
                    type="button"
                    className={btnAuxClass}
                    disabled={selectedUserIds.size === 0}
                    onClick={clearSelection}
                  >
                    Bỏ chọn
                  </button>
                  <span className="text-slate-500">
                    Đã chọn: <strong className="tabular-nums text-slate-900">{selectedUserIds.size}</strong>
                    {userOptions.length === 0 ? ' · Không còn user khả dụng' : null}
                  </span>
                </div>
                {usersQuery.isLoading ? (
                  <p className="text-sm text-slate-500">Đang tải danh sách người dùng…</p>
                ) : usersQuery.isError ? (
                  <p className="text-sm text-red-600">Không tải được danh sách người dùng.</p>
                ) : filteredUserOptions.length === 0 ? (
                  <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-6 text-center text-sm text-slate-500">
                    {userOptions.length === 0
                      ? 'Tất cả người dùng đang hoạt động đã có trong đơn vị này.'
                      : 'Không có người dùng khớp từ khóa.'}
                  </p>
                ) : (
                  <ul
                    className="m-0 max-h-[min(50vh,22rem)] list-none overflow-auto rounded-md border border-slate-200 p-1"
                    aria-label="Chọn người dùng"
                  >
                    {filteredUserOptions.map((user) => {
                      const checked = selectedUserIds.has(user.id)
                      return (
                        <li key={user.id}>
                          <label
                            className={[
                              'flex cursor-pointer items-start gap-3 rounded-md px-2 py-2 hover:bg-slate-50',
                              checked ? 'bg-teal-50' : '',
                            ].join(' ')}
                          >
                            <input
                              type="checkbox"
                              className="mt-0.5 size-4 shrink-0 accent-teal-700"
                              checked={checked}
                              onChange={(event) => toggleUser(user.id, event.currentTarget.checked)}
                            />
                            <span className="min-w-0 flex-1">
                              <span className="block text-sm font-medium text-slate-900">
                                {user.fullName?.trim() || user.userName}
                              </span>
                              <span className="block text-xs text-slate-500">
                                {user.userName}
                                {user.email ? ` · ${user.email}` : ''}
                              </span>
                            </span>
                          </label>
                        </li>
                      )
                    })}
                  </ul>
                )}
              </section>
            )}
            <MemberAssignmentFields
              draft={draft}
              setDraft={setDraft}
              positions={positions}
              grades={grades}
              titles={titles}
            />
          </div>
        </FeatureDialog>
      ) : null}
    </>
  )
}