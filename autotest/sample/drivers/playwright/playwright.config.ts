import { defineConfig, devices } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';
import { getReportDir } from '@jarvis/autotest';

const sampleRoot = path.resolve(__dirname, '../..');
const envFile = path.join(sampleRoot, '.env');
if (fs.existsSync(envFile)) {
  const text = fs.readFileSync(envFile, 'utf8');
  for (const line of text.split('\n')) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) continue;
    const eq = trimmed.indexOf('=');
    if (eq < 1) continue;
    const key = trimmed.slice(0, eq).trim();
    const value = trimmed.slice(eq + 1).trim();
    if (!process.env[key]) {
      process.env[key] = value;
    }
  }
}

const baseURL = process.env.WEB_URL ?? process.env.BASE_URL ?? 'http://127.0.0.1:5167';
const runId = process.env.RUN_ID ?? `local-${Date.now()}`;
const reportDir = getReportDir(runId);

export default defineConfig({
  testDir: path.join(sampleRoot, 'tests'),
  fullyParallel: false,
  workers: 1,
  timeout: 60_000,
  globalSetup: path.join(sampleRoot, 'drivers/playwright/global-setup.ts'),
  outputDir: path.join(reportDir, 'test-results'),
  reporter: [
    ['list'],
    ['html', { outputFolder: path.join(reportDir, 'playwright-report'), open: 'never' }],
    ['json', { outputFile: path.join(reportDir, 'results.json') }],
  ],
  use: {
    baseURL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
