import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ORGSTRUCTURE_ROUTES } from '../routes'
import { LegalEntitiesManagerDialog } from './LegalEntitiesManagerDialog'

export type LegalEntitiesPageProps = {
  /** Đường dẫn cây cơ cấu để quay về khi đóng dialog (deep-link cũ). */
  orgStructurePath?: string
  /** Host quyết định quyền quản lý (mặc định cho phép; host bọc kiểm quyền bên ngoài nếu cần). */
  canManage?: boolean
  /** Host tự điều hướng khi đóng thì truyền callback này thay cho router. */
  onClose?: () => void
}

/** Route giữ để deep-link; menu đã gộp vào Cơ cấu tổ chức. */
export function LegalEntitiesPage({
  orgStructurePath = ORGSTRUCTURE_ROUTES.orgStructure,
  canManage = true,
  onClose,
}: LegalEntitiesPageProps) {
  const navigate = useNavigate()
  const [open, setOpen] = useState(true)

  useEffect(() => {
    if (open) return
    if (onClose) onClose()
    else navigate(orgStructurePath, { replace: true })
  }, [navigate, onClose, open, orgStructurePath])

  if (!open) return null

  return <LegalEntitiesManagerDialog canManage={canManage} onClose={() => setOpen(false)} />
}
