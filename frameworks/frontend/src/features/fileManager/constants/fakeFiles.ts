import type { FileEntry, StorageInfo } from '../types'

/** Demo storage — khớp UX mẫu: 238.85 KB / 1 TB */
export const FAKE_STORAGE: StorageInfo = {
  usedBytes: Math.round(238.85 * 1024),
  totalBytes: 1024 * 1024 * 1024 * 1024,
}

export const FAKE_FILE_ENTRIES: FileEntry[] = [
  {
    id: 'folder-sang-cr7',
    name: 'SÁNG CR7',
    type: 'folder',
    parentPath: '',
    updatedAt: '2026-08-01T08:00:00.000Z',
  },
  {
    id: 'file-screenshot-1',
    name: 'Screenshot_1.png',
    type: 'file',
    parentPath: '',
    size: Math.round(238.85 * 1024),
    sharedLink: null,
    updatedAt: '2026-08-05T10:30:00.000Z',
  },
]
