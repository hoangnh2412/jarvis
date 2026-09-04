export const FILE_MANAGER_ROUTES = {
  list: '/files',
} as const

export type FileManagerRouteKey = keyof typeof FILE_MANAGER_ROUTES

export function getFileManagerListPath() {
  return FILE_MANAGER_ROUTES.list
}

export function getFileManagerRouteList() {
  return Object.values(FILE_MANAGER_ROUTES)
}

export const fileManagerPaths = FILE_MANAGER_ROUTES
