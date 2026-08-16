import { test as base } from '@playwright/test';
import { createPlaywrightBrowserDriver, createPlaywrightTransport } from '@jarvis/autotest.playwright';
import { loadEnv } from '../../../config/env';
import { bootstrap, type SampleApp } from '../../../composition/bootstrap';

export const test = base.extend<{ app: SampleApp }>({
  app: async ({ request, page }, use) => {
    const env = loadEnv();
    const transport = createPlaywrightTransport(request);
    const driver = createPlaywrightBrowserDriver(page);
    const app = bootstrap(transport, env, driver);
    await use(app);
  },
});

export { expect } from '@playwright/test';
