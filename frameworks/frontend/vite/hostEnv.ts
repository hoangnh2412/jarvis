export function requireEnv(
  env: Record<string, string>,
  key: string,
  fallback?: string,
) {
  const value = env[key]
  if (value)
    return value
  if (fallback !== undefined)
    return fallback
  throw new Error(`Missing environment variable ${key}`)
}
