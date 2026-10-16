import { FileManagerPage, type FileManagerPageProps } from '@platform/core'
import {
  buildDownloadUrl,
  callCreateFolder,
  callDeleteFile,
  callGetAllEntries,
  callGetFileList,
  callGetTreeFolderChildren,
  callGetViewUrl,
  callMoveFile,
  callOpenFile,
  callRenameFile,
  callUploadFiles,
} from '../services/files'
import type { FilesConnection } from '../types'
import { DEFAULT_FILES_CONNECTION } from '../types'

export type FilesPageProps = {
  locale?: FileManagerPageProps['locale']
  className?: FileManagerPageProps['className']
  connection?: FilesConnection
  content?: FileManagerPageProps['content']
}

export function FilesPage({
  locale = 'vi',
  className,
  connection = DEFAULT_FILES_CONNECTION,
  content,
}: FilesPageProps) {
  const rootLabel = connection.rootLabel?.trim() || undefined

  return (
    <FileManagerPage
      locale={locale}
      className={className}
      content={content}
      rootLabel={rootLabel}
      callback={{
        list: {
          onSubmit: (params) => callGetFileList(params, connection),
        },
        entries: {
          onSubmit: () => callGetAllEntries(connection),
        },
        treeChildren: {
          onSubmit: ({ parentPath }) =>
            callGetTreeFolderChildren(parentPath, connection),
        },
        upload: {
          onSubmit: (params) => callUploadFiles(params, connection),
        },
        createFolder: {
          onSubmit: (params) => callCreateFolder(params, connection),
        },
        delete: {
          onSubmit: (entry) => callDeleteFile(entry, connection),
        },
        rename: {
          onSubmit: (params) => callRenameFile(params, connection),
        },
        move: {
          onSubmit: (params) => callMoveFile(params, connection),
        },
        openFile: {
          onSubmit: (entry) => callOpenFile(entry, connection),
        },
        getSharedLink: {
          onSubmit: async (entry) => {
            try {
              return await callGetViewUrl(entry, connection)
            } catch {
              return buildDownloadUrl(entry, connection)
            }
          },
        },
      }}
    />
  )
}
