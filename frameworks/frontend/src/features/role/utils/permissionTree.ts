import type { PermissionGroup, PermissionNode } from '../types'

export type CheckState = 'checked' | 'unchecked' | 'indeterminate'

export function collectAllPermissionIds(
  groups: PermissionGroup[] | PermissionNode[],
): string[] {
  const ids: string[] = []

  const walk = (nodes: PermissionNode[]) => {
    for (const node of nodes) {
      ids.push(node.id)
      if (node.children?.length) walk(node.children)
    }
  }

  if (groups.length === 0) return ids

  if ('permissions' in (groups[0] as PermissionGroup)) {
    for (const group of groups as PermissionGroup[]) walk(group.permissions)
  } else {
    walk(groups as PermissionNode[])
  }

  return ids
}

export function getDescendantIds(node: PermissionNode): string[] {
  const ids = [node.id]
  for (const child of node.children ?? []) {
    ids.push(...getDescendantIds(child))
  }
  return ids
}

export function getNodeCheckState(
  node: PermissionNode,
  granted: ReadonlySet<string>,
): CheckState {
  const ids = getDescendantIds(node)
  const checkedCount = ids.filter((id) => granted.has(id)).length
  if (checkedCount === 0) return 'unchecked'
  if (checkedCount === ids.length) return 'checked'
  return 'indeterminate'
}

export function togglePermissionNode(
  node: PermissionNode,
  granted: Set<string>,
  nextChecked: boolean,
): Set<string> {
  const next = new Set(granted)
  for (const id of getDescendantIds(node)) {
    if (nextChecked) next.add(id)
    else next.delete(id)
  }
  return next
}

export function setGrantAll(
  _granted: Set<string>,
  allIds: string[],
  grant: boolean,
): Set<string> {
  if (!grant) return new Set()
  return new Set(allIds)
}

export function filterPermissionNodes(
  nodes: PermissionNode[],
  query: string,
): PermissionNode[] {
  const q = query.trim().toLowerCase()
  if (!q) return nodes

  const walk = (node: PermissionNode): PermissionNode | null => {
    const labelMatch = node.label.toLowerCase().includes(q)
    const filteredChildren = (node.children ?? [])
      .map(walk)
      .filter((n): n is PermissionNode => n != null)

    if (labelMatch || filteredChildren.length > 0) {
      return {
        ...node,
        children: filteredChildren.length ? filteredChildren : node.children,
      }
    }
    return null
  }

  return nodes.map(walk).filter((n): n is PermissionNode => n != null)
}

export function filterPermissionGroups(
  groups: PermissionGroup[],
  query: string,
): PermissionGroup[] {
  const q = query.trim().toLowerCase()
  if (!q) return groups

  return groups
    .map((group) => {
      const groupMatch = group.label.toLowerCase().includes(q)
      const permissions = filterPermissionNodes(group.permissions, q)
      if (groupMatch || permissions.length > 0) {
        return { ...group, permissions: groupMatch ? group.permissions : permissions }
      }
      return null
    })
    .filter((g): g is PermissionGroup => g != null)
}
