import { useCallback, useEffect, useMemo, useState, type ChangeEvent } from 'react'
import {
  Download,
  FileUp,
  Pencil,
  Plus,
  Search,
  Settings2,
  Trash2,
} from 'lucide-react'
import {
  Button,
  ConfirmDialog,
  DataTable,
  IconField,
  InputText,
  ProgressSpinner,
  getErrorMessage,
  notify,
} from '@platform/core'
import { ImportExportPageShell } from '../components/PageShell'
import {
  btnPrimaryClass,
  btnTextClass,
  fieldInputClass,
} from '../components/fieldStyles'
import {
  deleteTemplate,
  downloadTemplateFile,
  listTemplates,
} from '../services'
import type { ExcelTemplate } from '../types'
import {
  getImportExportTemplateCreatePath,
  getImportExportTemplateConfigurationPath,
  getImportExportTemplateEditPath,
  getImportExportTemplateImportPath,
  navigateImportExport,
} from '../routes'

export function TemplateListPage() {
  const [items, setItems] = useState<ExcelTemplate[]>([])
  const [loading, setLoading] = useState(false)
  const [search, setSearch] = useState('')
  const [pendingDelete, setPendingDelete] = useState<ExcelTemplate | null>(null)
  const [deleting, setDeleting] = useState(false)

  const fetchList = useCallback(async () => {
    setLoading(true)
    try {
      const data = await listTemplates()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setItems([])
      notify.error(getErrorMessage(err, 'Không tải được templates'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchList()
  }, [fetchList])

  const filtered = useMemo(() => {
    const list = Array.isArray(items) ? items : []
    const q = search.trim().toLowerCase()
    if (!q) return list
    return list.filter(
      (t) =>
        t.code.toLowerCase().includes(q) ||
        t.name.toLowerCase().includes(q) ||
        t.fileName.toLowerCase().includes(q),
    )
  }, [items, search])

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    try {
      await deleteTemplate(pendingDelete.code)
      notify.success('Đã xóa template')
      setPendingDelete(null)
      await fetchList()
    } catch (err) {
      notify.error(getErrorMessage(err, 'Xóa template thất bại'))
    } finally {
      setDeleting(false)
    }
  }

  return (
    <>
      <ImportExportPageShell
        title="Excel templates"
        description="Quản lý file mẫu và column mappings (tenant từ ICurrentTenant phía host)."
        headerActions={
          <Button
            type="button"
            unstyled
            className={btnPrimaryClass}
            onClick={() =>
              navigateImportExport(getImportExportTemplateCreatePath())
            }
          >
            <Plus className="size-4" />
            Tạo template
          </Button>
        }
      >
        <div className="mb-4 max-w-sm">
          <IconField.Root className="relative">
            <IconField.Inset className="pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2 text-slate-400">
              <Search className="size-4" />
            </IconField.Inset>
            <InputText
              unstyled
              value={search}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setSearch(e.target.value)
              }
              placeholder="Tìm code / tên / file…"
              className={`${fieldInputClass} !pl-10`}
            />
          </IconField.Root>
        </div>

        <DataTable.Root
          data={filtered}
          dataKey="code"
          loading={loading}
          className="relative overflow-hidden rounded-xl border border-line bg-white shadow-sm"
        >
          <DataTable.TableContainer className="overflow-x-auto">
            <DataTable.Table className="w-full min-w-[860px] border-collapse text-left text-sm">
              <DataTable.THead>
                <DataTable.THeadRow className="border-b border-line bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-mute">
                  <DataTable.THeadCell className="px-4 py-3">
                    Code
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3">
                    Tên
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3">
                    File
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3">
                    Header row
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3">
                    Start row
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3">
                    Row count
                  </DataTable.THeadCell>
                  <DataTable.THeadCell className="px-4 py-3 text-right">
                    Thao tác
                  </DataTable.THeadCell>
                </DataTable.THeadRow>
              </DataTable.THead>
              <DataTable.TBody>
                {({ item }) => {
                  const t = item as ExcelTemplate
                  return (
                    <DataTable.Row className="border-b border-line/80 last:border-b-0 transition-colors hover:bg-teal-50/30">
                      <DataTable.Cell className="px-4 py-3">
                        <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px] text-slate-800">
                          {t.code}
                        </code>
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                        {t.name}
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3 text-mute">
                        {t.fileName}
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3 text-mute">
                        {t.headerRow}
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3 text-mute">
                        {t.startRow}
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3 text-mute">
                        {t.rowCount ?? '—'}
                      </DataTable.Cell>
                      <DataTable.Cell className="px-4 py-3">
                        <div className="flex justify-end gap-0.5">
                          <Button
                            type="button"
                            unstyled
                            className={btnTextClass}
                            aria-label="Import"
                            onClick={() =>
                              navigateImportExport(
                                getImportExportTemplateImportPath(t.code),
                              )
                            }
                          >
                            <FileUp className="size-4" />
                          </Button>
                          <Button
                            onClick={() =>
                              void downloadTemplateFile(t.code).catch((err) =>
                                notify.error(
                                  getErrorMessage(err, 'Download thất bại'),
                                ),
                              )
                            }
                          >
                            <Download className="size-4" />
                          </Button>
                          <Button
                            type="button"
                            unstyled
                            className={btnTextClass}
                            aria-label="Sửa tên"
                            onClick={() =>
                              navigateImportExport(
                                getImportExportTemplateEditPath(t.code),
                              )
                            }
                          >
                            <Pencil className="size-4" />
                          </Button>
                          <Button
                            type="button"
                            unstyled
                            className={btnTextClass}
                            aria-label="Cấu hình"
                            onClick={() =>
                              navigateImportExport(
                                getImportExportTemplateConfigurationPath(t.code),
                              )
                            }
                          >
                            <Settings2 className="size-4" />
                          </Button>
                          <Button
                            type="button"
                            unstyled
                            className={`${btnTextClass} !text-red-600 hover:!bg-red-50`}
                            aria-label="Xóa"
                            onClick={() => setPendingDelete(t)}
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
                  <td colSpan={7} className="px-2 py-12 text-center">
                    {!loading && (
                      <p className="m-0 text-sm text-mute">Chưa có template.</p>
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
            <span className="text-sm text-mute">Đang tải danh sách…</span>
          </DataTable.Loading>
        </DataTable.Root>
      </ImportExportPageShell>

      <ConfirmDialog
        open={pendingDelete != null}
        onClose={() => setPendingDelete(null)}
        onConfirm={() => void confirmDelete()}
        loading={deleting}
        title="Xóa template?"
        description={
          pendingDelete
            ? `Xóa "${pendingDelete.code}" — thao tác không hoàn tác.`
            : undefined
        }
        confirmText="Xóa"
      />
    </>
  )
}
