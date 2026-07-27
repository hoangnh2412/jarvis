/**
 * Action callbacks kiểu jQuery `$.ajax`:
 * `before` → `onSubmit` (API) → page `onSuccess` (reload/navigate) → `success` → `complete`
 * Lỗi: `error` → `complete`. `before` return `false` → hủy (không gọi onSubmit/success/error/complete).
 */

type Awaitable<T> = T | PromiseLike<T>

export type ActionProps<TCtx extends object, TPayload = void> = {
  /** ≈ `beforeSend` — return `false` để hủy */
  before?: (ctx: TCtx) => Awaitable<false | void | undefined>
  /** ≈ gọi API */
  onSubmit?: (payload: TPayload) => Awaitable<unknown>
  /** ≈ `success` — sau API OK + `onSuccess` của page (reload/navigate) */
  success?: (ctx: TCtx) => Awaitable<void>
  /** ≈ `error` */
  error?: (ctx: TCtx & { error: unknown }) => Awaitable<void>
  /** ≈ `complete` — luôn chạy sau khi đã gọi onSubmit (OK hoặc lỗi) */
  complete?: (ctx: TCtx) => Awaitable<void>
}

export type RunActionSuccess<TResult> = {
  status: 'success'
  result: TResult
}

export type RunActionCancelled = {
  status: 'cancelled'
}

export type RunActionOutcome<TResult> =
  | RunActionSuccess<TResult>
  | RunActionCancelled

export type HandleActionOptions<TCtx extends object, TPayload = void> = {
  ctx: TCtx
  callback?: ActionProps<TCtx, TPayload>
  /** API mặc định khi `callback.onSubmit` không truyền */
  defaultSubmit: (payload: TPayload) => Awaitable<unknown>
  /** Lấy payload từ ctx để gọi `onSubmit` */
  getPayload: (ctx: TCtx) => TPayload
  /**
   * Page-owned side-effect sau API OK, trước `callback.success`
   * (reload list, navigate, reset form…).
   */
  onSuccess?: (ctx: TCtx) => void | Promise<void>
}

export async function handleAction<TCtx extends object, TPayload = void>(
  options: HandleActionOptions<TCtx, TPayload>,
): Promise<RunActionOutcome<void>> {
  const { ctx, callback, defaultSubmit, getPayload, onSuccess } = options

  const beforeResult = await callback?.before?.(ctx)
  if (beforeResult === false) {
    return { status: 'cancelled' }
  }

  const submit = callback?.onSubmit ?? defaultSubmit
  const payload = getPayload(ctx)

  try {
    await submit(payload)
    await onSuccess?.(ctx)
    await callback?.success?.(ctx)
    return { status: 'success', result: undefined }
  } catch (error) {
    await callback?.error?.({ ...ctx, error })
    throw error
  } finally {
    await callback?.complete?.(ctx)
  }
}
