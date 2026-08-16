export interface AuthStrategy {
  applyHeaders(): Record<string, string>;
}
