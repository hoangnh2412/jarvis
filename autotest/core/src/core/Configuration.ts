export interface Configuration {
  baseUrl: string;
  auth: Record<string, string>;
  env?: Record<string, string>;
}

const BASE_URL_KEYS = ['AUTOTEST_BASE_URL', 'BASE_URL', 'WEB_URL'] as const;

function readBaseUrlFromEnv(): string {
  for (const key of BASE_URL_KEYS) {
    const value = process.env[key]?.trim();
    if (value) {
      return value;
    }
  }
  return '';
}

/**
 * Nạp cấu hình: overrides > biến môi trường (AUTOTEST_BASE_URL, BASE_URL, WEB_URL) > mặc định rỗng.
 * Trước khi chạy suite production gọi {@link assertConfigurationReady}.
 */
export function loadConfiguration(overrides?: Partial<Configuration>): Configuration {
  return {
    baseUrl: overrides?.baseUrl ?? readBaseUrlFromEnv(),
    auth: overrides?.auth ?? {},
    env: overrides?.env ?? {},
  };
}

export function assertConfigurationReady(config: Configuration): void {
  if (!config.baseUrl?.trim()) {
    throw new Error(
      `Thiếu baseUrl. Đặt một trong: ${BASE_URL_KEYS.join(', ')} hoặc truyền overrides.baseUrl.`,
    );
  }
}
