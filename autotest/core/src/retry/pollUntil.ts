export interface PollOptions {
  timeoutMs?: number;
  intervalMs?: number;
  description?: string;
  retryOnError?: (error: unknown) => boolean;
}

export async function pollUntil<T>(
  fn: () => Promise<T | null | undefined>,
  predicate: (value: T) => boolean,
  options: PollOptions = {},
): Promise<T> {
  const timeoutMs = options.timeoutMs ?? 60000;
  const intervalMs = options.intervalMs ?? 1000;
  const deadline = Date.now() + timeoutMs;

  let lastValue: T | null | undefined;

  while (Date.now() < deadline) {
    try {
      lastValue = await fn();
      if (lastValue !== null && lastValue !== undefined && predicate(lastValue)) {
        return lastValue;
      }
    } catch (error) {
      const shouldRetry = options.retryOnError ? options.retryOnError(error) : false;
      if (!shouldRetry) {
        throw error;
      }
    }
    await delay(intervalMs);
  }

  const suffix = options.description ? ` cho "${options.description}"` : '';
  const last = lastValue === undefined ? 'undefined' : JSON.stringify(lastValue);
  throw new Error(`Hết thời gian chờ${suffix} sau ${timeoutMs}ms. Giá trị cuối: ${last}.`);
}

export async function delay(ms: number): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, ms));
}
