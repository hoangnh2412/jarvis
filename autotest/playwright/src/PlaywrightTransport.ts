import type { APIRequestContext, APIResponse } from '@playwright/test';
import type {
  HttpTransportOptions,
  HttpTransportResponse,
  IHttpTransport,
} from '@jarvis/autotest';

function wrap(response: APIResponse): HttpTransportResponse {
  return {
    ok: () => response.ok(),
    status: () => response.status(),
    url: () => response.url(),
    text: () => response.text(),
  };
}

export class PlaywrightTransport implements IHttpTransport {
  constructor(private readonly request: APIRequestContext) {}

  async get(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.get(url, { headers: options?.headers });
    return wrap(response);
  }

  async post(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.post(url, {
      headers: options?.headers,
      data: options?.data,
    });
    return wrap(response);
  }
}

export function createPlaywrightTransport(request: APIRequestContext): PlaywrightTransport {
  return new PlaywrightTransport(request);
}
