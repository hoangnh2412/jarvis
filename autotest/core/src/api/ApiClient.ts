import type { IHttpTransport, HttpTransportResponse } from '../ports/IHttpTransport';

export interface RequestOptions {
  params?: Record<string, string | number>;
  body?: unknown;
}

export class ApiClientError extends Error {
  constructor(
    public readonly status: number,
    public readonly body: string,
    url: string,
  ) {
    super(`API ${url} trả về HTTP ${status}: ${body}`);
    this.name = 'ApiClientError';
  }
}

export class ApiClient {
  protected readonly transport: IHttpTransport;
  protected readonly baseUrl: string;
  protected readonly authHeaders: Record<string, string>;

  constructor(transport: IHttpTransport, baseUrl: string, authHeaders: Record<string, string>) {
    if (!baseUrl) {
      throw new Error('Thiếu cấu hình baseUrl');
    }
    this.transport = transport;
    this.baseUrl = baseUrl;
    this.authHeaders = authHeaders;
  }

  protected async get<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const url = this.buildUrl(path, options.params);
    const response = await this.transport.get(url, { headers: this.authHeaders });
    return this.parse<T>(response, url);
  }

  protected async post<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const url = this.buildUrl(path, options.params);
    const response = await this.transport.post(url, {
      headers: { ...this.authHeaders, 'Content-Type': 'application/json' },
      data: options.body,
    });
    return this.parse<T>(response, url);
  }

  protected buildUrl(path: string, params?: Record<string, string | number>): string {
    const url = new URL(`${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`);
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        url.searchParams.set(key, String(value));
      }
    }
    return url.toString();
  }

  private async parse<T>(response: HttpTransportResponse, url: string): Promise<T> {
    const text = await response.text();
    if (!response.ok()) {
      throw new ApiClientError(response.status(), text, url);
    }
    if (!text) {
      return null as T;
    }
    try {
      return JSON.parse(text) as T;
    } catch {
      throw new ApiClientError(response.status(), `Response không phải JSON: ${text.slice(0, 200)}`, url);
    }
  }
}
