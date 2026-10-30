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
    headers: () => response.headers(),
  };
}

function withBody(options?: HttpTransportOptions) {
  return {
    headers: options?.headers,
    data: options?.data,
  };
}

function withHeaders(options?: HttpTransportOptions) {
  return { headers: options?.headers };
}

export class PlaywrightTransport implements IHttpTransport {
  constructor(private readonly request: APIRequestContext) {}

  async get(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.get(url, withHeaders(options));
    return wrap(response);
  }

  async head(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.head(url, withHeaders(options));
    return wrap(response);
  }

  async post(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.post(url, withBody(options));
    return wrap(response);
  }

  async put(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.put(url, withBody(options));
    return wrap(response);
  }

  async patch(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.patch(url, withBody(options));
    return wrap(response);
  }

  async delete(url: string, options?: HttpTransportOptions): Promise<HttpTransportResponse> {
    const response = await this.request.delete(url, withBody(options));
    return wrap(response);
  }
}

export function createPlaywrightTransport(request: APIRequestContext): PlaywrightTransport {
  return new PlaywrightTransport(request);
}
