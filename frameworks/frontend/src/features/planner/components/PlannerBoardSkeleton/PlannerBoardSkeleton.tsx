export type PlannerBoardSkeletonProps = {
  columns?: number
  cardsPerColumn?: number
}

/** Skeleton giữ đúng hình dạng board thật để tránh nhảy layout khi load xong. */
export function PlannerBoardSkeleton({
  columns = 4,
  cardsPerColumn = 3,
}: PlannerBoardSkeletonProps) {
  return (
    <div className="kit-planner-board" aria-hidden>
      {Array.from({ length: columns }).map((_, columnIndex) => (
        <section key={columnIndex} className="kit-planner-column">
          <header className="kit-planner-column__header">
            <div className="kit-planner-column__heading">
              <span className="kit-planner-column__dot" />
              <span className="kit-planner-skeleton h-3.5 w-24 rounded-full" />
            </div>
            <span className="kit-planner-skeleton h-4 w-6 rounded-full" />
          </header>
          <div className="kit-planner-column__body">
            {Array.from({ length: Math.max(1, cardsPerColumn - columnIndex % 2) }).map(
              (__, cardIndex) => (
                <div
                  key={cardIndex}
                  className="kit-planner-skeleton kit-planner-skeleton--card"
                />
              ),
            )}
          </div>
        </section>
      ))}
    </div>
  )
}
