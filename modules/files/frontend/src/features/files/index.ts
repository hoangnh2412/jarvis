export {
  FileManagerPage,
  configureFileManagerNavigate,
} from '@platform/core'

export type {
  FileManagerPageProps,
  FileEntry,
  ListFilesParams,
  ListFilesResult,
  CreateFolderPayload,
  RenameEntryPayload,
  MoveEntryPayload,
} from '@platform/core'

export { FilesPage } from './pages'
export type { FilesPageProps } from './pages'

export {
  DEFAULT_FILES_CONNECTION,
  type FilesConnection,
  type ApiFileTreeNode,
} from './types'

export {
  filesHttp,
  callGetFileList,
  callGetAllEntries,
  callGetTreeFolderChildren,
  callUploadFiles,
  callCreateFolder,
  callDeleteFile,
  callRenameFile,
  callMoveFile,
  callGetViewUrl,
  callDownloadFile,
  callOpenFile,
  buildDownloadUrl,
} from './services'

export { FILE_MANAGER_ROUTES, getFileManagerListPath } from './routes'
export { fileManagerMenuItems as filesMenuItems } from './menu'
