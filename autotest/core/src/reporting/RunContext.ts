import fs from 'fs';
import path from 'path';

export function getReportsRoot(): string {
  return path.resolve(process.cwd(), 'reports');
}

let currentRunId: string | null = null;

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

export function createRunId(): string {
  const now = new Date();
  const date = [
    now.getFullYear(),
    pad(now.getMonth() + 1),
    pad(now.getDate()),
  ].join('');
  const time = [pad(now.getHours()), pad(now.getMinutes()), pad(now.getSeconds())].join('');
  const random = Math.random().toString(36).slice(2, 6);
  return `${date}-${time}-${random}`;
}

export function getRunId(): string {
  if (!currentRunId) {
    currentRunId = createRunId();
  }
  return currentRunId;
}

export function getReportDir(runId: string): string {
  return path.join(getReportsRoot(), runId);
}

export function ensureReportDir(runId: string): string {
  const dir = getReportDir(runId);
  fs.mkdirSync(dir, { recursive: true });
  return dir;
}

export interface RunMeta {
  runId: string;
  startedAt: string;
  env: Record<string, string>;
  gitSha?: string;
  durationMs?: number;
  pass?: number;
  fail?: number;
  skipped?: number;
}

export function writeRunMeta(meta: RunMeta): void {
  const dir = ensureReportDir(meta.runId);
  fs.writeFileSync(path.join(dir, 'run-meta.json'), JSON.stringify(meta, null, 2), 'utf8');
}
