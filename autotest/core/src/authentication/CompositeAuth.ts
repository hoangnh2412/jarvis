import type { AuthStrategy } from './AuthStrategy';

/** Gộp nhiều chiến lược (vd. Bearer + tenant header tùy biến). Header sau ghi đè header trùng key. */
export class CompositeAuth implements AuthStrategy {
  constructor(private readonly strategies: AuthStrategy[]) {
    if (!strategies.length) {
      throw new Error('CompositeAuth cần ít nhất một AuthStrategy');
    }
  }

  applyHeaders(): Record<string, string> {
    return this.strategies.reduce<Record<string, string>>((acc, strategy) => {
      return { ...acc, ...strategy.applyHeaders() };
    }, {});
  }
}
