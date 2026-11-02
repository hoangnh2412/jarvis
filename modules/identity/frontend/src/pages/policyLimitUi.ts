import { catalogSearchMatches } from '../utils'
import type { AuthorizationPolicyDetail, AuthorizationPolicyRule } from '../services/policies'

export type PolicyLimitMeta = {
  title: string
  detail: string
  summary: string
  conditionLines: string[]
}

const POLICY_SUMMARY_BY_CODE: Record<string, string> = {
  P_SAME_TENANT: 'Chỉ truy cập dữ liệu thuộc cùng tenant (đơn vị thuê) với tài khoản đăng nhập.',
  P_SAME_ORG: 'Chỉ truy cập dữ liệu thuộc cùng đơn vị tổ chức được gán cho user.',
  P_OWNER_ONLY: 'Chỉ xem và thao tác trên bản ghi do chính user tạo.',
  P_SAME_DEPARTMENT: 'Đã ngừng dùng. Policy cũ cùng phòng ban, giữ để dữ liệu đã cài vẫn đọc được.',
  P_SAME_ORGANIZATIONAL_UNIT: 'Chỉ xem dữ liệu thuộc cùng đơn vị tổ chức (organizational unit) với user.',
  P_SAME_LEGAL_ENTITY: 'Chỉ xem dữ liệu thuộc cùng pháp nhân với user.',
  P_WITHIN_OU_SUBTREE: 'Chỉ xem dữ liệu nằm trong nhánh đơn vị của user.',
  P_APPROVE_LIMIT: 'Chỉ thao tác trên bản ghi có giá trị không vượt hạn mức duyệt của user.',
}

const OPERAND_LABELS: Record<string, string> = {
  'Resource.TenantId': 'tenant của bản ghi',
  'User.TenantId': 'tenant của user',
  'Resource.OrgId': 'đơn vị của bản ghi',
  'User.OrgId': 'đơn vị của user',
  'Resource.DepartmentId': 'phòng ban của bản ghi (đã ngừng dùng)',
  'User.DepartmentId': 'phòng ban của user (đã ngừng dùng)',
  'Resource.OrganizationalUnitId': 'đơn vị tổ chức của bản ghi',
  'User.OrganizationalUnitId': 'đơn vị tổ chức của user',
  'Resource.LegalEntityId': 'pháp nhân của bản ghi',
  'User.LegalEntityId': 'pháp nhân của user',
  'Resource.OrganizationalUnitPath': 'đường dẫn đơn vị của bản ghi',
  'User.OrganizationalUnitPath': 'đường dẫn đơn vị của user',
  'Resource.UnitType': 'loại đơn vị của bản ghi',
  'User.UnitType': 'loại đơn vị của user',
  'Resource.CreatedBy': 'người tạo bản ghi',
  'User.Id': 'user hiện tại',
  'Resource.Amount': 'giá trị bản ghi',
  'User.ApproveLimit': 'hạn mức duyệt của user',
}

const OPERATOR_LABELS: Record<string, string> = {
  '==': 'bằng',
  '!=': 'khác',
  '<': 'nhỏ hơn',
  '<=': 'không lớn hơn',
  '>': 'lớn hơn',
  '>=': 'không nhỏ hơn',
  in: 'thuộc danh sách',
  IN: 'thuộc danh sách',
  StartsWith: 'bắt đầu bằng',
  STARTSWITH: 'bắt đầu bằng',
}

function operandLabel(path: string): string {
  const trimmed = path.trim()
  return OPERAND_LABELS[trimmed] ?? trimmed
}

function capitalizeViPhrase(text: string): string {
  const trimmed = text.trim()
  if (!trimmed) return trimmed
  return trimmed.charAt(0).toLocaleUpperCase('vi-VN') + trimmed.slice(1)
}

export function operandDisplayLabel(path: string): string {
  return capitalizeViPhrase(operandLabel(path))
}

export function operatorDisplayLabel(operator: string): string {
  const key = operator.trim()
  const label = OPERATOR_LABELS[key] ?? key
  return capitalizeViPhrase(label)
}

function ruleToHumanLine(rule: AuthorizationPolicyRule): string {
  const left = rule.leftOperand.trim()
  const right = rule.rightOperand.trim()
  const op = rule.operator.trim()

  if (op === '==' && left === 'Resource.DepartmentId' && right === 'User.DepartmentId') {
    return 'Dữ liệu phải cùng phòng ban với user.'
  }
  if (op === '==' && left === 'Resource.OrgId' && right === 'User.OrgId') {
    return 'Dữ liệu phải cùng đơn vị với user.'
  }
  if (op === '==' && left === 'Resource.TenantId' && right === 'User.TenantId') {
    return 'Dữ liệu phải cùng tenant với user.'
  }
  if (op === '==' && left === 'Resource.CreatedBy' && right === 'User.Id') {
    return 'Chỉ bản ghi do user hiện tại tạo.'
  }
  if (op === '<=' && left === 'Resource.Amount' && right === 'User.ApproveLimit') {
    return 'Giá trị bản ghi không được vượt hạn mức duyệt của user.'
  }

  const opLabel = OPERATOR_LABELS[op] ?? op
  const rightLabel = /^\d+(\.\d+)?$/.test(right) ? right : operandLabel(right)
  return `${operandLabel(left)} ${opLabel} ${rightLabel}.`
}

