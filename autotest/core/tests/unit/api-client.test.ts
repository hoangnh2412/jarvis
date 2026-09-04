import { test } from 'node:test';
import assert from 'node:assert/strict';
import { ApiClient, ApiClientError, type RequestOptions } from '../../src/api/ApiClient';
import type { IHttpTransport, HttpTransportResponse } from '../../src/ports/IHttpTransport';

class FakeTransport implements IHttpTransport {
  calls: Array<{ method: string; url: string; options?: Record<string, unknown> }> = [];
  constructor(private readonly responder: (method: string, url: string) => HttpTransportResponse) {}
  async get(url: string, options?: Record<string, unknown>): Promise<HttpTransportResponse> {
    this.calls.push({ method: 'get', url, options });
    return this.responder('get', url);
  }
  async post(url: string, options?: Record<string, unknown>): Promise<HttpTransportResponse> {
    this.calls.push({ method: 'post', url, options });
    return this.responder('post', url);
  }
}

function jsonResponse(status: number, body: string): HttpTransportResponse {
  return {
    ok: () => status < 400,
    status: () => status,
    url: () => 'http://x',
    text: async () => body,
  };
}

test('buildUrl nối baseUrl với path và params query', async () => {
  const probe = new (class extends ApiClient {
    probeBuildUrl() {
      return (this as unknown as ApiClient & { buildUrl: (p: string, o?: unknown) => string }).buildUrl('/a/b', { id: '7', x: 1 });
    }
  })(new FakeTransport(() => jsonResponse(200, '{}')), 'http://h', {});
  const url = await probe.probeBuildUrl();
  assert.equal(url, 'http://h/a/b?id=7&x=1');
});

test('get trả về body JSON đã parse', async () => {
  const client = new (class extends ApiClient { getPublic<T>(p: string, o?: RequestOptions) { return this.get<T>(p, o); } })(
    new FakeTransport(() => jsonResponse(200, '{"a":1}')),
    'http://h',
    {},
  );
  const result = await client.getPublic<{ a: number }>('/x');
  assert.deepEqual(result, { a: 1 });
});

test('post thêm Content-Type JSON header', async () => {
  const transport = new FakeTransport(() => jsonResponse(200, '{}'));
  const client = new (class extends ApiClient { postPublic<T>(p: string, o?: RequestOptions) { return this.post<T>(p, o); } })(
    transport,
    'http://h',
    { 'X-Key': 'k' },
  );
  await client.postPublic('/x', { body: { b: 2 } });
  assert.equal(transport.calls[0].method, 'post');
  assert.equal((transport.calls[0].options as Record<string, unknown> & { headers: Record<string, string> }).headers['Content-Type'], 'application/json');
  assert.equal((transport.calls[0].options as Record<string, unknown> & { headers: Record<string, string> }).headers['X-Key'], 'k');
});

test('HTTP non-ok ném ApiClientError với status và body', async () => {
  const client = new (class extends ApiClient { getPublic<T>(p: string) { return this.get<T>(p); } })(
    new FakeTransport(() => jsonResponse(500, 'boom')),
    'http://h',
    {},
  );
  await assert.rejects(client.getPublic('/x'), (error: unknown) => {
    const err = error as ApiClientError;
    assert.equal(err.status, 500);
    assert.equal(err.body, 'boom');
    assert.equal(err.name, 'ApiClientError');
    return true;
  });
});

test('response non-JSON ném ApiClientError', async () => {
  const client = new (class extends ApiClient { getPublic<T>(p: string) { return this.get<T>(p); } })(
    new FakeTransport(() => jsonResponse(200, '<html>')),
    'http://h',
    {},
  );
  await assert.rejects(client.getPublic('/x'), /không phải JSON/i);
});

test('response rỗng trả null', async () => {
  const client = new (class extends ApiClient { getPublic<T>(p: string) { return this.get<T>(p); } })(
    new FakeTransport(() => jsonResponse(200, '')),
    'http://h',
    {},
  );
  const result = await client.getPublic<unknown>('/x');
  assert.equal(result, null);
});

test('constructor thiếu baseUrl ném Error', () => {
  assert.throws(() => new ApiClient(new FakeTransport(() => jsonResponse(200, '{}')), '', {}), /baseUrl/);
});