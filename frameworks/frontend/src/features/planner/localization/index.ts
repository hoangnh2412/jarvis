export type PlannerMessages = {
  page: {
    title: string
    description: string
    kanban: string
    calendar: string
    timeline: string
    unassigned: string
    projectTimeline: string
    taskName: string
    create: string
    addItem: string
    emptyColumn: string
    emptyColumnHint: string
    loading: string
    search: string
    priorityFilter: string
    allPriorities: string
    noResults: string
    noResultsHint: string
    clearFilters: string
    overdue: string
    itemsCount: (count: number) => string
    tooltipAssignee: (name: string) => string
    tooltipDate: (range: string) => string
    moreDetails: string
    detailCreateHint: string
    detailEditHint: string
  }
  form: {
    createTitle: string
    editTitle: string
    title: string
    description: string
    column: string
    start: string
    end: string
    allDay: string
    allDayHint: string
    priority: string
    assignee: string
    dates: string
    taskType: string
    resizePanel: string
    titlePlaceholder: string
    descriptionPlaceholder: string
    assigneePlaceholder: string
    titleRequired: string
    cancel: string
    save: string
    delete: string
    priorityLow: string
    priorityMedium: string
    priorityHigh: string
    scheduleSection: string
    detailSection: string
  }
  toast: {
    createSuccess: string
    updateSuccess: string
    moveSuccess: string
    rescheduleSuccess: string
    deleteSuccess: string
    error: string
  }
  dialog: {
    deleteTitle: string
    deleteConfirm: (title: string) => string
  }
}

export const plannerMessagesVi: PlannerMessages = {
  page: {
    title: 'Planner',
    description:
      'Kanban, lịch và timeline. Kéo thả để cập nhật trạng thái hoặc thời gian.',
    kanban: 'Kanban',
    calendar: 'Calendar',
    timeline: 'Timeline',
    unassigned: 'Chưa gán',
    projectTimeline: 'Project Timeline',
    taskName: 'TASK NAME',
    create: 'Tạo mục',
    addItem: 'Thêm mục',
    emptyColumn: 'Chưa có mục nào',
    emptyColumnHint: 'Kéo thả card vào đây hoặc bấm Thêm mục.',
    loading: 'Đang tải…',
    search: 'Tìm theo tiêu đề, người phụ trách…',
    priorityFilter: 'Độ ưu tiên',
    allPriorities: 'Tất cả độ ưu tiên',
    noResults: 'Không có mục nào khớp',
    noResultsHint: 'Thử đổi từ khoá hoặc bỏ bộ lọc độ ưu tiên.',
    clearFilters: 'Bỏ bộ lọc',
    overdue: 'Quá hạn',
    itemsCount: (count: number) => `${count} mục`,
    tooltipAssignee: (name: string) => `Người phụ trách: ${name}`,
    tooltipDate: (range: string) => `Ngày: ${range}`,
    moreDetails: 'Chi tiết hơn',
    detailCreateHint: 'Nhập thông tin task ở panel bên phải.',
    detailEditHint: 'Chỉnh sửa task ở panel bên phải.',
  },
  form: {
    createTitle: 'Tạo mục',
    editTitle: 'Chỉnh sửa mục',
    title: 'Tiêu đề',
    description: 'Mô tả',
    column: 'Trạng thái',
    start: 'Bắt đầu',
    end: 'Kết thúc',
    allDay: 'Cả ngày',
    allDayHint: 'Không dùng giờ cụ thể',
    priority: 'Độ ưu tiên',
    assignee: 'Người phụ trách',
    dates: 'Thời gian',
    taskType: 'Task',
    resizePanel: 'Kéo để đổi độ rộng panel',
    titlePlaceholder: 'Nhập tiêu đề task…',
    descriptionPlaceholder: 'Thêm mô tả…',
    assigneePlaceholder: 'Thêm người phụ trách…',
    titleRequired: 'Vui lòng nhập tiêu đề',
    cancel: 'Hủy',
    save: 'Lưu',
    delete: 'Xoá',
    priorityLow: 'Thấp',
    priorityMedium: 'Trung bình',
    priorityHigh: 'Cao',
    scheduleSection: 'Thời gian',
    detailSection: 'Phân loại',
  },
  toast: {
    createSuccess: 'Đã tạo mục',
    updateSuccess: 'Đã cập nhật mục',
    moveSuccess: 'Đã chuyển trạng thái',
    rescheduleSuccess: 'Đã đổi lịch',
    deleteSuccess: 'Đã xoá mục',
    error: 'Thao tác thất bại',
  },
  dialog: {
    deleteTitle: 'Xoá mục?',
    deleteConfirm: (title: string) => `"${title}" sẽ bị xoá khỏi planner.`,
  },
}

export const plannerMessagesEn: PlannerMessages = {
  page: {
    title: 'Planner',
    description: 'Kanban, calendar, and timeline. Drag to update status or schedule.',
    kanban: 'Kanban',
    calendar: 'Calendar',
    timeline: 'Timeline',
    unassigned: 'Unassigned',
    projectTimeline: 'Project Timeline',
    taskName: 'TASK NAME',
    create: 'New item',
    addItem: 'Add item',
    emptyColumn: 'No items yet',
    emptyColumnHint: 'Drop a card here or use Add item.',
    loading: 'Loading…',
    search: 'Search by title, assignee…',
    priorityFilter: 'Priority',
    allPriorities: 'All priorities',
    noResults: 'No matching items',
    noResultsHint: 'Try another keyword or clear the priority filter.',
    clearFilters: 'Clear filters',
    overdue: 'Overdue',
    itemsCount: (count: number) => `${count} items`,
    tooltipAssignee: (name: string) => `Assignee: ${name}`,
    tooltipDate: (range: string) => `Date: ${range}`,
    moreDetails: 'More details',
    detailCreateHint: 'Fill in task details in the right panel.',
    detailEditHint: 'Edit this task in the right panel.',
  },
  form: {
    createTitle: 'New item',
    editTitle: 'Edit item',
    title: 'Title',
    description: 'Description',
    column: 'Status',
    start: 'Start',
    end: 'End',
    allDay: 'All day',
    allDayHint: 'No specific time',
    priority: 'Priority',
    assignee: 'Assignees',
    dates: 'Dates',
    taskType: 'Task',
    resizePanel: 'Drag to resize panel',
    titlePlaceholder: 'Task name…',
    descriptionPlaceholder: 'Add description…',
    assigneePlaceholder: 'Add assignee…',
    titleRequired: 'Title is required',
    cancel: 'Cancel',
    save: 'Save',
    delete: 'Delete',
    priorityLow: 'Low',
    priorityMedium: 'Normal',
    priorityHigh: 'High',
    scheduleSection: 'Schedule',
    detailSection: 'Classification',
  },
  toast: {
    createSuccess: 'Item created',
    updateSuccess: 'Item updated',
    moveSuccess: 'Status updated',
    rescheduleSuccess: 'Rescheduled',
    deleteSuccess: 'Item deleted',
    error: 'Operation failed',
  },
  dialog: {
    deleteTitle: 'Delete item?',
    deleteConfirm: (title: string) =>
      `"${title}" will be removed from the planner.`,
  },
}

export type PlannerLocale = 'vi' | 'en'

const messages: Record<PlannerLocale, PlannerMessages> = {
  vi: plannerMessagesVi,
  en: plannerMessagesEn,
}

export function getPlannerMessages(locale: PlannerLocale = 'vi') {
  return messages[locale] ?? plannerMessagesVi
}

export { plannerMessagesVi as plannerMessages }