export function policyHumanSummary(policy: AuthorizationPolicyDetail): string {
  const byCode = POLICY_SUMMARY_BY_CODE[policy.code]
  if (byCode) return byCode

  const rules = [...(policy.rules ?? [])].sort((a, b) => (a.order || 0) - (b.order || 0))
  if (rules.length === 0) return 'Chưa có điều kiện lọc — cần bổ sung rule trong cấu hình phạm vi.'

  const lines = rules.map(ruleToHumanLine)
  if (lines.length === 1) return lines[0]
  return `Áp dụng khi thỏa một trong các điều kiện: ${lines.join(' ')}`
}

export function policyConditionLines(rules: readonly AuthorizationPolicyRule[]): string[] {
  if (rules.length === 0) return []
  const ordered = [...rules].sort((a, b) => (a.order || 0) - (b.order || 0))
  return ordered.map((r) => `${r.leftOperand} ${r.operator} ${r.rightOperand}`)
}

/** Hiển thị phạm vi truy cập — chỉ dữ liệu từ API (name, code, rules, resourceType, action). */
export function policyLimitMeta(policy: AuthorizationPolicyDetail): PolicyLimitMeta {
  const title = policy.name?.trim() || policy.code
  const scope = [policy.resourceType, policy.action]
    .map((v) => v?.trim())
    .filter((v): v is string => Boolean(v))
  const detail = scope.length > 0 ? scope.join(' · ') : ''
  const conditionLines = policyConditionLines(policy.rules ?? [])
  const summary = policyHumanSummary(policy)
  return { title, detail, summary, conditionLines }
}

/** Catalog: sửa/xóa chỉ khi không phải hệ thống và chưa gán vai trò. */
export function canMutatePolicyCatalog(policy: AuthorizationPolicyDetail): boolean {
  return !policy.isSystemPolicy && policy.isInUse !== true
}

export function matchesPolicySearch(policy: AuthorizationPolicyDetail, keyword: string): boolean {
  const meta = policyLimitMeta(policy)
  return catalogSearchMatches(
    keyword,
    policy.code,
    policy.name,
    meta.title,
    meta.summary,
    meta.detail,
    ...meta.conditionLines,
  )
}

export type RightInputMode = 'operand' | 'literal'

const ORDERING_OPERATORS = new Set(['<', '<=', '>', '>='])

export function operatorPrefersLiteral(operator: string): boolean {
  return ORDERING_OPERATORS.has(operator)
}

export function inferRightInputMode(rightOperand: string, operandPaths: readonly string[]): RightInputMode {
  if (!rightOperand.trim()) return 'operand'
  return operandPaths.includes(rightOperand) ? 'operand' : 'literal'
}

export type ConditionDraft = {
  key: string
  order: number
  leftOperand: string
  operator: string
  rightOperand: string
  rightMode: RightInputMode
}

export function newConditionDraft(order: number): ConditionDraft {
  return {
    key: `cond-${order}-${Math.random().toString(36).slice(2, 8)}`,
    order,
    leftOperand: 'Resource.OrgId',
    operator: '==',
    rightOperand: 'User.OrgId',
    rightMode: 'operand',
  }
}

export function policyToDraft(
  policy: AuthorizationPolicyDetail | null,
  operandPaths: readonly string[] = [],
): {
  id: string | null
  code: string
  name: string
  resourceType: string
  action: string
  isSystemPolicy: boolean
  rules: ConditionDraft[]
} {
  if (!policy) {
    return {
      id: null,
      code: '',
      name: '',
      resourceType: '',
      action: '',
      isSystemPolicy: false,
      rules: [newConditionDraft(1)],
    }
  }
  return {
    id: policy.id,
    code: policy.code,
    name: policy.name,
    resourceType: policy.resourceType ?? '',
    action: policy.action ?? '',
    isSystemPolicy: policy.isSystemPolicy,
    rules:
      policy.rules.length > 0
        ? policy.rules.map((rule, index) => ({
          key: rule.id || `cond-${index}`,
          order: rule.order || index + 1,
          leftOperand: rule.leftOperand,
          operator: rule.operator,
          rightOperand: rule.rightOperand,
          rightMode: inferRightInputMode(rule.rightOperand, operandPaths),
        }))
        : [newConditionDraft(1)],
  }
}
