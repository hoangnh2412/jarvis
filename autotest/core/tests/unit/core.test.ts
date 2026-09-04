import { test } from 'node:test';
import assert from 'node:assert/strict';
import { ApiKeyAuth } from '../../src/authentication/ApiKeyAuth';
import { ConsoleLogger } from '../../src/logging/ConsoleLogger';
import { createTestContext } from '../../src/core/TestContext';
import { createTestContextWithDefaults } from '../../src/core/createTestContext';
import { loadConfiguration } from '../../src/core/Configuration';
import { createTestHarness } from '../../src/composition/createTestHarness';
import type { IHttpTransport } from '../../src/ports/IHttpTransport';

test('ApiKeyAuth trả header X-KEY-API mặc định', () => {
  const headers = new ApiKeyAuth('secret').applyHeaders();
  assert.deepEqual(headers, { 'X-KEY-API': 'secret' });
});

test('ApiKeyAuth dùng header name tùy biến', () => {
  const headers = new ApiKeyAuth('k', 'x-api-key').applyHeaders();
  assert.deepEqual(headers, { 'x-api-key': 'k' });
});

test('ApiKeyAuth ném khi không có apiKey', () => {
  assert.throws(() => new ApiKeyAuth(''), /apiKey/);
});

test('ConsoleLogger ghi log theo prefix', () => {
  const logs: string[] = [];
  const original = console.log;
  console.log = (msg: string) => logs.push(msg);
  try {
    new ConsoleLogger('tv').info('hello', { a: 1 });
  } finally {
    console.log = original;
  }
  assert.equal(logs.length, 1);
  assert.match(logs[0], /\[tv\]\[INFO\] hello/);
  assert.match(logs[0], /"a":1/);
});

test('createTestContext mang runId và env', () => {
  const ctx = createTestContext('run-1', { KEY: 'v' });
  assert.equal(ctx.runId, 'run-1');
  assert.equal(ctx.env.KEY, 'v');
});

test('createTestContextWithDefaults sinh runId mặc định theo format', () => {
  const ctx = createTestContextWithDefaults({});
  assert.match(ctx.runId, /^\d{8}-\d{6}-[a-z0-9]{4}$/);
});

test('loadConfiguration merge overrides', () => {
  const config = loadConfiguration({ baseUrl: 'http://x' });
  assert.equal(config.baseUrl, 'http://x');
  assert.deepEqual(config.auth, {});
});

test('createTestHarness build registry từ deps', () => {
  const transport = {} as IHttpTransport;
  const registry = createTestHarness(
    { transport, baseUrl: 'http://x', authHeaders: { a: 'b' } },
    (deps) => ({ url: deps.baseUrl }),
  );
  assert.equal(registry.url, 'http://x');
});