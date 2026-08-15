import { HardDrive } from 'lucide-react'
import { ProgressBar } from 'primereact/progressbar'
import type { StorageInfo } from '../../types'
import type { FileManagerMessages } from '../../localization'
import {
  formatAvailableBytes,
  formatFileSize,
  formatStoragePercent,
} from '../../utils/fileTree'

export type FileManagerStorageBarProps = {
  storage: StorageInfo
  messages: FileManagerMessages
}

export function FileManagerStorageBar({
  storage,
  messages,
}: FileManagerStorageBarProps) {
  const usedLabel = formatFileSize(storage.usedBytes)
  const totalLabel = formatFileSize(storage.totalBytes)
  const percentLabel = formatStoragePercent(storage.usedBytes, storage.totalBytes)
  const available = formatAvailableBytes(storage.usedBytes, storage.totalBytes)
  const percentValue =
    storage.totalBytes > 0
      ? Math.min(100, (storage.usedBytes / storage.totalBytes) * 100)
      : 0

  return (
    <div className="mx-4 mb-4 rounded-xl border border-slate-200 bg-slate-50/80 p-4">
      <div className="mb-3 flex items-center gap-2 text-sm font-medium text-slate-800">
        <HardDrive className="size-4 text-slate-500" />
        <span>{messages.list.storageUsage(usedLabel, totalLabel)}</span>
      </div>

      <ProgressBar.Root
        value={percentValue}
        className="[&_[data-part=track]]:h-2 [&_[data-part=track]]:overflow-hidden [&_[data-part=track]]:rounded-full [&_[data-part=track]]:bg-slate-200 [&_[data-part=value]]:h-full [&_[data-part=value]]:rounded-full [&_[data-part=value]]:bg-slate-800 [&_[data-part=value]]:transition-[width]"
      >
        <ProgressBar.Track>
          <ProgressBar.Indicator />
        </ProgressBar.Track>
      </ProgressBar.Root>

      <div className="mt-2 flex items-center justify-between text-xs text-slate-500">
        <span>{percentLabel}</span>
        <span>{messages.list.available(available)}</span>
      </div>
    </div>
  )
}
