import { Fragment, useEffect, useMemo, useState } from 'react'
import { FeatureDialog } from '@platform/core'
import { Search } from 'lucide-react'
import { SHELL_ACCESS } from '../session'
import { catalogSearchMatches, catalogSearchNormalize } from '../utils'
import type { PermissionChoiceGroup, SystemRole } from '../services/roles'
import { btnPrimaryClass, btnSecondaryClass, codeBadgeClass, systemBadgeClass } from '../components/fieldStyles'

function rolePermissionKeys(role: SystemRole, groups: PermissionChoiceGroup[]) {
  const keys = role.permissionKeys
  if (keys.includes('*') || keys.includes('System.All')) {
    return groups.flatMap((group) => group.permissions.map((permission) => permission.id))
  }
  return keys
}

export function RoleAuthorizationDialog({
  role,
  groups,
  loading,
  onClose,
  onSavePermissions,
}: {
  role: SystemRole | null
  groups: PermissionChoiceGroup[]
  loading: boolean
  onClose: () => void
  onSavePermissions: (permissionKeys: string[]) => void
}) {
  const readOnly = Boolean(role?.isSystemRole)
  const [checkedKeys, setCheckedKeys] = useState<string[]>([])
  const [search, setSearch] = useState('')

  const allPermissionIds = useMemo(
    () => groups.flatMap((group) => group.permissions.map((permission) => permission.id)),
    [groups],
  )

  const filteredGroups = useMemo(() => {
    if (!catalogSearchNormalize(search)) return groups
    return groups
      .map((group) => ({
        ...group,
        permissions: group.permissions.filter((permission) =>
          catalogSearchMatches(
            search,
            permission.id,
            permission.label,
            group.label,
            permission.usageHint,
          ),
        ),
      }))
      .filter((group) => group.permissions.length > 0)
  }, [groups, search])

  useEffect(() => {
    if (!role) return
    setCheckedKeys(rolePermissionKeys(role, groups))
    setSearch('')
  }, [groups, role])

  const roleTogglePermission = (permissionId: string, checked: boolean) => {
    setCheckedKeys((current) => checked
      ? Array.from(new Set([...current, permissionId]))
      : current.filter((id) => id !== permissionId),
    )
  }

  const mergeHiddenRolePermissions = (assignableKeys: string[]) => {
    const preserved = (role?.permissionKeys ?? []).filter((id) => id === SHELL_ACCESS)
    return Array.from(new Set([...assignableKeys, ...preserved]))
  }

  const roleTogglePermissionGroup = (permissionIds: string[], checked: boolean) => {
    setCheckedKeys((current) => checked
      ? Array.from(new Set([...current, ...permissionIds]))
      : current.filter((id) => !permissionIds.includes(id)),
    )
  }

  return (
    <FeatureDialog
      open={Boolean(role)}
      onClose={onClose}
      title="Phân quyền"
      description={role?.name}
      size="2xl"
      footer={(
        <>
          <button type="button" className={btnSecondaryClass} onClick={onClose}>
            {readOnly ? 'Đóng' : 'Hủy'}
          </button>
          {readOnly ? null : (
            <button
              type="button"
              className={btnPrimaryClass}
              disabled={!role || loading}
              onClick={() => onSavePermissions(mergeHiddenRolePermissions(checkedKeys))}
            >
              Lưu quyền chức năng
            </button>
          )}
        </>
      )}
    >
      <div className="flex min-h-0 flex-col gap-3">
        {readOnly ? (
          <p className="text-pretty pt-0.5 text-sm text-slate-500">
            Vai trò hệ thống không sửa quyền. Vai trò tùy chỉnh mới được phân quyền.
          </p>
        ) : (
          <p className="text-pretty pt-0.5 text-sm text-slate-500">
            Chọn chức năng user được truy cập.
          </p>
        )}

        {!loading && groups.length > 0 ? (
          <label className="relative block w-full">
            <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
            <input
              type="search"
              aria-label="Tìm mã hoặc tên quyền"
              value={search}
              onChange={(event) => setSearch(event.currentTarget.value)}
              placeholder="Tìm mã hoặc tên quyền, nhóm, trang…"
              className="h-9 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600"
            />
          </label>
        ) : null}

        {!readOnly && allPermissionIds.length > 0 ? (
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs">
            <button
              type="button"
              className="font-semibold text-teal-700 hover:underline"
              onClick={() => setCheckedKeys(allPermissionIds)}
            >
              Chọn tất cả
            </button>
            <button
              type="button"
              className="font-semibold text-teal-700 hover:underline"
              onClick={() => setCheckedKeys([])}
            >
              Bỏ chọn tất cả
            </button>
            <span className="text-slate-500">
              {checkedKeys.length}/{allPermissionIds.length} quyền
            </span>
          </div>
        ) : null}

        {loading ? (
          <p className="text-sm text-slate-500">Đang tải danh sách quyền…</p>
        ) : null}
        {!loading && groups.length === 0 ? (
          <p className="text-sm text-slate-500">Chưa có quyền trong danh mục.</p>
        ) : null}
        {!loading && groups.length > 0 && filteredGroups.length === 0 ? (
          <p className="text-sm text-slate-500">Không tìm thấy quyền phù hợp.</p>
        ) : null}
        {!loading && filteredGroups.length > 0 ? (
          <div className="overflow-hidden rounded-xl border border-slate-200">
            <table className="w-full border-collapse text-left">
              <tbody>
                {filteredGroups.map((group) => {
                  const permissionIds = group.permissions.map((permission) => permission.id)
                  const selectedCount = permissionIds.filter((id) => checkedKeys.includes(id)).length
                  const allInGroup = permissionIds.length > 0 && selectedCount === permissionIds.length
                  return (
                    <Fragment key={group.id}>
                      <tr
                        className={`border-t border-slate-200 bg-slate-50 ${selectedCount > 0 ? 'border-l-[3px] border-l-teal-600/60' : ''}`}
                      >
                        <td className="px-3 py-2.5">
                          <div className="flex flex-wrap items-center justify-between gap-2">
                            <label className={`flex min-w-0 items-center gap-2 text-sm font-semibold ${readOnly ? '' : 'cursor-pointer'}`}>
                              <input
                                type="checkbox"
                                checked={allInGroup}
                                disabled={readOnly}
                                onChange={(event) => roleTogglePermissionGroup(permissionIds, event.currentTarget.checked)}
                                className="size-4 shrink-0 accent-teal-700"
                                aria-label={`Chọn toàn bộ nhóm ${group.label}`}
                              />
                              <span>{group.label}</span>
                            </label>
                            <span className="shrink-0 text-xs text-slate-500">
                              {selectedCount}/{permissionIds.length}
                            </span>
                          </div>
                        </td>
                      </tr>
                      {group.permissions.map((permission) => {
                        const isChecked = checkedKeys.includes(permission.id)
                        return (
                          <tr
                            key={permission.id}
                            className={`border-t border-slate-200 text-sm transition-colors ${isChecked ? 'bg-teal-50' : ''}`}
                          >
                            <td className="px-3 py-2.5 align-top">
                              <label className={`flex items-start gap-2 pl-6 ${readOnly ? '' : 'cursor-pointer'}`}>
                                <input
                                  type="checkbox"
                                  checked={isChecked}
                                  disabled={readOnly}
                                  onChange={(event) => roleTogglePermission(permission.id, event.currentTarget.checked)}
                                  className="mt-0.5 size-4 shrink-0 accent-teal-700"
                                />
                                <span className="min-w-0">
                                  <span className="flex flex-wrap items-baseline gap-x-2 gap-y-0.5">
                                    <span className="font-medium text-slate-800">{permission.label}</span>
                                    <span className={codeBadgeClass}>{permission.id}</span>
                                    {permission.isSystemPermission ? (
                                      <span className={systemBadgeClass}>Hệ thống</span>
                                    ) : null}
                                  </span>
                                  {permission.usageHint ? (
                                    <span className="mt-0.5 block text-pretty text-xs text-slate-500">
                                      {permission.usageHint}
                                    </span>
                                  ) : (
                                    <span className="mt-0.5 block text-xs italic text-slate-500">
                                      Chưa ghi trong menu — xem mã quyền trên màn Quyền
                                    </span>
                                  )}
                                </span>
                              </label>
                            </td>
                          </tr>
                        )
                      })}
                    </Fragment>
                  )
                })}
              </tbody>
            </table>
          </div>
        ) : null}
      </div>
    </FeatureDialog>
  )
}
