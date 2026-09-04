import { createTestContext, type TestContext } from './TestContext';
import { loadConfiguration, type Configuration } from './Configuration';
import { createRunId } from '../reporting/RunContext';

export function createTestContextWithDefaults(
  env: Record<string, string> = {},
): TestContext {
  return createTestContext(createRunId(), env);
}

export { createTestContext, type TestContext, loadConfiguration, type Configuration };
