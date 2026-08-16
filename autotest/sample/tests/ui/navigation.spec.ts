import { test } from '../../drivers/playwright/fixtures/sample-test';
import { loadEnv } from '../../config/env';

test.describe('Sample SPA navigation @ui @smoke', () => {
  test('login rồi mở setting, tenant, file @smoke', async ({ app }) => {
    const env = loadEnv();
    await app.loginAs(env.loginEmail, env.loginPassword);
    await app.openSettings();
    await app.openTenants();
    await app.openFiles();
  });
});
