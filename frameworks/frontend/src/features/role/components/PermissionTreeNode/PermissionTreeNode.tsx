import { useEffect, useRef } from 'react'
import type { PermissionNode } from '../../types'
import {
  getNodeCheckState,
  togglePermissionNode,
  type CheckState,
} from '../../utils/permissionTree'

export type PermissionTreeNodeProps = {
  node: PermissionNode
  granted: ReadonlySet<string>
  depth?: number
  onChange: (next: Set<string>) => void
  disabled?: boolean
}

function TriCheckbox({
  state,
  disabled,
  onToggle,
  label,
}: {
  state: CheckState
  disabled?: boolean
  onToggle: (checked: boolean) => void
  label: string
}) {
  const ref = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (ref.current) ref.current.indeterminate = state === 'indeterminate'
  }, [state])

  return (
    <label className="flex cursor-pointer items-start gap-2.5 py-1.5">
      <input
        ref={ref}
        type="checkbox"
        className="mt-0.5 size-4 shrink-0 rounded border-slate-300 text-teal-600 focus:ring-teal-500/30 disabled:cursor-not-allowed"
        checked={state === 'checked'}
        disabled={disabled}
        onChange={(e) => onToggle(e.target.checked)}
      />
      <span className="text-sm leading-snug text-slate-800">{label}</span>
    </label>
  )
}

export function PermissionTreeNode({
  node,
  granted,
  depth = 0,
  onChange,
  disabled = false,
}: PermissionTreeNodeProps) {
  const state = getNodeCheckState(node, granted)
  const hasChildren = Boolean(node.children?.length)

  const handleToggle = (checked: boolean) => {
    onChange(togglePermissionNode(node, new Set(granted), checked))
  }

  return (
    <div className="min-w-0">
      <div style={{ paddingLeft: depth * 20 }}>
        <TriCheckbox
          state={state}
          disabled={disabled}
          onToggle={handleToggle}
          label={node.label}
        />
      </div>
      {hasChildren ? (
        <div className="border-l border-slate-200/90 ml-2">
          {node.children!.map((child) => (
            <PermissionTreeNode
              key={child.id}
              node={child}
              granted={granted}
              depth={depth + 1}
              onChange={onChange}
              disabled={disabled}
            />
          ))}
        </div>
      ) : null}
    </div>
  )
}
