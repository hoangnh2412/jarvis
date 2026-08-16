import { test, expect } from '../../drivers/playwright/fixtures/sample-test';

test.describe('Sample API @api @smoke', () => {
  test('whoami anonymous @smoke', async ({ app }) => {
    const who = await app.probeWhoAmI();
    expect(who.authenticated).toBe(false);
  });

  test('setting groups @smoke', async ({ app }) => {
    const groups = await app.listSettingGroups();
    expect(Array.isArray(groups)).toBe(true);
  });
});
