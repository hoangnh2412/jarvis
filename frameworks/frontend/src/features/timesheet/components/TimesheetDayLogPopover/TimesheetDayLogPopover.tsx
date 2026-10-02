import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { MoreHorizontal } from 'lucide-react'
import { Button } from 'primereact/button'
import { getTimesheetMessages, type TimesheetLocale } from '../../localization'
import type { TimesheetDayCellAnchor, TimesheetWorkLogEntry } from '../../types'
import { formatDayLabel, formatHours, getAvatarTone, getInitials } from '../../utils'
import { btnOutlinedClass, btnPrimaryClass } from '../fieldStyles'

const btnIconClass =
  'inline-flex size-7 items-center justify-center rounded-md border-0 bg-transparent text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30 disabled:cursor-not-allowed disabled:opacity-60'

const btnMenuItemClass =
  'flex w-full items-center border-0 bg-transparent px-3.5 py-2 text-left text-sm text-slate-700 transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/30'

const btnMenuItemDangerClass =
  'flex w-full items-center border-0 bg-transparent px-3.5 py-2 text-left text-sm text-red-600 transition-colors hover:bg-red-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-600/30'

export type TimesheetDayLogPopoverProps = {
  open: boolean
  anchor: TimesheetDayCellAnchor | null
  entries: TimesheetWorkLogEntry[]
  locale?: TimesheetLocale
  onClose: () => void
  onLogTime?: () => void
}

type LogEntryActionsProps = {
  editLabel: string
  deleteLabel: string
  ariaLabel: string
}

function clamp(value: number, min: number, max: number) {
  return Math.min(Math.max(value, min), max)
}

function LogEntryActions({ editLabel, deleteLabel, ariaLabel }: LogEntryActionsProps) {
  const [open, setOpen] = useState(false)
  const triggerRef = useRef<HTMLSpanElement>(null)
  const [menuStyle, setMenuStyle] = useState<{ top: number; left: number }>({
    top: 0,
    left: 0,
  })

  useLayoutEffect(() => {
    if (!open || !triggerRef.current) return
    const trigger = triggerRef.current.getBoundingClientRect()
    const menuWidth = 144
    const margin = 8
    setMenuStyle({
      top: trigger.bottom + 4,
      left: clamp(
        trigger.right - menuWidth,
        margin,
        window.innerWidth - menuWidth - margin,
      ),
    })
  }, [open])

  useEffect(() => {
    if (!open) return
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.stopPropagation()
        setOpen(false)
      }
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open])

  return (
    <>
      <span ref={triggerRef} className="inline-flex">
        <Button
          type="button"
          unstyled
          className={btnIconClass}
          aria-label={ariaLabel}
          aria-expanded={open}
          onClick={(event: { stopPropagation: () => void }) => {
            event.stopPropagation()
            setOpen((current) => !current)
          }}
        >
          <MoreHorizontal className="size-4" />
        </Button>
      </span>
      {open
        ? createPortal(
            <>
              <Button
                type="button"
                unstyled
                className="kit-timesheet-log-popover__menu-backdrop"
                aria-label={ariaLabel}
                onClick={() => setOpen(false)}
              />
              <div
                className="kit-timesheet-log-popover__menu"
                style={{ top: menuStyle.top, left: menuStyle.left }}
                role="menu"
              >
                <Button
                  type="button"
                  unstyled
                  className={btnMenuItemClass}
                  role="menuitem"
                  onClick={() => setOpen(false)}
                >
                  {editLabel}
                </Button>
                <Button
                  type="button"
                  unstyled
                  className={btnMenuItemDangerClass}
                  role="menuitem"
                  onClick={() => setOpen(false)}
                >
                  {deleteLabel}
                </Button>
              </div>
            </>,
            document.body,
          )
        : null}
    </>
  )
}

