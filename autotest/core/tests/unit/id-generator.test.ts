import { test } from 'node:test';
import assert from 'node:assert/strict';
import { newUniqueId } from '../../src/test-data/IdGenerator';

test('newUniqueId có prefix và không trùng trong hai lần gọi', () => {
  const a = newUniqueId('run');
  const b = newUniqueId('run');
  assert.match(a, /^run-\d+-[a-z0-9]+$/);
  assert.notEqual(a, b);
});
