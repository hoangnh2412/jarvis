import type { AuthStrategy } from './AuthStrategy';

export class BearerTokenAuth implements AuthStrategy {
  constructor(private readonly accessToken: string) {
    if (!accessToken?.trim()) {
      throw new Error('BearerTokenAuth cần accessToken khác rỗng');
    }
  }

  applyHeaders(): Record<string, string> {
    return { Authorization: `Bearer ${this.accessToken.trim()}` };
  }
}
