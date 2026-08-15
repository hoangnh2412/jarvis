import { CornerUpLeft, Home } from 'lucide-react'
import { Breadcrumb } from 'primereact/breadcrumb'
import { Button } from 'primereact/button'
import type { FileManagerMessages } from '../../localization'
import { getBreadcrumbSegments, normalizePath } from '../../utils/fileTree'
import { btnOutlinedClass, navBtnClass } from '../fieldStyles'

export type FileManagerToolbarProps = {
  currentPath: string
  messages: FileManagerMessages
  disabled?: boolean
  onNavigate: (path: string) => void
  onGoUp: () => void
}

export function FileManagerToolbar({
  currentPath,
  messages,
  disabled = false,
  onNavigate,
  onGoUp,
}: FileManagerToolbarProps) {
  const crumbs = getBreadcrumbSegments(currentPath).map((crumb, index) =>
    index === 0 ? { ...crumb, label: messages.list.allFiles } : crumb,
  )
  const canGoUp = normalizePath(currentPath) !== ''

  return (
    <div className="flex flex-col gap-3 border-b border-slate-200 px-4 py-3">
      <Breadcrumb.Root className="flex items-center gap-3 text-sm text-slate-600">
        <div className="flex flex-wrap items-center gap-2">
          <Button
            type="button"
            unstyled
            disabled={disabled || !canGoUp}
            onClick={onGoUp}
            title={messages.list.goUp}
            className={`${btnOutlinedClass} !w-auto !px-3 ${navBtnClass}`}
          >
            <CornerUpLeft className="size-4" />
          </Button>
        </div>
        <Breadcrumb.List className="m-0 flex flex-wrap list-none items-center gap-1 p-0">
          {crumbs.map((crumb, index) => {
            const isLast = index === crumbs.length - 1
            return (
              <Breadcrumb.Item
                key={crumb.path || 'root'}
                className="inline-flex items-center gap-1"
              >
                {index > 0 ? (
                  <Breadcrumb.Separator className="text-slate-400">
                    /
                  </Breadcrumb.Separator>
                ) : null}
                {index === 0 ? (
                  <Home className="size-3.5 text-slate-400" aria-hidden />
                ) : null}
                {isLast ? (
                  <Breadcrumb.Current className="font-medium text-slate-900">
                    {crumb.label}
                  </Breadcrumb.Current>
                ) : (
                  <Breadcrumb.Link
                    as="button"
                    type="button"
                    disabled={disabled}
                    onClick={() => onNavigate(crumb.path)}
                    className={`rounded-lg border-0 bg-transparent px-1.5 py-0.5 text-slate-600 hover:bg-slate-100 hover:text-teal-700 ${navBtnClass}`}
                  >
                    {crumb.label}
                  </Breadcrumb.Link>
                )}
              </Breadcrumb.Item>
            )
          })}
        </Breadcrumb.List>
      </Breadcrumb.Root>


    </div>
  )
}
