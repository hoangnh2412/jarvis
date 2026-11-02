import { useEffect, useState } from 'react'
import { FeatureDialog } from '@platform/core'
import type { SystemRole } from '../services/roles'
import { RolePolicyPanel } from './RolePolicyPanel'
import { btnPrimaryClass, btnSecondaryClass } from '../components/fieldStyles'

export type RolePolicyDialogProps = {
  open: boolean
  role: SystemRole | null
  contextHint?: string | null
  onClose: () => void
  onSave: (policyCodes: string[]) => void | Promise<void>
  saving: boolean
  policyCatalogPath?: string
}

export function RolePolicyDialog({
  open,
  role,
  contextHint,
  onClose,
  onSave,
  saving,
  policyCatalogPath,
}: RolePolicyDialogProps) {
  const readOnlyRole = Boolean(role?.isSystemRole)
  const [assignedCodes, setAssignedCodes] = useState<string[]>([])

  useEffect(() => {
    if (!open || !role) return
    setAssignedCodes(role.policyCodes ?? [])
  }, [open, role])

  const subtitle = [role?.name, contextHint].filter(Boolean).join(' · ')

  return (
    <FeatureDialog
      open={open && Boolean(role)}
      onClose={onClose}
      title="Phạm vi dữ liệu"
      description={subtitle || undefined}
      size="xl"
      footer={(
        <>
          <button type="button" className={btnSecondaryClass} onClick={onClose}>
            {readOnlyRole ? 'Đóng' : 'Hủy'}
          </button>
          {readOnlyRole ? null : (
            <button
              type="button"
              className={btnPrimaryClass}
              disabled={!role || saving}
              onClick={() => void onSave(assignedCodes)}
            >
              Lưu phạm vi ({assignedCodes.length})
            </button>
          )}
        </>
      )}
    >
      <RolePolicyPanel
        enabled={open && Boolean(role)}
        readOnly={readOnlyRole}
        assignedCodes={assignedCodes}
        onAssignedCodesChange={setAssignedCodes}
        contextHint={contextHint}
        onNavigateToCatalog={onClose}
        policyCatalogPath={policyCatalogPath}
      />
    </FeatureDialog>
  )
}
