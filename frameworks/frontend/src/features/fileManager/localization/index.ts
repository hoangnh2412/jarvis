import { fileManagerMessagesVi } from './vi'

export const fileManagerMessagesEn = {
  ...fileManagerMessagesVi,
  routes: {
    list: 'Files',
  },
  list: {
    ...fileManagerMessagesVi.list,
    description: 'Manage files and folders',
    goUp: 'Go up',
    empty: 'Folder is empty',
    folderTree: 'Folders',
  },
  dialog: {
    ...fileManagerMessagesVi.dialog,
    cancel: 'Cancel',
  },
  toast: {
    uploadSuccess: 'Files uploaded',
    createFolderSuccess: 'Folder created',
    renameSuccess: 'Renamed',
    moveSuccess: 'Moved',
    deleteSuccess: 'Deleted',
    copyLinkSuccess: 'Link copied',
    openFile: 'Opening file',
    error: 'Operation failed',
  },
}

export type FileManagerLocale = 'vi' | 'en'

export type FileManagerMessages =
  | typeof fileManagerMessagesVi
  | typeof fileManagerMessagesEn

const messages: Record<FileManagerLocale, FileManagerMessages> = {
  vi: fileManagerMessagesVi,
  en: fileManagerMessagesEn,
}

export function getFileManagerMessages(locale: FileManagerLocale = 'vi') {
  return messages[locale] ?? fileManagerMessagesVi
}

export { fileManagerMessagesVi }
