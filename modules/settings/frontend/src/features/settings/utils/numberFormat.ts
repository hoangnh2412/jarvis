/**
 * Number UI: dấu chấm ngăn hàng nghìn, dấu phẩy thập phân.
 * Giá trị lưu / gửi API luôn invariant (vd. 1234567.89).
 */

/** Chuẩn hóa chuỗi hiển thị → invariant. */
export function toInvariantNumber(raw: string, decimals: number | null): string | null {
  const trimmed = raw.trim()
  if (!trimmed || trimmed === '-' || trimmed === ',') return null

  const negative = trimmed.startsWith('-')
  const body = (negative ? trimmed.slice(1) : trimmed).trim()

  let normalized: string
  const commaCount = (body.match(/,/g) ?? []).length

  if (commaCount === 1) {
    // 1.234.567,89
    normalized = body.replace(/\./g, '').replace(',', '.')
  } else if (commaCount === 0) {
    // 1.234.567 hoặc 1234.56 (invariant)
    if ((body.match(/\./g) ?? []).length > 1) {
      normalized = body.replace(/\./g, '')
    } else {
      normalized = body
    }
  } else {
    return null
  }

  if (!/^\d+(\.\d+)?$/.test(normalized)) return null

  const value = Number(normalized)
  if (!Number.isFinite(value)) return null

  const sign = negative ? '-' : ''
  if (decimals === null) {
    const [intPart, frac = ''] = normalized.split('.')
    return frac ? `${sign}${intPart}.${frac}` : `${sign}${intPart}`
  }

  if (decimals === 0) return `${sign}${Math.trunc(Math.abs(value))}`
  return `${sign}${Math.abs(value).toFixed(decimals)}`
}

/** Hiển thị từ invariant: 1234567.89 → 1.234.567,89 */
export function formatNumberDisplay(invariant: string, decimals: number | null): string {
  const trimmed = invariant.trim()
  if (!trimmed) return ''

  const negative = trimmed.startsWith('-')
  const abs = negative ? trimmed.slice(1) : trimmed
  if (!/^\d+(\.\d+)?$/.test(abs)) return trimmed

  let intPart: string
  let frac = ''

  if (decimals !== null) {
    const num = Number(trimmed)
    if (!Number.isFinite(num)) return trimmed
    const fixed = Math.abs(num).toFixed(decimals)
    ;[intPart, frac = ''] = fixed.split('.')
  } else {
    ;[intPart, frac = ''] = abs.split('.')
  }

  const withGroup = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, '.')
  const sign = negative ? '-' : ''
  if (decimals === 0) return `${sign}${withGroup}`
  if (frac || (decimals !== null && decimals > 0)) {
    return `${sign}${withGroup},${frac}`
  }
  return `${sign}${withGroup}`
}

/**
 * Trong lúc gõ: chỉ nhận số + dấu phẩy thập phân; tự chèn dấu chấm hàng nghìn.
 */
export function formatNumberWhileTyping(raw: string, decimals: number | null): string {
  const negative = raw.trimStart().startsWith('-')
  const body = raw.replace(/[^\d,]/g, '')

  const commaIdx = body.indexOf(',')
  let intDigits: string
  let fracDigits = ''
  const hasDecimal = commaIdx >= 0 && decimals !== 0

  if (hasDecimal) {
    intDigits = body.slice(0, commaIdx).replace(/\D/g, '')
    fracDigits = body.slice(commaIdx + 1).replace(/\D/g, '')
    if (decimals !== null) fracDigits = fracDigits.slice(0, decimals)
  } else {
    intDigits = body.replace(/\D/g, '')
  }

  const grouped = intDigits.replace(/\B(?=(\d{3})+(?!\d))/g, '.')
  const sign = negative ? '-' : ''
  if (hasDecimal) return `${sign}${grouped},${fracDigits}`
  return `${sign}${grouped}`
}
