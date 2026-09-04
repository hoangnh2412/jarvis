/**
 * Action callbacks kiểu jQuery `$.ajax`:
 * `before` → `onSubmit` (API) → page `onSuccess` (reload/navigate) → `success` → `complete`
 * Lỗi: `error` → `complete`. `before` return `false` → hủy (không gọi onSubmit/success/error/complete).
 */

export type Awaitable<T> = T | PromiseLike<T>

export type ActionProps<TCtx extends object, TPayload = void, TResult = unknown> = {
  /** ≈ `beforeSend` — return `false` để hủy */
  before?: (ctx: TCtx) => Awaitable<false | void | undefined>
  /** ≈ gọi API */
  onSubmit?: (payload: TPayload) => Awaitable<TResult>
  /** ≈ `success` — sau API OK + `onSuccess` của page (reload/navigate) */
  success?: (ctx: TCtx & { result: TResult }) => Awaitable<void>
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

export type HandleActionOptions<
  TCtx extends object,
  TPayload = void,
  TResult = unknown,
> = {
  ctx: TCtx
  callback?: ActionProps<TCtx, TPayload, TResult>
  /** API mặc định khi `callback.onSubmit` không truyền */
  defaultSubmit: (payload: TPayload) => Awaitable<TResult>
  /** Lấy payload từ ctx để gọi `onSubmit` */
  getPayload: (ctx: TCtx) => TPayload
  /**
   * Page-owned side-effect sau API OK, trước `callback.success`
   * (reload list, navigate, reset form…).
   */
  onSuccess?: (ctx: TCtx, result: TResult) => void | Promise<void>
}

export async function handleAction<
  TCtx extends object,
  TPayload = void,
  TResult = unknown,
>(
  options: HandleActionOptions<TCtx, TPayload, TResult>,
): Promise<RunActionOutcome<TResult>> {
  const { ctx, callback, defaultSubmit, getPayload, onSuccess } = options

  const beforeResult = await callback?.before?.(ctx)
  if (beforeResult === false) {
    return { status: 'cancelled' }
  }

  const submit = callback?.onSubmit ?? defaultSubmit
  const payload = getPayload(ctx)

  try {
    const result = await submit(payload)
    await onSuccess?.(ctx, result)
    await callback?.success?.({ ...ctx, result })
    return { status: 'success', result }
  } catch (error) {
    await callback?.error?.({ ...ctx, error })
    throw error
  } finally {
    await callback?.complete?.(ctx)
  }
}
