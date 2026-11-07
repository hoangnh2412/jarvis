export type TimesheetMessages = {
  page: {
    title: string
    description: string
    reports: string
    loggedTime: string
    save: string
    export: string
    filterBy: string
    groupBy: string
    grain: string
    grainDay: string
    grainWeek: string
    loading: string
    empty: string
    emptyHint: string
    total: string
    saved: string
    exported: string
    error: string
  }
  detail: {
    title: string
    total: string
    overtime: string
    empty: string
    close: string
    logTime: string
    date: string
    user: string
    issue: string
    description: string
    logged: string
    actions: string
    edit: string
    delete: string
  }
  columns: {
    name: string
    key: string
    logged: string
  }
  group: {
    chipProject: string
    chipUser: string
    chipKey: string
    chipNone: string
    active: string
    inactive: string
    level1: string
    level2: string
  }
}

export const timesheetMessagesVi: TimesheetMessages = {
  page: {
    title: 'Logged Time',
    description: 'Báo cáo giờ đã log theo project và người dùng.',
    reports: 'Reports',
    loggedTime: 'Logged Time',
    save: 'Save',
    export: 'Export',
    filterBy: 'Filter by',
    groupBy: 'Group by',
    grain: 'Grid view',
    grainDay: 'Grid view (Days)',
    grainWeek: 'Grid view (Weeks)',
    loading: 'Đang tải…',
    empty: 'Không có dữ liệu nào trong khoảng này',
    emptyHint: 'Đổi khoảng ngày hoặc bỏ bộ lọc.',
    total: 'Total',
    saved: 'Đã lưu timesheet',
    exported: 'Đã xuất CSV',
    error: 'Thao tác thất bại',
  },
  detail: {
    title: 'Logged work',
    total: 'Total',
    overtime: 'Over 8h',
    empty: 'No work items logged for this day.',
    close: 'Close',
    logTime: 'Log Time',
    date: 'Date',
    user: 'User',
    issue: 'Issue',
    description: 'Description',
    logged: 'Logged',
    actions: 'Actions',
    edit: 'Edit',
    delete: 'Delete',
  },
  columns: {
    name: 'Project / User',
    key: 'Key',
    logged: 'Logged',
  },
  group: {
    chipProject: 'Project',
    chipUser: 'User',
    chipKey: 'Key',
    chipNone: 'None',
    active: 'Group by',
    inactive: 'Inactive',
    level1: '1.',
    level2: '2.',
  },
}

export const timesheetMessagesEn: TimesheetMessages = {
  page: {
    title: 'Logged Time',
    description: 'Logged hours grouped by project and user.',
    reports: 'Reports',
    loggedTime: 'Logged Time',
    save: 'Save',
    export: 'Export',
    filterBy: 'Filter by',
    groupBy: 'Group by',
    grain: 'Grid view',
    grainDay: 'Grid view (Days)',
    grainWeek: 'Grid view (Weeks)',
    loading: 'Loading…',
    empty: 'No hours in this range',
    emptyHint: 'Try another date range or clear the filter.',
    total: 'Total',
    saved: 'Timesheet saved',
    exported: 'CSV exported',
    error: 'Operation failed',
  },
  detail: {
    title: 'Logged work',
    total: 'Total',
    overtime: 'Over 8h',
    empty: 'No work items logged for this day.',
    close: 'Close',
    logTime: 'Log Time',
    date: 'Date',
    user: 'User',
    issue: 'Issue',
    description: 'Description',
    logged: 'Logged',
    actions: 'Actions',
    edit: 'Edit',
    delete: 'Delete',
  },
  columns: {
    name: 'Project / User',
    key: 'Key',
    logged: 'Logged',
  },
  group: {
    chipProject: 'Project',
    chipUser: 'User',
    chipKey: 'Key',
    chipNone: 'None',
    active: 'Group by',
    inactive: 'Inactive',
    level1: '1.',
    level2: '2.',
  },
}

export type TimesheetLocale = 'vi' | 'en'

const messages: Record<TimesheetLocale, TimesheetMessages> = {
  vi: timesheetMessagesVi,
  en: timesheetMessagesEn,
}

export function getTimesheetMessages(locale: TimesheetLocale = 'vi') {
  return messages[locale] ?? timesheetMessagesVi
}

export { timesheetMessagesVi as timesheetMessages }
