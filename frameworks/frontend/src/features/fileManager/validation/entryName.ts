import { z } from 'zod'

/** Message validate — chỉ bắt buộc nhập tên */
export const ENTRY_NAME_MESSAGES = {
  required: 'Vui lòng nhập tên',
  invalid: 'Tên không hợp lệ',
} as const

/** Chỉ validate không để trống. */
export const entryNameSchema = z
  .string()
  .trim()
  .min(1, ENTRY_NAME_MESSAGES.required)

export type EntryNameData = z.infer<typeof entryNameSchema>

/** Validate tên — trả về message lỗi hoặc `null` nếu hợp lệ. */
export function getEntryNameError(name: string): string | null {
  const result = entryNameSchema.safeParse(name)
  if (result.success) return null
  return result.error.issues[0]?.message ?? ENTRY_NAME_MESSAGES.invalid
}

/** Throw nếu tên không hợp lệ — dùng trong mock / service. */
export function assertValidEntryName(name: string): string {
  return entryNameSchema.parse(name)
}
