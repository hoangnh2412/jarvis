import { useCallback, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FeatureDialog, notify, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass, btnSecondaryClass, fieldInputClass } from '../components/fieldStyles'
import { notifyApiError } from '../notify'
import {
  createOrgUnitType,
  deleteOrgUnitType,
  listUnitTypes,
  orgStructureQueryKeys,
  updateOrgUnitType,
  type OrgUnitTypeCatalogItem,
} from '../services/orgStructure'
import { UNIT_TYPE_ICON_OPTIONS, resolveUnitTypeIcon } from '../unitTypeIcons'

export type UnitTypesManagerDialogProps = {
  onClose: () => void
  /** Host quyết định quyền quản lý (mặc định cho phép; host bọc kiểm quyền bên ngoài nếu cần). */
  canManage?: boolean
}

type UnitTypeForm = {
  code: string
  name: string
  icon: string
}

function emptyForm(): UnitTypeForm {
  return { code: '', name: '', icon: 'briefcase' }
}

export function UnitTypesManagerDialog({ onClose, canManage = true }: UnitTypesManagerDialogProps) {
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<OrgUnitTypeCatalogItem | null>(null)
  const [draft, setDraft] = useState<UnitTypeForm>(emptyForm)

  const listQuery = useQuery({
    queryKey: orgStructureQueryKeys.unitTypes,
    queryFn: listUnitTypes,
  })

  const refresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: orgStructureQueryKeys.unitTypes })
  }, [queryClient])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const body = {
        code: draft.code.trim(),
        name: draft.name.trim(),
        icon: draft.icon,
      }
      if (editing?.id) return updateOrgUnitType(editing.id, body)
      return createOrgUnitType(body)
    },
    onError: notifyApiError,
    onSuccess: async () => {
      await refresh()
      setFormOpen(false)
      setEditing(null)
      setDraft(emptyForm())
      notify.success('Đã lưu loại đơn vị.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteOrgUnitType(id),
    onError: notifyApiError,
    onSuccess: async () => {
      await refresh()
      notify.success('Đã xóa loại đơn vị.')
    },
  })

  const rows = listQuery.data ?? []

  const openCreate = () => {
    setEditing(null)
    setDraft(emptyForm())
    setFormOpen(true)
  }

  const openEdit = (row: OrgUnitTypeCatalogItem) => {
    setEditing(row)
    setDraft({ code: row.code, name: row.name, icon: row.icon })
    setFormOpen(true)
  }

  const tableRows = useMemo(
    () => rows.map((row) => ({ ...row, id: row.id || row.code })),
    [rows],
  )

  const columns = useMemo<SimpleColumn<OrgUnitTypeCatalogItem>[]>(() => [
    {
      key: 'icon',
      label: 'Icon',
      render: (row) => {
        const Icon = resolveUnitTypeIcon(row.icon)
        return <Icon className="size-4 text-teal-700" aria-hidden />
      },
    },
    { key: 'name', label: 'Tên loại', className: 'font-medium', value: (row) => row.name },
    { key: 'code', label: 'Mã', className: 'font-mono text-xs text-slate-500', value: (row) => row.code },
  ], [])

  const actionsFor = useCallback((row: OrgUnitTypeCatalogItem): RowActionItem[] => [
    { id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => openEdit(row) },
    {
      id: 'delete',
      label: 'Xóa loại đơn vị',
      icon: Trash2,
      danger: true,
      separatorBefore: true,
      onClick: () => { if (row.id) deleteMutation.mutate(row.id) },
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
  ], [deleteMutation])

  return (
    <>
      <FeatureDialog
        open
        onClose={onClose}
        title="Loại đơn vị"
        description="Tên hiển thị, mã lưu trên cây (UnitType) và icon trên từng dòng. Mã đang được đơn vị dùng thì không đổi / không xóa."
        size="lg"
        footer={(
          <>
            {canManage ? (
              <button type="button" className={btnPrimaryClass} onClick={openCreate}>
                <Plus className="size-4" aria-hidden />
                Thêm loại
              </button>
            ) : null}
            <button type="button" className={btnSecondaryClass} onClick={onClose}>Đóng</button>
          </>
        )}
      >
        {listQuery.isError ? (
          <p className="text-sm text-red-600">Không tải được danh mục loại đơn vị.</p>
        ) : (
          <SimpleTable
            items={tableRows}
            columns={columns}
            loading={listQuery.isLoading}
            emptyMessage={listQuery.isLoading ? 'Đang tải…' : 'Chưa có loại đơn vị.'}
            actionsFor={canManage ? actionsFor : undefined}
          />
        )}
      </FeatureDialog>

      {formOpen ? (
        <FeatureDialog
          open
          onClose={() => {
            if (saveMutation.isPending) return
            setFormOpen(false)
            setEditing(null)
          }}
          title={editing ? 'Sửa loại đơn vị' : 'Thêm loại đơn vị'}
          size="md"
          onSubmit={(event) => {
            event.preventDefault()
            saveMutation.mutate()
          }}
          submitting={saveMutation.isPending}
          submitLabel="Lưu"
        >
          <div className="grid gap-3">
            <label className="grid gap-1 text-sm">
              Tên loại
              <input className={fieldInputClass} required value={draft.name} onChange={(e) => setDraft({ ...draft, name: e.target.value })} />
            </label>
            <label className="grid gap-1 text-sm">
              Mã loại
              <input
                className={fieldInputClass}
                required
                value={draft.code}
                onChange={(e) => setDraft({ ...draft, code: e.target.value })}
                placeholder="Department, Branch, …"
              />
            </label>
            <div className="grid gap-1 text-sm">
              Icon
              <div className="grid grid-cols-4 gap-2 sm:grid-cols-7">
                {UNIT_TYPE_ICON_OPTIONS.map(({ key, label, Icon }) => {
                  const selected = draft.icon === key
                  return (
                    <button
                      key={key}
                      type="button"
                      title={label}
                      aria-label={label}
                      className={[
                        'flex flex-col items-center gap-1 rounded-md border px-1 py-2 text-[10px] leading-tight',
                        selected
                          ? 'border-teal-600 bg-teal-50 text-teal-700'
                          : 'border-slate-200 bg-white text-slate-500 hover:border-teal-600/40',
                      ].join(' ')}
                      onClick={() => setDraft({ ...draft, icon: key })}
                    >
                      <Icon className="size-4 shrink-0" aria-hidden />
                    </button>
                  )
                })}
              </div>
            </div>
          </div>
        </FeatureDialog>
      ) : null}
    </>
  )
}
