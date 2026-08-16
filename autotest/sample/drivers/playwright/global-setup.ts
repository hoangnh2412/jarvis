import { createRunId, ensureReportDir, writeRunMeta } from '@jarvis/autotest';
import type { FullConfig } from '@playwright/test';

export default async function globalSetup(_config: FullConfig): Promise<void> {
  const runId = process.env.RUN_ID ?? createRunId();
  process.env.RUN_ID = runId;
  ensureReportDir(runId);
  writeRunMeta({
    runId,
    startedAt: new Date().toISOString(),
    env: {
      BASE_URL: process.env.BASE_URL ?? '',
      WEB_URL: process.env.WEB_URL ?? '',
    },
  });
}
