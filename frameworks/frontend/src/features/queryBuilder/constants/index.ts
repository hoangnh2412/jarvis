export const translations = {
  fields: {
    title: 'Trường',
    placeholderName: '~',
    placeholderLabel: 'Chọn trường…',
    placeholderGroupLabel: '—',
  },
  operators: {
    title: 'Toán tử',
    placeholderName: '~',
    placeholderLabel: 'Toán tử…',
    placeholderGroupLabel: '—',
  },
  values: {
    title: 'Giá trị',
    placeholderName: '~',
    placeholderLabel: 'Giá trị…',
    placeholderGroupLabel: '—',
  },
  value: { title: 'Giá trị' },
  removeRule: { label: 'Xóa', title: 'Xóa điều kiện' },
  removeGroup: { label: 'Xóa', title: 'Xóa nhóm' },
  addRule: { label: 'Điều kiện', title: 'Thêm điều kiện' },
  addGroup: { label: 'Nhóm', title: 'Thêm nhóm điều kiện' },
  combinators: { title: 'Kết hợp' },
  notToggle: { label: 'NOT', title: 'Đảo ngược nhóm' },
} as const

export {
  FAKE_QUERY_BUILDER_FIELDS,
  getFakeQueryBuilderFields,
} from './fakeFields'
export {
  EMPLOYEE_ALLOWED_FIELDS,
  EMPLOYEE_CUSTOM_ALLOWED_FIELDS,
  getQueryBuilderFields,
  callGetQueryBuilderFields,
  resolveCompanyApiMode,
  normalizeQueryBuilderFields,
} from './employeeFields'
export type { QueryBuilderFieldsMode } from './employeeFields'

/** @deprecated Use shared `BASE_URL` from `@jarvis/core` / `lib/http` */
export { BASE_URL as BASE_URL_QUERY_BUILDER } from '../../../lib/http/constants'

/** Relative to shared `BASE_URL` (e.g. `/api/` + path) */
export const API_COMPANY_EMPLOYEES = 'v1/company/employees'
export const API_COMPANY_EMPLOYEES_LIST_CUSTOM =
  'v1/company/employees/list-custom'
