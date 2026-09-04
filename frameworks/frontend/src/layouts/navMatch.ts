export function navItemMatches(pathname: string, path: string) {
  if (path === '/') return pathname === '/'
  if (pathname === path) return true
  return pathname.startsWith(`${path}/`)
}

export function findBestNavMatch<T extends { path: string }>(
  pathname: string,
  items: readonly T[],
): T | undefined {
  let best: T | undefined
  for (const item of items) {
    if (!navItemMatches(pathname, item.path)) continue
    if (!best || item.path.length > best.path.length) best = item
  }
  return best
}

export function isNavItemActive(
  pathname: string,
  path: string,
  items: readonly { path: string }[],
) {
  return findBestNavMatch(pathname, items)?.path === path
}
