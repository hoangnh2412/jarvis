/** Bỏ dấu tiếng Việt để tìm kiếm không phụ thuộc dấu. */
export function catalogSearchNormalize(value: string | null | undefined): string {
  return (value ?? '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D')
    .toLocaleLowerCase('vi-VN')
    .trim()
}

export function catalogSearchMatches(keyword: string, ...fields: (string | null | undefined)[]): boolean {
  const normalized = catalogSearchNormalize(keyword)
  if (!normalized) return true
  return fields.some((field) => catalogSearchNormalize(field).includes(normalized))
}

export function searchNormalize(value: string): string {
  return catalogSearchNormalize(value)
}
