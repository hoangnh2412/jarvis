import { Bell, LogOut, Menu, User } from 'lucide-react'
import type { ChangeEvent } from 'react'
import { Badge } from 'primereact/badge'
import { Button } from 'primereact/button'
import { InputText } from 'primereact/inputtext'
import { OverlayBadge } from 'primereact/overlaybadge'
import { Popover } from 'primereact/popover'
import { Toolbar } from 'primereact/toolbar'
import Logo from '../../common/Logo'

export type HeaderProps = {
  title?: string
  onMenuClick?: () => void
  user?: { fullName?: string; email?: string } | null
  notificationCount?: number
  searchPlaceholder?: string
  searchValue?: string
  onSearchChange?: (value: string) => void
  onProfileClick?: () => void
  onLogout?: () => void
  className?: string
}

export function Header({
  title = 'Quản lý',
  onMenuClick,
  user = null,
  notificationCount = 0,
  searchPlaceholder = 'Tìm kiếm...',
  searchValue,
  onSearchChange,
  onProfileClick,
  onLogout,
  className = '',
}: HeaderProps) {
  return (
    <Toolbar.Root
      className={[
        'sticky top-0 z-30 box-border flex h-16 w-full shrink-0 items-center gap-4 border-0 border-b border-slate-200 bg-white pl-4 pr-5 sm:pr-6 lg:pl-6 lg:pr-8',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <Toolbar.Start className="flex min-w-0 items-center gap-3">
        {onMenuClick && (
          <Button
            type="button"
            unstyled
            onClick={onMenuClick}
            aria-label="Mở menu"
            className="pr-btn-text inline-flex h-10 cursor-pointer appearance-none items-center justify-center rounded-lg border-0 bg-transparent px-2 text-slate-500 transition-colors hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 lg:hidden"
          >
            <Menu className="h-5 w-5" />
          </Button>
        )}

        <Logo size="sm" showText={false} className="hidden sm:flex lg:hidden" />
        <h1 className="m-0 truncate text-lg font-semibold text-slate-900">
          {title}
        </h1>
      </Toolbar.Start>

      <Toolbar.End className="ml-auto flex shrink-0 items-center gap-2 sm:gap-3">
        <div className="hidden w-56 md:block lg:w-64">
          <InputText
            type="search"
            value={searchValue}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              onSearchChange?.(e.target.value)
            }
            placeholder={searchPlaceholder}
            unstyled
            className="box-border h-11 w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 text-sm text-slate-800 shadow-sm outline-none transition-[border-color,box-shadow] placeholder:text-slate-400 hover:border-slate-300 focus:border-teal-600 focus:ring-2 focus:ring-teal-600/20"
          />
        </div>

        <OverlayBadge className="relative inline-flex shrink-0 text-slate-600">
          <Bell className="size-6" />
          {notificationCount > 0 && (
            <Badge shape="circle" severity="danger">
              {notificationCount > 99 ? '99+' : notificationCount}
            </Badge>
          )}
        </OverlayBadge>

        <Popover.Root>
          <Popover.Trigger
            className="flex shrink-0 cursor-pointer appearance-none items-center gap-2 rounded-lg border-0 border-l border-solid border-slate-200 bg-transparent py-1 pl-3 pr-1 transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-600/30"
            aria-label="Tài khoản"
          >
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-teal-700">
              <User className="h-4 w-4 text-white" />
            </div>
            <div className="hidden max-w-[9rem] text-left sm:block">
              <p className="m-0 truncate text-sm font-medium leading-none text-slate-900">
                {user?.fullName ?? 'Quản trị viên'}
              </p>
              <p className="m-0 mt-0.5 truncate text-xs text-slate-500">
                {user?.email ?? 'Admin'}
              </p>
            </div>
          </Popover.Trigger>
          <Popover.Portal>
            <Popover.Positioner>
              <Popover.Popup className="z-[120] w-56 rounded-lg border border-slate-200 bg-white py-1 shadow-lg">
                <p className="m-0 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-slate-400">
                  Tài khoản
                </p>
                {onProfileClick && (
                  <button
                    type="button"
                    className="flex w-full items-center gap-2 border-0 bg-transparent px-3 py-2 text-left text-sm text-slate-700 hover:bg-slate-50"
                    onClick={onProfileClick}
                  >
                    <User className="h-4 w-4" />
                    Hồ sơ
                  </button>
                )}
                {onLogout && (
                  <>
                    <div className="my-1 border-t border-slate-100" />
                    <button
                      type="button"
                      className="flex w-full items-center gap-2 border-0 bg-transparent px-3 py-2 text-left text-sm text-red-600 hover:bg-red-50"
                      onClick={onLogout}
                    >
                      <LogOut className="h-4 w-4" />
                      Đăng xuất
                    </button>
                  </>
                )}
              </Popover.Popup>
            </Popover.Positioner>
          </Popover.Portal>
        </Popover.Root>
      </Toolbar.End>
    </Toolbar.Root>
  )
}
