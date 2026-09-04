import type { IHttpTransport } from '../ports/IHttpTransport';

export interface HarnessDependencies {
  transport: IHttpTransport;
  baseUrl: string;
  authHeaders: Record<string, string>;
}

export function createTestHarness<TRegistry>(
  deps: HarnessDependencies,
  buildRegistry: (deps: HarnessDependencies) => TRegistry,
): TRegistry {
  return buildRegistry(deps);
}
