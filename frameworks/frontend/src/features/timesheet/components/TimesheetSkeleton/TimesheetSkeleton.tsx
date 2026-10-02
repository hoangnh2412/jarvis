export function TimesheetSkeleton() {
  return (
    <div className="kit-timesheet-skeleton" aria-hidden>
      <div className="kit-timesheet-skeleton__bar" />
      <div className="kit-timesheet-skeleton__grid">
        {Array.from({ length: 8 }).map((_, index) => (
          <div key={index} className="kit-timesheet-skeleton__row" />
        ))}
      </div>
    </div>
  )
}
