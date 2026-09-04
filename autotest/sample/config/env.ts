export interface SampleEnv {
  baseUrl: string;
  webUrl: string;
  apiKey: string;
  apiKeyHeader: string;
  loginEmail: string;
  loginPassword: string;
}

export function loadEnv(): SampleEnv {
  const baseUrl = process.env.BASE_URL ?? 'http://127.0.0.1:5167';
  return {
    baseUrl,
    webUrl: process.env.WEB_URL ?? baseUrl,
    apiKey: process.env.API_KEY ?? '',
    apiKeyHeader: process.env.API_KEY_HEADER ?? 'X-API-KEY',
    loginEmail: process.env.LOGIN_EMAIL ?? 'admin@gmail.com',
    loginPassword: process.env.LOGIN_PASSWORD ?? 'Admin@123',
  };
}
