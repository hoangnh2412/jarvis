import type { AuthStrategy } from './AuthStrategy';

export class ApiKeyAuth implements AuthStrategy {
  constructor(
    private readonly apiKey: string,
    private readonly headerName = 'X-KEY-API',
  ) {
    if (!apiKey) {
      throw new Error('ApiKeyAuth cần apiKey khác rỗng');
    }
  }

  applyHeaders(): Record<string, string> {
    return { [this.headerName]: this.apiKey };
  }
}
