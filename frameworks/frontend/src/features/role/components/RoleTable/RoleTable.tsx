import { memo, useMemo, useState, type ReactNode } from 'react'
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  X,
} from 'lucide-react'
import { DataTable } from 'primereact/datatable'
import { InputText } from 'primereact/inputtext'
import { ProgressSpinner } from 'primereact/progressspinner'
import { Select } from 'primereact/select'
import { Tag } from 'primereact/tag'
import type { Role } from '../../types'
import { RoleRowActions } from '../RoleRowActions'

export type RoleTableProps = {
  items: Role[]
  loading?: boolean
  emptyMessage?: string
  onView?: (role: Role) => void
  onEdit?: (role: Role) => void
  onPermissions?: (role: Role) => void
  onDelete?: (role: Role) => void
  className?: string
}

type RoleTableRow = Role & {
  permissionCount: number
}

type FilterSlot = {
  value?: unknown
  onChange: (event: unknown, value: unknown, matchMode?: string) => void
  onClear?: (event: unknown) => void
}

const filterInputClass =
  'box-border mt-1.5 h-9 w-full min-w-[7rem] rounded-lg border border-solid border-slate-200 bg-white px-2.5 text-sm font-normal normal-case tracking-normal text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20'

const filterSelectTriggerClass =
  'mt-1.5 flex h-9 w-full min-w-[8rem] items-center justify-between gap-1 rounded-lg border border-solid border-slate-200 bg-white px-2.5 text-sm font-normal normal-case tracking-normal text-slate-800 shadow-sm outline-none transition hover:border-slate-300 focus-visible:border-teal-600 focus-visible:ring-2 focus-visible:ring-teal-600/20'

const visibilityFilterOptions = [
  { label: 'Tất cả', value: '' },
  { label: 'Public', value: 'public' },
  { label: 'Private', value: 'private' },
] as const

function TextColumnFilter({ value, onChange, onClear }: FilterSlot) {
  const text = value == null ? '' : String(value)
  return (
    <div className="relative">
      <InputText
        unstyled
        value={text}
        placeholder="Lọc…"
        className={filterInputClass}
        onChange={(e: { target: { value: string } }) => {
          const next = e.target.value
          onChange(e, next === '' ? null : next, 'contains')
        }}
      />
      {text ? (
        <button
          type="button"
          aria-label="Xóa lọc"
          className="absolute right-1.5 top-[calc(50%+3px)] inline-flex size-5 -translate-y-1/2 items-center justify-center rounded text-slate-400 hover:bg-slate-100 hover:text-slate-600"
          onClick={(e) => onClear?.(e)}
        >
          <X className="size-3.5" aria-hidden />
        </button>
      ) : null}
    </div>
  )
}

function VisibilityColumnFilter({ value, onChange, onClear }: FilterSlot) {
  const [open, setOpen] = useState(false)
  const selected =
    value === true || value === 'true' || value === 'public'
      ? 'public'
      : value === false || value === 'false' || value === 'private'
        ? 'private'
        : ''

  return (
    <Select.Root
      value={selected}
      open={open}
      options={[...visibilityFilterOptions]}
      optionLabel="label"
      optionValue="value"
      onOpenChange={(e: { value: boolean }) => setOpen(e.value)}
      onValueChange={(e: { value?: unknown }) => {
        const next = e.value
        if (next === '' || next == null) {
          onClear?.(e)
          return
        }
        onChange(e, next === 'public', 'equals')
      }}
    >
      <Select.Trigger type="button" className={filterSelectTriggerClass}>
        <Select.Value placeholder="Tất cả" />
        <Select.Indicator className="text-slate-400">▾</Select.Indicator>
      </Select.Trigger>
      <Select.Portal>
        <Select.Positioner className="z-[200]">
          <Select.Popup className="min-w-[var(--px-positioner-anchor-width)] overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-lg">
            <Select.List className="m-0 list-none p-0 outline-none">
              {visibilityFilterOptions.map((option, index) => (
                <Select.Option
                  key={String(option.value)}
                  index={index}
                  className="cursor-pointer px-3 py-2 text-sm text-slate-800 outline-none data-[focused]:bg-slate-50 data-[selected]:bg-teal-50 data-[selected]:font-medium data-[selected]:text-teal-800"
                >
                  {option.label}
                </Select.Option>
              ))}
            </Select.List>
          </Select.Popup>
        </Select.Positioner>
      </Select.Portal>
    </Select.Root>
  )
}

