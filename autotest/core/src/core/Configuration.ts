export interface Configuration {
  baseUrl: string;
  auth: Record<string, string>;
  env?: Record<string, string>;
}

export function loadConfiguration(overrides?: Partial<Configuration>): Configuration {
  return {
    baseUrl: overrides?.baseUrl ?? '',
    auth: overrides?.auth ?? {},
    env: overrides?.env ?? {},
  };
}
