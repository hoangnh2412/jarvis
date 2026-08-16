import { test } from 'node:test';
import assert from 'node:assert/strict';
import { pollUntil, delay } from '../../src/retry/pollUntil';

test('pollUntil trả về giá trị khi predicate đúng', async () => {
  let counter = 0;
  const result = await pollUntil(
    async () => ++counter,
    (v) => v === 3,
    { timeoutMs: 2000, intervalMs: 10, description: 'poll counter' },
  );
  assert.equal(result, 3);
});

test('pollUntil ném khi timeout hết', async () => {
  await assert.rejects(
    pollUntil(async () => 1, () => false, { timeoutMs: 50, intervalMs: 10, description: 'fail' }),
    /Hết thời gian chờ.*fail/,
  );
});

test('pollUntil retryOnError bỏ qua lỗi và chạy lại', async () => {
  let attempts = 0;
  const result = await pollUntil(
    async () => {
      attempts++;
      if (attempts <= 2) throw new Error('transient');
      return 'ok';
    },
    () => true,
    { timeoutMs: 2000, intervalMs: 10, retryOnError: () => true, description: 'retry' },
  );
  assert.equal(result, 'ok');
  assert.equal(attempts, 3);
});

test('pollUntil retryOnError false ném ngay lập tức', async () => {
  await assert.rejects(
    pollUntil(
      async () => { throw new Error('fatal'); },
      () => false,
      { timeoutMs: 1000, intervalMs: 10, retryOnError: () => false, description: 'stop' },
    ),
    /fatal/,
  );
});

test('delay resolves trong đúng khoảng thời gian', async () => {
  const start = Date.now();
  await delay(30);
  const elapsed = Date.now() - start;
  assert.ok(elapsed >= 20, `elapsed ${elapsed}ms quá thấp`);
});