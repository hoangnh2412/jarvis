import {
  useCallback,
  useEffect,
  useState,
  type ChangeEvent,
} from 'react'
import { useNavigate } from 'react-router-dom'
import {
  FileDown,
  FileUp,
  Pencil,
  Plus,
  Search,
  Trash2,
} from 'lucide-react'
import {
  Button,
  ConfirmDialog,
  DataTable,
  InputText,
  ProgressSpinner,
  getErrorMessage,
  notify,
} from '@platform/core'
import { ImportExportPageShell } from '@platform/importexport'
import {
  btnOutlinedClass,
  btnPrimaryClass,
  btnTextClass,
  fieldInputClass,
} from '../components/fieldStyles'
import {
  deleteStudent,
  exportStudents,
  listStudents,
  type Student,
} from '../services/students'

export function StudentListPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<Student[]>([])
  const [loading, setLoading] = useState(false)
  const [search, setSearch] = useState('')
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [size] = useState(20)
  const [pendingDelete, setPendingDelete] = useState<Student | null>(null)
  const [deleting, setDeleting] = useState(false)
  const [exporting, setExporting] = useState(false)

  const fetchList = useCallback(async () => {
    setLoading(true)
    try {
      const filter = search.trim()
        ? JSON.stringify(['Name', 'contains', search.trim()])
        : undefined
      const result = await listStudents({ page, size, filter })
      setItems(result.items)
      setTotalCount(result.totalCount)
    } catch (err) {
      setItems([])
      setTotalCount(0)
      notify.error(getErrorMessage(err, 'Không tải được danh sách học sinh'))
    } finally {
      setLoading(false)
    }
  }, [page, search, size])

  useEffect(() => {
    void fetchList()
  }, [fetchList])

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      await deleteStudent(pendingDelete.id)
      notify.success('Đã xóa học sinh')
      setPendingDelete(null)
      await fetchList()
    } catch (err) {
      notify.error(getErrorMessage(err, 'Xóa học sinh thất bại'))
    } finally {
      setDeleting(false)
    }
  }

  const onExport = async () => {
    setExporting(true)
    try {
      await exportStudents('students.xlsx')
      notify.success('Đã export danh sách học sinh')
    } catch (err) {
      notify.error(getErrorMessage(err, 'Export thất bại'))
    } finally {
      setExporting(false)
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / size))

  return (
    <ImportExportPageShell
      title="Học sinh"
      description="CRUD mẫu — import/export qua API domain; BE resolve template từ Setting."
      actions={
        <>
          <Button
            type="button"
            unstyled
            className={btnOutlinedClass}
            onClick={() => navigate('/students/import')}
          >
            <FileUp className="size-4" />
            Import Excel
          </Button>
          <Button
            type="button"
            unstyled
            disabled={exporting}
            className={btnOutlinedClass}
            onClick={() => void onExport()}
          >
            <FileDown className="size-4" />
            {exporting ? 'Đang export…' : 'Export Excel'}
          </Button>
          <Button
            type="button"
            unstyled
            className={btnPrimaryClass}
            onClick={() => navigate('/students/create')}
          >
            <Plus className="size-4" />
            Thêm học sinh
          </Button>
        </>
      }
    >
      <div className="mb-3 max-w-sm">
        <div className="relative">
          <Search className="pointer-events-none absolute left-3 top-1/2 z-[1] size-4 -translate-y-1/2 text-slate-400" />
          <InputText
            unstyled
            value={search}
            onChange={(e: ChangeEvent<HTMLInputElement>) => {
              setSearch(e.target.value)
              setPage(1)
            }}
            placeholder="Tìm theo tên…"
            className={`${fieldInputClass} !pl-10`}
          />
        </div>
      </div>

      <DataTable.Root
        data={items}
        dataKey="id"
        loading={loading}
        className="relative overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
      >
        <DataTable.TableContainer className="overflow-x-auto">
          <DataTable.Table className="w-full min-w-[560px] border-collapse text-left text-sm">
            <DataTable.THead>
              <DataTable.THeadRow className="border-b border-slate-100 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <DataTable.THeadCell className="px-4 py-3">
                  Tên
                </DataTable.THeadCell>
                <DataTable.THeadCell className="px-4 py-3">
                  Tuổi
                </DataTable.THeadCell>
                <DataTable.THeadCell className="px-4 py-3 text-right">
                  Thao tác
                </DataTable.THeadCell>
              </DataTable.THeadRow>
            </DataTable.THead>
            <DataTable.TBody>
              {({ item }) => {
                const s = item as Student
                return (
                  <DataTable.Row className="border-b border-slate-100 last:border-b-0 hover:bg-teal-50/30">
                    <DataTable.Cell className="px-4 py-3 font-medium text-slate-900">
                      {s.name}
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-3 text-slate-600">
                      {s.age}
                    </DataTable.Cell>
                    <DataTable.Cell className="px-4 py-3">
                      <div className="flex justify-end gap-0.5">
                        <Button
                          type="button"
                          unstyled
                          className={btnTextClass}
                          aria-label="Sửa"
                          onClick={() => navigate(`/students/${s.id}/edit`)}
                        >
                          <Pencil className="size-4" />
                        </Button>
                        <Button
                          type="button"
                          unstyled
                          className={`${btnTextClass} !text-red-600 hover:!bg-red-50`}
                          aria-label="Xóa"
                          onClick={() => setPendingDelete(s)}
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      </div>
                    </DataTable.Cell>
                  </DataTable.Row>
                )
              }}
            </DataTable.TBody>
            <DataTable.EmptyTBody>
              <tr>
                <td
                  colSpan={3}
                  className="px-2 text-center"
                  style={{ paddingTop: '3rem', paddingBottom: '3rem' }}
                >
                  {!loading && (
                    <p className="m-0 text-sm text-slate-500">
                      Chưa có học sinh. Thêm thủ công hoặc import Excel.
                    </p>
                  )}
                </td>
              </tr>
            </DataTable.EmptyTBody>
          </DataTable.Table>
        </DataTable.TableContainer>

        <DataTable.Loading className="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-white/80">
          <ProgressSpinner.Root className="h-8 w-8">
            <ProgressSpinner.Track />
            <ProgressSpinner.Range />
          </ProgressSpinner.Root>
          <span className="text-sm text-slate-500">Đang tải danh sách…</span>
        </DataTable.Loading>
      </DataTable.Root>

      {totalCount > size ? (
        <div className="flex items-center justify-between text-sm text-slate-600">
          <span>
            Trang {page} / {totalPages} — {totalCount} bản ghi
          </span>
          <div className="flex gap-2">
            <Button
              type="button"
              unstyled
              disabled={page <= 1}
              className={btnOutlinedClass}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
            >
              Trước
            </Button>
            <Button
              type="button"
              unstyled
              disabled={page >= totalPages}
              className={btnOutlinedClass}
              onClick={() => setPage((p) => p + 1)}
            >
              Sau
            </Button>
          </div>
        </div>
      ) : null}

      <ConfirmDialog
        open={pendingDelete != null}
        onClose={() => setPendingDelete(null)}
        onConfirm={() => void confirmDelete()}
        loading={deleting}
        title="Xóa học sinh?"
        description={
          pendingDelete
            ? `Xóa "${pendingDelete.name}" — thao tác không hoàn tác.`
            : undefined
        }
        confirmText="Xóa"
      />
    </ImportExportPageShell>
  )
}
