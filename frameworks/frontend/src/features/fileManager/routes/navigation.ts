import { FILE_MANAGER_ROUTES, type FileManagerRouteKey } from './paths'

type NavigateFn = (to: string) => void

let navigateFn: NavigateFn | null = null

export function configureFileManagerNavigate(fn: NavigateFn) {
  navigateFn = fn
}

export function navigateFileManager(
  to: FileManagerRouteKey | (string & {}),
) {
  if (!navigateFn) {
    console.warn('[fileManager] navigate chưa được cấu hình — gọi configureFileManagerNavigate')
    return
  }
  const path =
    to in FILE_MANAGER_ROUTES
      ? FILE_MANAGER_ROUTES[to as FileManagerRouteKey]
      : to
  navigateFn(path)
}
