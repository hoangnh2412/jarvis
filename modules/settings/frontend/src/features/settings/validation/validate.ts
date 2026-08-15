import { parseSettingOptions, type SettingFormItem } from '../types'
import { splitMultiSelectValue } from '../utils/dateFormat'
import { validateImageDataUrl } from '../utils/imageConstraints'
import { toInvariantNumber } from '../utils/numberFormat'
import { getEmailRegex, getMaxLength, getMaxTextareaRows, getNumberDecimals, countTextLines } from '../utils/typeOptions'

export type FieldValidationError = {
  key: string
  name: string
  message: string
}

const DATE_RE = /^\d{4}-\d{2}-\d{2}$/
const DATETIME_RE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2})?$/

export function validateSettingValue(
  item: SettingFormItem,
  value: string,
): string | null {
  if (item.isReadOnly) return null

  const type = item.type.toLowerCase()
  const trimmed = value.trim()

  if (type === 'text') {
    const maxLength = getMaxLength(item.options)
    if (maxLength !== null && value.length > maxLength) {
      return `Tối đa ${maxLength} ký tự.`
    }
    return null
  }

  if (type === 'textarea') {
    const maxLength = getMaxLength(item.options)
    if (maxLength !== null && value.length > maxLength) {
      return `Tối đa ${maxLength} ký tự.`
    }
    const maxRows = getMaxTextareaRows(item.options)
    if (maxRows !== null && countTextLines(value) > maxRows) {
      return `Tối đa ${maxRows} dòng.`
    }
    return null
  }

  if (type === 'email') {
    // Optional: empty allowed (e.g. Email.FromAddress before SMTP is configured).
    if (trimmed === '') return null
    const maxLength = getMaxLength(item.options)
    if (maxLength !== null && trimmed.length > maxLength) {
      return `Tối đa ${maxLength} ký tự.`
    }
    try {
      const pattern = getEmailRegex(item.options)
      if (!new RegExp(pattern, 'i').test(trimmed)) {
        return 'Địa chỉ email không hợp lệ.'
      }
    } catch {
      return 'Regex email trong Options không hợp lệ.'
    }
    return null
  }

  if (type === 'number') {
    if (trimmed === '') return 'Giá trị số không được để trống.'
    const decimals = getNumberDecimals(item.options)
    const invariant = toInvariantNumber(trimmed, decimals)
    if (invariant === null || !Number.isFinite(Number(invariant))) {
      return 'Giá trị phải là số hợp lệ.'
    }
    if (decimals !== null) {
      const frac = invariant.split('.')[1] ?? ''
      if (frac.length > decimals) {
        return `Tối đa ${decimals} chữ số sau dấu phẩy.`
      }
    }
    return null
  }

  if (type === 'checkbox' || type === 'switch') {
    const normalized = trimmed.toLowerCase()
    if (normalized !== 'true' && normalized !== 'false') {
      return 'Giá trị phải là true hoặc false.'
    }
    return null
  }

  if (type === 'combobox' || type === 'radio') {
    const options = parseSettingOptions(item.options)
    if (!options.length) return null
    if (!trimmed) return 'Vui lòng chọn một giá trị.'
    if (!options.some((option) => option.value === value)) {
      return 'Giá trị không nằm trong danh sách tùy chọn.'
    }
    return null
  }

  if (type === 'multiselect') {
    const options = parseSettingOptions(item.options)
    if (!options.length) return null
    const selected = splitMultiSelectValue(value)
    if (!selected.length) return null
    const allowed = new Set(options.map((option) => option.value))
    if (selected.some((entry) => !allowed.has(entry))) {
      return 'Có giá trị không nằm trong danh sách tùy chọn.'
    }
    return null
  }

  if (type === 'date') {
    if (!trimmed) return 'Vui lòng chọn ngày.'
    if (!DATE_RE.test(trimmed) || Number.isNaN(Date.parse(trimmed))) {
      return 'Ngày không hợp lệ.'
    }
    return null
  }

  if (type === 'datetime') {
    if (!trimmed) return 'Vui lòng chọn ngày giờ.'
    if (!DATETIME_RE.test(trimmed) || Number.isNaN(Date.parse(trimmed))) {
      return 'Ngày giờ không hợp lệ.'
    }
    return null
  }

  if (type === 'image') {
    return validateImageDataUrl(value, item.options)
  }

  return null
}

export function validateSettingForm(
  items: SettingFormItem[],
  values: Record<string, string>,
): FieldValidationError[] {
  const errors: FieldValidationError[] = []

  for (const item of items) {
    if (item.isReadOnly) continue
    const message = validateSettingValue(item, values[item.key] ?? '')
    if (message) {
      errors.push({
        key: item.key,
        name: item.name || item.key,
        message,
      })
    }
  }

  return errors
}