export function TimesheetDayLogPopover({
  open,
  anchor,
  entries,
  locale = 'vi',
  onClose,
  onLogTime,
}: TimesheetDayLogPopoverProps) {
  const messages = getTimesheetMessages(locale)
  const cardRef = useRef<HTMLDivElement>(null)
  const [layout, setLayout] = useState<{
    top: number
    left: number
    arrowLeft: number
    placement: 'top' | 'bottom'
    ready: boolean
  }>({ top: 0, left: 0, arrowLeft: 0, placement: 'top', ready: false })

  useLayoutEffect(() => {
    if (!open || !anchor || !cardRef.current) return
    const card = cardRef.current.getBoundingClientRect()
    const margin = 12
    const gap = 12
    const cellCenterX = anchor.left + anchor.width / 2
    const spaceAbove = anchor.top - margin
    const spaceBelow = window.innerHeight - (anchor.top + anchor.height) - margin
    const fitsAbove = spaceAbove >= card.height + gap
    const fitsBelow = spaceBelow >= card.height + gap
    const placement: 'top' | 'bottom' =
      fitsAbove || (!fitsBelow && spaceAbove >= spaceBelow) ? 'top' : 'bottom'
    const left = clamp(
      cellCenterX - card.width / 2,
      margin,
      Math.max(margin, window.innerWidth - card.width - margin),
    )
    const top =
      placement === 'top'
        ? clamp(anchor.top - card.height - gap, margin, window.innerHeight - card.height - margin)
        : clamp(
            anchor.top + anchor.height + gap,
            margin,
            window.innerHeight - card.height - margin,
          )
    const arrowLeft = clamp(cellCenterX - left, 18, Math.max(18, card.width - 18))
    setLayout({ top, left, arrowLeft, placement, ready: true })
  }, [anchor, entries.length, open])

  useEffect(() => {
    if (!open) return
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [onClose, open])

  if (!open || !anchor) return null

  return createPortal(
    <>
      <Button
        type="button"
        unstyled
        className="kit-timesheet-log-popover__backdrop"
        aria-label={messages.detail.close}
        onClick={onClose}
      />
      <div
        className={`kit-timesheet-log-popover kit-timesheet-log-popover--${layout.placement}`}
        style={{
          top: layout.top,
          left: layout.left,
          visibility: layout.ready ? 'visible' : 'hidden',
        }}
        role="dialog"
        aria-modal="true"
        aria-label={messages.detail.title}
        onClick={(event) => event.stopPropagation()}
      >
        <span
          className="kit-timesheet-log-popover__arrow"
          style={{ left: layout.arrowLeft }}
          aria-hidden
        />
        <div ref={cardRef} className="kit-timesheet-log-popover__card">
        <div className="kit-timesheet-log-popover__table-wrap">
          <table className="kit-timesheet-log-popover__table">
            <colgroup>
              <col className="kit-timesheet-log-popover__col-date" />
              <col className="kit-timesheet-log-popover__col-user" />
              <col className="kit-timesheet-log-popover__col-issue" />
              <col className="kit-timesheet-log-popover__col-description" />
              <col className="kit-timesheet-log-popover__col-logged" />
              <col className="kit-timesheet-log-popover__col-actions" />
            </colgroup>
            <thead>
              <tr>
                <th>{messages.detail.date}</th>
                <th>{messages.detail.user}</th>
                <th>{messages.detail.issue}</th>
                <th>{messages.detail.description}</th>
                <th>{messages.detail.logged}</th>
                <th>{messages.detail.actions}</th>
              </tr>
            </thead>
            <tbody>
              {entries.length === 0 ? (
                <tr>
                  <td colSpan={6} className="kit-timesheet-log-popover__empty">
                    {messages.detail.empty}
                  </td>
                </tr>
              ) : (
                entries.map((entry) => {
                  const initials = getInitials(entry.userName)
                  const tone = getAvatarTone(entry.userName)
                  const issueLabel = entry.issueKey
                    ? `${entry.issueKey} - ${entry.issueTitle}`
                    : entry.issueTitle

                  return (
                    <tr key={entry.id}>
                      <td className="kit-timesheet-log-popover__date">
                        {formatDayLabel(entry.date)}
                      </td>
                      <td>
                        <span className="kit-timesheet-log-popover__user">
                          <span
                            className={`kit-timesheet-avatar kit-timesheet-avatar--${tone}`}
                            aria-hidden
                          >
                            {initials}
                          </span>
                          <span className="kit-timesheet-log-popover__user-name">
                            {entry.userName}
                          </span>
                        </span>
                      </td>
                      <td>
                        <span className="kit-timesheet-log-popover__issue" title={issueLabel}>
                          {issueLabel}
                        </span>
                      </td>
                      <td>
                        <div className="kit-timesheet-log-popover__description">
                          {entry.description}
                        </div>
                      </td>
                      <td className="kit-timesheet-log-popover__hours">
                        {formatHours(entry.hours) || '0'}h
                      </td>
                      <td className="kit-timesheet-log-popover__actions">
                        <LogEntryActions
                          ariaLabel={messages.detail.actions}
                          editLabel={messages.detail.edit}
                          deleteLabel={messages.detail.delete}
                        />
                      </td>
                    </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>

        <div className="kit-timesheet-log-popover__footer">
          <Button
            type="button"
            unstyled
            className={btnPrimaryClass}
            onClick={() => onLogTime?.()}
          >
            {messages.detail.logTime}
          </Button>
          <Button type="button" unstyled className={btnOutlinedClass} onClick={onClose}>
            {messages.detail.close}
          </Button>
        </div>
        </div>
      </div>
    </>,
    document.body,
  )
}
