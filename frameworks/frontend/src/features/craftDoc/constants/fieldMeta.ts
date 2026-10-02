import { FieldType, type FieldTypeValue } from '../types'

export type FieldOption = { label: string; value: string }

export type FieldMeta = {
  label?: string
  type?: FieldTypeValue
  required?: boolean
  group?: string
  description?: string
  placeholderText?: string
  options?: FieldOption[]
}

export const GLOBAL_FIELD_META: Record<string, FieldMeta> = {
  contractId: { label: 'Số văn bản', placeholderText: 'VD: 01/QĐ-NS' },
  day: { label: 'Ngày', type: FieldType.Number, group: 'header' },
  month: { label: 'Tháng', type: FieldType.Number, group: 'header' },
  year: { label: 'Năm', type: FieldType.Number, group: 'header' },
  company: { label: 'Tên công ty' },
  staff: {
    label: 'Họ và tên nhân viên',
    required: true,
    placeholderText: 'VD: Nguyễn Văn A',
  },
  birthday: { label: 'Ngày sinh', type: FieldType.Date },
  staffId: { label: 'Mã số nhân viên', placeholderText: 'VD: NV-001' },
  representative: { label: 'Người ký' },
  contractNumber: {
    label: 'Số hợp đồng',
    group: 'contract',
    placeholderText: 'VD: HĐLĐ-2026/001',
  },
  contractDate: { label: 'Ngày', type: FieldType.Date, group: 'contract' },
  legalEntity: {
    label: 'Bên sử dụng lao động',
    type: FieldType.Select,
    required: true,
    group: 'contract',
    options: [{ label: 'Công ty TNHH ABC Việt Nam', value: 'Công ty TNHH ABC Việt Nam' }],
  },
  employerAddress: { label: 'Địa chỉ công ty', group: 'contract' },
  employerRepresentative: { label: 'Người đại diện', group: 'contract' },
  employerTitle: { label: 'Chức vụ người đại diện', group: 'contract' },
  fullName: {
    label: 'Họ và tên người lao động',
    required: true,
    group: 'employee',
    placeholderText: 'VD: Nguyễn Văn A',
  },
  birthDate: { label: 'Ngày sinh', type: FieldType.Date, group: 'employee' },
  identityNumber: { label: 'Số CCCD/CMND', group: 'employee' },
  identityIssueDate: { label: 'Ngày cấp CCCD', type: FieldType.Date, group: 'employee' },
  identityIssuePlace: { label: 'Nơi cấp CCCD', group: 'employee' },
  permanentAddress: { label: 'Địa chỉ thường trú', group: 'employee' },
  position: {
    label: 'Chức danh chuyên môn',
    required: true,
    group: 'job',
    placeholderText: 'VD: Nhân viên kinh doanh',
  },
  department: { label: 'Phòng ban', group: 'job', placeholderText: 'VD: Phòng Kinh doanh' },
  workLocation: { label: 'Nơi làm việc', group: 'job', placeholderText: 'VD: Hà Nội' },
  contractType: {
    label: 'Loại hợp đồng',
    type: FieldType.Select,
    group: 'terms',
    options: [
      { label: 'Xác định thời hạn', value: 'Xác định thời hạn' },
      { label: 'Không xác định thời hạn', value: 'Không xác định thời hạn' },
    ],
  },
  startDate: { label: 'Ngày bắt đầu', type: FieldType.Date, required: true, group: 'terms' },
  endDate: { label: 'Ngày kết thúc', type: FieldType.Date, group: 'terms' },
  probationDays: {
    label: 'Thời gian thử việc (ngày)',
    type: FieldType.Number,
    group: 'terms',
    placeholderText: 'VD: 60',
  },
  baseSalary: {
    label: 'Mức lương cơ bản (VNĐ)',
    type: FieldType.Currency,
    group: 'terms',
    placeholderText: 'VD: 10.000.000',
  },
  salaryPaymentMethod: { label: 'Hình thức trả lương', group: 'terms' },
  workingHours: { label: 'Thời giờ làm việc', type: FieldType.Textarea, group: 'terms' },
  signPlace: { label: 'Nơi ký', group: 'sign' },
  signDate: { label: 'Ngày ký', type: FieldType.Date, group: 'sign' },
}

export type FieldGroupDef = { id: string; label: string }

export const TEMPLATE_FIELD_GROUPS: Record<string, FieldGroupDef[]> = {
  'labor-contract': [
    { id: 'contract', label: 'Thông tin hợp đồng' },
    { id: 'employee', label: 'Bên lao động' },
    { id: 'job', label: 'Công việc' },
    { id: 'terms', label: 'Điều khoản chính' },
    { id: 'sign', label: 'Ký kết' },
  ],
  'lla-idas': [
    { id: 'general', label: 'Thông tin chung' },
    { id: 'flow1', label: 'Luồng 1 — Đồng bộ nhân sự' },
    { id: 'flow2', label: 'Luồng 2 — Chuyển hướng phiên ký' },
    { id: 'flow3', label: 'Luồng 3 — Trích xuất tài liệu' },
    { id: 'flow4', label: 'Luồng 4 — Ký & đóng dấu' },
  ],
  default: [{ id: 'general', label: 'Thông tin chung' }],
}

export function hasPredefinedFieldGroups(templateId: string): boolean {
  return Object.prototype.hasOwnProperty.call(TEMPLATE_FIELD_GROUPS, templateId)
}

export function getTemplateFieldGroups(templateId: string): FieldGroupDef[] {
  return TEMPLATE_FIELD_GROUPS[templateId] ?? TEMPLATE_FIELD_GROUPS.default
}
