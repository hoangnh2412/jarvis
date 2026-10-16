import { useCallback, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { ImportWorkflowPage } from '@platform/importexport'
import { importStudents, validateStudentImport } from '../services/students'

export function StudentImportPage() {
  const navigate = useNavigate()

  const handleBack = useCallback(() => {
    navigate('/students')
  }, [navigate])

  const callbacks = useMemo(
    () => ({
      validate: async (file: File) => {
        const res = await validateStudentImport(file)
        return {
          result: res.result,
          message: res.message,
          status: res.status,
        }
      },
      commit: async (file: File) => {
        const res = await importStudents(file)
        return {
          result: res.result,
          message: res.message,
          status: res.status,
        }
      },
    }),
    [],
  )

  return (
    <ImportWorkflowPage
      title="Import học sinh"
      description="Chọn file Excel/CSV — hệ thống tự kiểm tra, sau đó xác nhận import khi không còn lỗi."
      callbacks={callbacks}
      onBack={handleBack}
      accept=".xlsx,.csv"
    />
  )
}
