import { useMemo, useState, type ChangeEvent } from 'react'
import { Search } from 'lucide-react'
import { IconField } from 'primereact/iconfield'
import { InputText } from 'primereact/inputtext'
import type { PermissionGroup } from '../../types'
import {
  collectAllPermissionIds,
  filterPermissionGroups,
  getNodeCheckState,
  setGrantAll,
  togglePermissionNode,
} from '../../utils/permissionTree'
import { PermissionTreeNode } from '../PermissionTreeNode'

export type PermissionEditorProps = {
  groups: PermissionGroup[]
  value: string[]
  onChange: (permissions: string[]) => void
  disabled?: boolean
}

export function PermissionEditor({
  groups,
  value,
  onChange,
  disabled = false,
}: PermissionEditorProps) {
  const [filter, setFilter] = useState('')
  const [activeGroupId, setActiveGroupId] = useState(groups[0]?.id ?? '')

  const granted = useMemo(() => new Set(value), [value])
  const allIds = useMemo(() => collectAllPermissionIds(groups), [groups])

  const filteredGroups = useMemo(
    () => filterPermissionGroups(groups, filter),
    [groups, filter],
  )

  const activeGroup = useMemo(() => {
    const list = filter ? filteredGroups : groups
    return list.find((g) => g.id === activeGroupId) ?? list[0] ?? null
  }, [activeGroupId, filter, filteredGroups, groups])

  const grantAllChecked = value.length > 0 && value.length >= allIds.length
  const grantAllIndeterminate = value.length > 0 && value.length < allIds.length

  const visibleNodes = activeGroup?.permissions ?? []

  const groupCheckState = (group: PermissionGroup) => {
    const root = { id: group.id, label: group.label, children: group.permissions }
    return getNodeCheckState(root, granted)
  }

  const setGranted = (next: Set<string>) => onChange([...next])

  return (
    <div className="flex min-h-[420px] flex-col gap-3">
      <div className="flex flex-wrap items-center gap-3 border-b border-slate-200 pb-3">
        <IconField.Root className="relative min-w-[220px] flex-1">
          <IconField.Inset className="pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2 text-slate-400">
            <Search className="size-4" />
          </IconField.Inset>
          <InputText
            value={filter}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setFilter(e.target.value)
            }
            placeholder="Filter"
            unstyled
            className="box-border h-10 w-full rounded-lg border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
          />
        </IconField.Root>

        <label className="inline-flex cursor-pointer items-center gap-2 whitespace-nowrap text-sm text-slate-700">
          <input
            type="checkbox"
            className="size-4 rounded border-slate-300 text-teal-600"
            checked={grantAllChecked}
            ref={(el) => {
              if (el) el.indeterminate = grantAllIndeterminate
            }}
            disabled={disabled}
            onChange={(e) =>
              setGranted(setGrantAll(granted, allIds, e.target.checked))
            }
          />
          Grant all permissions
        </label>
      </div>

      <div className="flex min-h-0 flex-1 gap-3 overflow-hidden rounded-xl border border-slate-200">
        <aside className="w-[220px] shrink-0 overflow-y-auto border-r border-slate-200 bg-slate-50/80 p-2">
          <p className="m-0 mb-2 px-2 text-[10px] font-semibold uppercase tracking-[0.08em] text-slate-400">
            Permission Group
          </p>
          <ul className="m-0 flex list-none flex-col gap-1 p-0">
            {(filter ? filteredGroups : groups).map((group) => {
              const active = group.id === (activeGroup?.id ?? '')
              const state = groupCheckState(group)
              return (
                <li key={group.id}>
                  <button
                    type="button"
                    disabled={disabled}
                    onClick={() => setActiveGroupId(group.id)}
                    className={[
                      'flex w-full items-center gap-2 rounded-lg border px-3 py-2.5 text-left text-sm transition',
                      active
                        ? 'border-violet-300 bg-violet-600 text-white shadow-sm'
                        : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-white',
                    ].join(' ')}
                  >
                    <span
                      className={[
                        'size-2 shrink-0 rounded-full',
                        state === 'checked'
                          ? active
                            ? 'bg-white'
                            : 'bg-teal-500'
                          : state === 'indeterminate'
                            ? active
                              ? 'bg-violet-200'
                              : 'bg-amber-400'
                            : active
                              ? 'bg-violet-300'
                              : 'bg-slate-300',
                      ].join(' ')}
                    />
                    <span className="min-w-0 truncate">{group.label}</span>
                  </button>
                </li>
              )
            })}
          </ul>
        </aside>

        <div className="min-w-0 flex-1 overflow-y-auto p-4">
          {activeGroup ? (
            <>
              <div className="mb-3 flex items-center gap-2 border-b border-slate-100 pb-3">
                <input
                  type="checkbox"
                  className="size-4 rounded border-slate-300 text-teal-600"
                  disabled={disabled}
                  checked={groupCheckState(activeGroup) === 'checked'}
                  ref={(el) => {
                    if (el) {
                      el.indeterminate =
                        groupCheckState(activeGroup) === 'indeterminate'
                    }
                  }}
                  onChange={(e) => {
                    const root = {
                      id: activeGroup.id,
                      label: activeGroup.label,
                      children: activeGroup.permissions,
                    }
                    setGranted(
                      togglePermissionNode(
                        root,
                        new Set(granted),
                        e.target.checked,
                      ),
                    )
                  }}
                />
                <span className="text-sm font-semibold text-slate-800">
                  {activeGroup.label}
                </span>
              </div>

              <div className="flex flex-col gap-1">
                {visibleNodes.map((node) => (
                  <PermissionTreeNode
                    key={node.id}
                    node={node}
                    granted={granted}
                    onChange={setGranted}
                    disabled={disabled}
                  />
                ))}
              </div>
            </>
          ) : (
            <p className="m-0 text-sm text-slate-500">Không có nhóm quyền phù hợp.</p>
          )}
        </div>
      </div>
    </div>
  )
}
