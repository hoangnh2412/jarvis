import type { DocumentField } from '../types'

export const DYNAMIC_GROUP_LABELS: Record<string, string> = {
  header: 'Thông tin văn bản',
  general: 'Thông tin chung',
  employee: 'Thông tin nhân sự',
  job: 'Công việc & bổ nhiệm',
  terms: 'Điều khoản',
  sign: 'Ký duyệt',
  flow1: 'Luồng 1',
  flow2: 'Luồng 2',
  flow3: 'Luồng 3',
  flow4: 'Luồng 4',
}

const DYNAMIC_GROUP_ORDER = [
  'header',
  'general',
  'employee',
  'job',
  'terms',
  'sign',
  'flow1',
  'flow2',
  'flow3',
  'flow4',
]

/** Suy luận nhóm field từ tên placeholder — dùng cho mẫu upload / chưa cấu hình sẵn. */
export function inferFieldGroup(fieldName: string): string {
  const key = fieldName.trim()
  if (!key) return 'general'

  const flowMatch = key.match(/^flow([1-4])/i)
  if (flowMatch) return `flow${flowMatch[1]}`

  const n = key.toLowerCase()

  if (
    /^(sign|representative|approver|director|giamdoc|nguoiky|authorized|kyduyet)/.test(n)
  ) {
    return 'sign'
  }

  if (
    /^(staff|employee|fullname|full_name|birth|birthday|birthdate|identity|cccd|cmnd|staffid|employeeid|hoten|nhanvien|permanentaddress|diachi|ngaysinh|socccd)/.test(
      n,
    )
  ) {
    return 'employee'
  }

  if (
    /^(position|department|worklocation|jobtitle|chucvu|phongban|bophan|role|chucdanh|vitri)/.test(
      n,
    )
  ) {
    return 'job'
  }

  if (
    /^(contractdate|effectivedate|ngayhieuluc|startdate|enddate|probation|salary|basesalary|contracttype|workinghours|luong|thoihan|dieukhoan|quyenloi|trachnhiem)/.test(
      n,
    )
  ) {
    if (/^(startdate|enddate|probation|salary|basesalary|contracttype|workinghours|luong|thoihan|dieukhoan|quyenloi|trachnhiem)/.test(n)) {
      return 'terms'
    }
    return 'job'
  }

  if (
    /^(contractid|contractnumber|decisionnumber|documentnumber|documenttitle|intro|introparagraph|so|number|day|month|year|company|legalentity|employer|employeraddress|employerrepresentative|employertitle|signplace|signdate|ngay|thang|nam|congty|quyetdinh|partner|platform)/.test(
      n,
    )
  ) {
    return 'header'
  }

  return 'general'
}

export type DynamicFieldGroup = {
  id: string
  label: string
  fields: DocumentField[]
}

function compareDynamicGroupOrder(a: string, b: string): number {
  const ai = DYNAMIC_GROUP_ORDER.indexOf(a)
  const bi = DYNAMIC_GROUP_ORDER.indexOf(b)
  if (ai === -1 && bi === -1) return a.localeCompare(b)
  if (ai === -1) return 1
  if (bi === -1) return -1
  return ai - bi
}

function groupIdToLabel(id: string): string {
  return (
    DYNAMIC_GROUP_LABELS[id] ??
    id
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim()
      .replace(/^./, (char) => char.toUpperCase())
  )
}

/** Gom field theo nhóm suy luận — thứ tự ổn định, label tiếng Việt. */
export function buildDynamicFieldGroups(fields: DocumentField[]): DynamicFieldGroup[] {
  const buckets = new Map<string, DocumentField[]>()

  for (const field of fields) {
    const groupId = field.group ?? inferFieldGroup(field.name)
    const bucket = buckets.get(groupId) ?? []
    bucket.push(field)
    buckets.set(groupId, bucket)
  }

  return [...buckets.keys()]
    .sort(compareDynamicGroupOrder)
    .map((id) => ({
      id,
      label: groupIdToLabel(id),
      fields: buckets.get(id) ?? [],
    }))
    .filter((group) => group.fields.length > 0)
}
