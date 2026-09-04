// Pages
export { FileManagerPage } from './pages/FileManager'
export type {
  FileManagerPageProps,
  FileManagerPageContentContext,
} from './pages/FileManager'

// Components
export {
  FileManagerSidebar,
  FileManagerToolbar,
  FileManagerTable,
  FileManagerStorageBar,
  FileRowActions,
} from './components'
export type {
  FileManagerSidebarProps,
  FileManagerToolbarProps,
  FileManagerTableProps,
  FileManagerStorageBarProps,
  FileRowActionsProps,
} from './components'

// Types
export type {
  FileEntry,
  FileEntryType,
  StorageInfo,
  ListFilesParams,
  ListFilesResult,
  UploadFilesPayload,
  CreateFolderPayload,
  RenameEntryPayload,
  MoveEntryPayload,
  ActionProps,
} from './types'

// Services (mock — demo mặc định)
export {
  mockListFiles,
  mockUploadFiles,
  mockCreateFolder,
  mockDeleteEntry,
  mockMoveEntry,
  mockRenameEntry,
  mockResetFiles,
  mockGetAllEntries,
} from './services'

// Constants
export {
  FAKE_FILE_ENTRIES,
  FAKE_STORAGE,
  FILE_MANAGER_ROOT_PATH,
} from './constants'

// Routes
export {
  FILE_MANAGER_ROUTES,
  fileManagerPaths,
  getFileManagerListPath,
  getFileManagerRouteList,
  configureFileManagerNavigate,
  navigateFileManager,
} from './routes'
export type { FileManagerRouteKey } from './routes'

// Menu
export { fileManagerMenuItems } from './menu'
export type { FileManagerMenuItem } from './menu'

// Localization
export {
  fileManagerMessagesVi,
  fileManagerMessagesEn,
  getFileManagerMessages,
} from './localization'
export type { FileManagerLocale, FileManagerMessages } from './localization'

// Utils
export {
  formatFileSize,
  formatStoragePercent,
  formatAvailableBytes,
  getBreadcrumbSegments,
  getChildren,
  getEntryPath,
  getParentPath,
  joinPath,
  normalizePath,
  resolveFileManagerContent,
} from './utils'
export type { FileManagerSlotContent } from './utils'