function HeaderWithFilter({
  label,
  sortField,
  align = 'left',
  children,
}: {
  label: string
  sortField?: string
  align?: 'left' | 'right'
  children?: ReactNode
}) {
  const title = sortField ? (
    <DataTable.Sort
      field={sortField}
      className="inline-flex w-full items-center gap-1 border-0 bg-transparent p-0 text-xs font-semibold uppercase tracking-wide text-mute outline-none transition hover:text-teal-800"
    >
      <span>{label}</span>
      <span className="inline-flex size-3.5 shrink-0 items-center justify-center text-slate-400">
        <DataTable.SortIndicator match="asc" className="text-teal-700">
          <ArrowUp className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
        <DataTable.SortIndicator match="desc" className="text-teal-700">
          <ArrowDown className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
        <DataTable.SortIndicator match="unsorted" className="opacity-50">
          <ArrowUpDown className="size-3.5" aria-hidden />
        </DataTable.SortIndicator>
      </span>
    </DataTable.Sort>
  ) : (
    <span className="block text-xs font-semibold uppercase tracking-wide text-mute">
      {label}
    </span>
  )

  return (
    <DataTable.THeadCell
      className={[
        'px-4 py-3 align-top',
        align === 'right' ? 'text-right' : 'text-left',
      ].join(' ')}
    >
      {title}
      {children}
    </DataTable.THeadCell>
  )
}

export const RoleTable = memo(function RoleTable({
  items,
  loading = false,
  emptyMessage = 'Không có dữ liệu',
  onView,
  onEdit,
  onPermissions,
  onDelete,
  className = '',
}: RoleTableProps) {
  const rows = useMemo<RoleTableRow[]>(
    () =>
      items.map((role) => ({
        ...role,
        permissionCount: role.permissions.length,
      })),
    [items],
  )

  return (
    <DataTable.Root
      data={rows}
      dataKey="id"
      loading={loading}
      removableSort
      className={`relative overflow-hidden rounded-xl border border-line bg-white shadow-sm ${className}`}
    >
      <DataTable.TableContainer className="overflow-x-auto">
        <DataTable.Table className="w-full min-w-[720px] border-collapse text-left text-sm">
          <DataTable.THead>
            <DataTable.THeadRow className="border-b border-line bg-slate-50/80">
              <HeaderWithFilter label="Tên" sortField="name">
                <DataTable.Filter field="name" display="row" dataType="text">
                  {(ctx: FilterSlot) => <TextColumnFilter {...ctx} />}
                </DataTable.Filter>
              </HeaderWithFilter>

              <HeaderWithFilter label="Hiển thị" sortField="displayName">
                <DataTable.Filter field="displayName" display="row" dataType="text">
                  {(ctx: FilterSlot) => <TextColumnFilter {...ctx} />}
                </DataTable.Filter>
              </HeaderWithFilter>

              <HeaderWithFilter label="Thuộc tính" sortField="isPublic">
                <DataTable.Filter field="isPublic" display="row" dataType="boolean">
                  {(ctx: FilterSlot) => <VisibilityColumnFilter {...ctx} />}
                </DataTable.Filter>
              </HeaderWithFilter>

              <HeaderWithFilter label="Quyền" sortField="permissionCount" />

              <HeaderWithFilter label="Thao tác" align="right" />
            </DataTable.THeadRow>
          </DataTable.THead>

          <DataTable.TBody>
            {({ item }) => {
              const role = item as RoleTableRow
              return (
                <DataTable.Row className="border-b border-line/80 transition-colors last:border-b-0 hover:bg-teal-50/30">
                  <DataTable.Cell className="px-4 py-3">
                    <code className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[13px] text-slate-800">
                      {role.name}
                    </code>
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 font-medium text-ink">
                    {role.displayName}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3">
                    {role.isPublic ? (
                      <Tag severity="success" className="font-medium text-slate-500">
                        Public
                      </Tag>
                    ) : (
                      <Tag severity="secondary" className="font-medium text-slate-500">
                        Private
                      </Tag>
                    )}
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-mute">
                    {role.permissionCount} quyền
                  </DataTable.Cell>
                  <DataTable.Cell className="px-4 py-3 text-right">
                    <RoleRowActions
                      role={role}
                      onView={onView}
                      onEdit={onEdit}
                      onPermissions={onPermissions}
                      onDelete={onDelete}
                    />
                  </DataTable.Cell>
                </DataTable.Row>
              )
            }}
          </DataTable.TBody>

          <DataTable.EmptyTBody>
            <tr>
              <td colSpan={5} className="px-2 py-12 text-center">
                {!loading ? (
                  <p className="m-0 text-sm text-mute">{emptyMessage}</p>
                ) : null}
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
        <span className="text-sm text-mute">Đang tải…</span>
      </DataTable.Loading>
    </DataTable.Root>
  )
})
