export interface TestContext {
  runId: string;
  env: Record<string, string>;
}

export function createTestContext(
  runId: string,
  env: Record<string, string> = {},
): TestContext {
  return { runId, env };
}
