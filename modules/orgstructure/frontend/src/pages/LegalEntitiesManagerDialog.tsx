import { useCallback, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FeatureDialog, Tag, notify, type RowActionItem } from '@platform/core'
import { Pencil, Plus, Power, PowerOff } from 'lucide-react'
import { LegalEntityForm } from '../components/LegalEntityForm'
import { SimpleTable, type SimpleColumn } from '../components/SimpleTable'
import { btnPrimaryClass, btnSecondaryClass } from '../components/fieldStyles'
import { companyFormToLegalEntityInput, legalEntityToCompanyForm, type LegalEntityFormData } from '../legalEntityForm'
import { notifyApiError } from '../notify'
import {
  createLegalEntity,
  listLegalEntities,
  orgStructureQueryKeys,
  setLegalEntityActive,
  updateLegalEntity,
  type LegalEntity,
} from '../services/orgStructure'

export type LegalEntitiesManagerDialogProps = {
  onClose: () => void
  onSelect?: (id: string) => void
  selectedId?: string
  /** Host quyết định quyền quản lý (mặc định cho phép; host bọc kiểm quyền bên ngoài nếu cần). */
  canManage?: boolean
}

export function LegalEntitiesManagerDialog({
  onClose,
  onSelect,
  selectedId,
  canManage = true,
}: LegalEntitiesManagerDialogProps) {
  const queryClient = useQueryClient()
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<LegalEntity | null>(null)
  const [formInitial, setFormInitial] = useState<Partial<LegalEntityFormData>>({})
  const [formKey, setFormKey] = useState(0)

  const listQuery = useQuery({
    queryKey: orgStructureQueryKeys.legalEntities,
    queryFn: listLegalEntities,
  })

  const refresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: orgStructureQueryKeys.legalEntities })
  }, [queryClient])

  const saveMutation = useMutation({
    mutationFn: async (data: LegalEntityFormData) => {
      const body = companyFormToLegalEntityInput(data)
      if (editing) return updateLegalEntity(editing.id, body)
      return createLegalEntity(body)
    },
    onError: notifyApiError,
    onSuccess: async (saved) => {
      await refresh()
      setFormOpen(false)
      setEditing(null)
      notify.success('Đã lưu pháp nhân.')
      if (onSelect && saved.isActive) onSelect(saved.id)
    },
  })

  const activeMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      setLegalEntityActive(id, isActive),
    onError: notifyApiError,
    onSuccess: async (_, { id, isActive }) => {
      await refresh()
      notify.success(isActive ? 'Đã bật lại pháp nhân.' : 'Đã ngừng dùng pháp nhân.')
      if (!isActive && selectedId === id && onSelect) onSelect('')
    },
  })

  const rows = listQuery.data ?? []

  const openCreate = () => {
    setEditing(null)
    setFormInitial({})
    setFormKey((key) => key + 1)
    setFormOpen(true)
  }

  const openEdit = (row: LegalEntity) => {
    setEditing(row)
    setFormInitial(legalEntityToCompanyForm(row))
    setFormKey((key) => key + 1)
    setFormOpen(true)
  }

  const columns = useMemo<SimpleColumn<LegalEntity>[]>(() => [
    { key: 'code', label: 'Mã', className: 'font-mono text-xs', value: (row) => row.code },
    { key: 'name', label: 'Tên', className: 'font-medium', value: (row) => row.name },
    { key: 'taxId', label: 'MST', className: 'text-slate-500', value: (row) => row.taxId ?? '—' },
    {
      key: 'isActive',
      label: 'Trạng thái',
      render: (row) => row.isActive
        ? <Tag severity="success">Đang dùng</Tag>
        : <Tag severity="secondary">Ngừng</Tag>,
    },
  ], [])

  const actionsFor = useCallback((row: LegalEntity): RowActionItem[] => {
    const actions: RowActionItem[] = []
    if (onSelect && row.isActive) {
      actions.push({
        id: 'select',
        label: selectedId === row.id ? 'Đã chọn' : 'Chọn',
        icon: Plus,
        onClick: () => {
          onSelect(row.id)
          onClose()
        },
      })
    }
    if (canManage) {
      actions.push({ id: 'edit', label: 'Chỉnh sửa', icon: Pencil, onClick: () => openEdit(row) })
      actions.push({
        id: 'toggle',
        label: row.isActive ? 'Ngừng dùng' : 'Bật lại',
        icon: row.isActive ? PowerOff : Power,
        danger: row.isActive,
        separatorBefore: true,
        onClick: () => activeMutation.mutate({ id: row.id, isActive: !row.isActive }),
      })
    }
    return actions
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeMutation, canManage, onClose, onSelect, selectedId])

  return (
    <>
      <FeatureDialog
        open
        onClose={onClose}
        title="Pháp nhân"
        description={
          onSelect
            ? 'Thêm, sửa hoặc ngừng dùng pháp nhân. Chọn một dòng để gắn vào đơn vị.'
            : 'Danh mục pháp nhân của tổ chức đang chọn. Gắn vào từng đơn vị trên cây cơ cấu.'
        }
        size="lg"
        footer={(
          <>
            {canManage ? (
              <button type="button" className={btnPrimaryClass} onClick={openCreate}>
                <Plus className="size-4" aria-hidden />
                Thêm pháp nhân
              </button>
            ) : null}
            <button type="button" className={btnSecondaryClass} onClick={onClose}>Đóng</button>
          </>
        )}
      >
        {listQuery.isError ? (
          <p className="text-sm text-red-600">Không tải được danh sách pháp nhân.</p>
        ) : (
          <SimpleTable
            items={rows}
            columns={columns}
            loading={listQuery.isLoading}
            emptyMessage={listQuery.isLoading ? 'Đang tải…' : canManage ? 'Chưa có pháp nhân. Bấm Thêm pháp nhân để tạo mới.' : 'Chưa có pháp nhân.'}
            actionsFor={actionsFor}
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
          title={editing ? 'Sửa pháp nhân' : 'Thêm pháp nhân'}
          description="Hồ sơ pháp nhân dùng cùng các trường thông tin doanh nghiệp. Mã định danh = mã pháp nhân trên cây; MST khóa khi sửa."
          size="2xl"
          hideFooter
        >
          <LegalEntityForm
            key={formKey}
            formId="legal-entity-form"
            initialValues={formInitial}
            isEdit={Boolean(editing)}
            isSubmitting={saveMutation.isPending}
            onCancel={() => setFormOpen(false)}
            onSubmit={async (data) => {
              await saveMutation.mutateAsync(data)
              return true
            }}
          />
        </FeatureDialog>
      ) : null}
    </>
  )
}
