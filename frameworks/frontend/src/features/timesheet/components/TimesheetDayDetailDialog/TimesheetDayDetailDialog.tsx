import { FeatureDialog } from '../../../../common/FeatureDialog'
import type { TimesheetWorkItem } from '../../types'
import { getTimesheetMessages, type TimesheetLocale } from '../../localization'
import { formatHours } from '../../utils'

export type TimesheetDayDetailDialogProps = {
  open: boolean
  onClose: () => void
  locale?: TimesheetLocale
  title: string
  description?: string
  hours: number
  workItems: TimesheetWorkItem[]
}

export function TimesheetDayDetailDialog({
  open,
  onClose,
  locale = 'vi',
  title,
  description,
  hours,
  workItems,
}: TimesheetDayDetailDialogProps) {
  const messages = getTimesheetMessages(locale)
  const overtime = hours > 8

  return (
    <FeatureDialog
      open={open}
      onClose={onClose}
      size="sm"
      title={title}
      description={description}
      cancelLabel={messages.detail.close}
    >
      <div className="kit-timesheet-detail">
        <div className="kit-timesheet-detail__summary">
          <span>{messages.detail.total}</span>
          <strong>
            {formatHours(hours) || '0'}h
            {overtime ? (
              <em className="kit-timesheet-detail__over">{messages.detail.overtime}</em>
            ) : null}
          </strong>
        </div>

        {workItems.length === 0 ? (
          <p className="kit-timesheet-detail__empty">{messages.detail.empty}</p>
        ) : (
          <ul className="kit-timesheet-detail__list">
            {workItems.map((item) => (
              <li key={item.id} className="kit-timesheet-detail__item">
                <div className="kit-timesheet-detail__item-main">
                  {item.issueKey ? (
                    <span className="kit-timesheet-detail__key">{item.issueKey}</span>
                  ) : null}
                  <span className="kit-timesheet-detail__title">{item.title}</span>
                </div>
                <span className="kit-timesheet-detail__hours">
                  {formatHours(item.hours) || '0'}h
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </FeatureDialog>
  )
}
