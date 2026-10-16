import { useCallback, useMemo } from 'react'
import { navigateImportExport } from '../routes'
import { importExcel, validateExcelImport } from '../services'
import {
  ImportWorkflowPage,
  type ImportWorkflowPageProps,
} from './ImportWorkflowPage'

export type ExcelImportPageProps = Omit<
  ImportWorkflowPageProps,
  'callbacks'
> & {
  templateCode: string
  backPath?: string
}

export function ExcelImportPage({
  templateCode,
  backPath,
  onBack,
  ...rest
}: ExcelImportPageProps) {
  const handleBack = useCallback(() => {
    if (onBack) {
      onBack()
      return
    }
    if (backPath) {
      navigateImportExport(backPath)
    }
  }, [backPath, onBack])

  const callbacks = useMemo(
    () => ({
      validate: async (file: File) => {
        const result = await validateExcelImport(templateCode, file)
        const validCount = result.sheets.reduce(
          (total, sheet) => total + sheet.validRowCount,
          0,
        )
        const message =
          result.failedRowCount > 0
            ? `Phát hiện ${result.failedRowCount} dòng lỗi` +
              (validCount > 0
                ? `; còn ${validCount} dòng hợp lệ có thể import.`
                : '.')
            : 'Kiểm tra file hoàn tất, không có lỗi.'
        return { result, message }
      },
      commit: async (file: File) => {
        const result = await importExcel(templateCode, file)
        const allFailed =
          result.failedRowCount > 0 && result.savedCount === 0

        let message: string
        if (allFailed) {
          message = `Import thất bại: ${result.failedRowCount} dòng lỗi, không lưu được dòng nào.`
        } else if (result.failedRowCount > 0) {
          message = `Đã lưu ${result.savedCount} dòng hợp lệ; bỏ qua ${result.failedRowCount} dòng lỗi.`
        } else {
          message = `Import thành công ${result.savedCount} dòng.`
        }

        return {
          result,
          message,
          status: allFailed ? 422 : 200,
        }
      },
    }),
    [templateCode],
  )

  return (
    <ImportWorkflowPage
      {...rest}
      callbacks={callbacks}
      onBack={handleBack}
      onConfirmed={rest.onConfirmed}
    />
  )
}


