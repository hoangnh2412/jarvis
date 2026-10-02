import filesMock from '../mocks/get-files.json'
import type { FileEntry, StorageInfo } from '../types'

/** Seed storage — nguồn: `mocks/get-files.json`. */
export const FAKE_STORAGE: StorageInfo = filesMock.storage as StorageInfo

/** Seed file/folder — nguồn: `mocks/get-files.json`. */
export const FAKE_FILE_ENTRIES: FileEntry[] = filesMock.items as FileEntry[]
