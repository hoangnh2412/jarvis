import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { createRunId, getReportDir, ensureReportDir, writeRunMeta } from '../../src/reporting/RunContext';

test('createRunId trả về format YYYYMMDD-HHMMSS-xxxx', () => {
  const id = createRunId();
  assert.match(id, /^\d{8}-\d{6}-[a-z0-9]{4}$/);
});

test('getReportDir nằm dưới REPORTS_ROOT', () => {
  const dir = getReportDir('20260101');
  assert.ok(dir.includes('reports'));
  assert.ok(dir.endsWith('20260101'));
});

test('ensureReportDir tạo thư mục và trả về đường dẫn', () => {
  const original = process.cwd();
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'core-report-'));
  try {
    process.chdir(tmp);
    const dir = ensureReportDir('run-xyz');
    assert.ok(fs.existsSync(dir));
  } finally {
    process.chdir(original);
    fs.rmSync(tmp, { recursive: true, force: true });
  }
});

test('writeRunMeta ghi run-meta.json hợp lệ', () => {
  const original = process.cwd();
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'core-meta-'));
  try {
    process.chdir(tmp);
    writeRunMeta({ runId: 'meta-1', startedAt: '2026-01-01', env: { API_BASE_URL: 'x' }, gitSha: 'abc' });
    const content = JSON.parse(fs.readFileSync(path.join(tmp, 'reports', 'meta-1', 'run-meta.json'), 'utf8'));
    assert.equal(content.runId, 'meta-1');
    assert.equal(content.gitSha, 'abc');
  } finally {
    process.chdir(original);
    fs.rmSync(tmp, { recursive: true, force: true });
  }
});